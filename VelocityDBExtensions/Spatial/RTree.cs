#if !NET2
using System;
using System.Collections.Generic;
#if !NET2
using System.Linq;
#endif
using System.Text;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.Session;

namespace VelocityDBExtensions.Spatial
{
  /// <summary>
  /// RTree implementation inspired by description on Wikipedia http://en.wikipedia.org/wiki/R-tree
  /// </summary>
  public class RTree : OptimizedPersistable
  {
    internal int maxNodeEntries;
    internal int minNodeEntries;
    /// <summary>
    /// default minimum number of reactangles in a leaf RTree node
    /// </summary>
    public const int defaultMinNodeEntries = 80;
    /// <summary>
    /// default maximum number of reactangles in a leaf RTree node
    /// </summary>
    public const int defaultMaxNodeEntries = 200;

    internal enum EntryStatus : byte { assigned = 0, unassigned = 1 };
    [NonSerialized]
    internal byte[] entryStatus;
    [NonSerialized]
    internal byte[] initialEntryStatus;
    [NonSerialized]
    internal Stack<NodeBase> parents = new Stack<NodeBase>();
    [NonSerialized]
    internal Stack<int> parentsEntry = new Stack<int>();

    internal int treeHeight = 1;
    private int size = 0;
    NodeBase rootNode;
    [NonSerialized]
    private List<Rectangle> savedValues = new List<Rectangle>();
    [NonSerialized]
    private double savedPriority = 0;
 
    /// <summary>
    /// RTree implementaion customized for optimal performance with VelocityDB. Follows outline as described in http://en.wikipedia.org/wiki/R-tree
    /// </summary>
    /// <param name="minNodeEntries">Minimum number of entries in a node. The default value is 80.</param>
    /// <param name="maxNodeEntries">Maximum number of entries in a node. The default value is 200.</param>
    public RTree(int minNodeEntries = defaultMinNodeEntries, int maxNodeEntries = defaultMaxNodeEntries)
    {
      this.minNodeEntries = minNodeEntries;
      this.maxNodeEntries = maxNodeEntries;

      if (this.maxNodeEntries < 2)
        this.maxNodeEntries = defaultMaxNodeEntries;

      if (this.minNodeEntries < 1 || this.minNodeEntries > this.maxNodeEntries / 2)
        this.minNodeEntries = this.maxNodeEntries / 3;

      entryStatus = new byte[maxNodeEntries];
      initialEntryStatus = new byte[maxNodeEntries];

      for (int i = 0; i < maxNodeEntries; i++)
        initialEntryStatus[i] = ((byte) EntryStatus.unassigned);

      rootNode = new NodeLeaf(1, this.maxNodeEntries);
    } 
 
    /// <summary>
    /// Start at the Root Node
    /// Select the child that needs the least enlargement in order to fit the new geometry.
    /// Repeat until at a leaf node.
    /// If leaf node has available space insert Else split the entry into two nodes
    /// Update parent nodes
    /// Update the entry that pointed to the node with a new minimum bounding rectangle
    /// Add a new entry for the second new node
    /// If there is no space in the parent node, split and repeat
    /// </summary>
    /// <param name="r">the rectangle being added</param>
    public void Add(Rectangle r)
    {
      AddInternal(r);
      size++;
#if RtreeCheck
      checkConsistency();
#endif
    }

    /// <summary>
    /// Adds a new entry at a specified level in the tree
    /// </summary>
    /// <param name="r">the rectangle added</param>
    internal void AddInternal(Rectangle r)
    {
      // I1 [Find position for new record] Invoke ChooseLeaf to select a leaf node L in which to place r
      NodeLeaf n = (NodeLeaf) chooseNode(r, 1);
      NodeLeaf newLeaf = null;

      // I2 [Add record to leaf node] If L has room for another entry, install E. Otherwise invoke SplitNode to obtain L and LL containing E and all the old entries of L
      if (n.entryCount < maxNodeEntries)
        n.addEntry(ref r);
      else
        newLeaf = n.splitNode(this, r);

      // I3 [Propagate changes upwards] Invoke AdjustTree on L, also passing LL if a split was performed
      NodeBase newNode = n.adjustTree(this, newLeaf);

      // I4 [Grow tree taller] If node split propagation caused the root to split, create a new root whose children are the two resulting nodes.
      if (newNode != null)
      {
        NodeBase oldRoot = rootNode;
        NodeInternal root = new NodeInternal(++treeHeight, maxNodeEntries);
        rootNode = root;
        root.addEntry(ref newNode.minimumBoundingRectangle, newNode);
        root.addEntry(ref oldRoot.minimumBoundingRectangle, oldRoot);
      }
    }

    /// <summary>
    /// Adds a new entry at a specified level in the tree
    /// </summary>
    /// <param name="r">the rectangle added</param>
    /// <param name="level">the level of the tree to add it at</param>
    internal void AddInternal(Rectangle r, int level, NodeBase childNode)
    {
      // I1 [Find position for new record] Invoke ChooseLeaf to select a leaf node L in which to place r
      NodeInternal n = (NodeInternal)chooseNode(r, level);
      NodeInternal newInternal = null;

      // I2 [Add record to leaf node] If L has room for another entry, install E. Otherwise invoke SplitNode to obtain L and LL containing E and all the old entries of L
      if (n.entryCount < maxNodeEntries)
        n.addEntry(ref r, childNode);
      else
        newInternal = n.splitNode(this, ref r, childNode);

      // I3 [Propagate changes upwards] Invoke AdjustTree on L, also passing LL if a split was performed
      NodeBase newNode = n.adjustTree(this, newInternal);

      // I4 [Grow tree taller] If node split propagation caused the root to split, create a new root whose children are the two resulting nodes.
      if (newNode != null)
      {
        NodeBase oldRoot = rootNode;
        NodeInternal root = new NodeInternal(++treeHeight, maxNodeEntries);
        rootNode = root;
        root.addEntry(ref newNode.minimumBoundingRectangle, newNode);
        root.addEntry(ref oldRoot.minimumBoundingRectangle, oldRoot);
      }
    }
   
    /// <summary>
    /// Removes a rectangle from the Rtree
    /// </summary>
    /// <param name="r">the rectangle to delete</param>
    /// <returns>true if rectangle deleted otherwise false</returns>
    public bool Remove(Rectangle r)
    {
      // FindLeaf algorithm inlined here. Note the "official" algorithm searches all overlapping entries. This seems inefficient, 
      // as an entry is only worth searching if it contains (NOT overlaps) the rectangle we are searching for.

      // FL1 [Search subtrees] If root is not a leaf, check each entry to determine if it contains r. For each entry found, invoke
      // findLeaf on the node pointed to by the entry, until r is found or all entries have been checked.
      parents.Clear();
      parents.Push(rootNode);

      parentsEntry.Clear();
      parentsEntry.Push(-1);
      NodeBase n = null;
      int foundIndex = -1; // index of entry to be deleted in leaf

      while (foundIndex == -1 && parents.Count > 0)
      {
        n = parents.Peek();
        int startIndex = parentsEntry.Peek() + 1;

        if (!n.IsLeaf)
        {
          NodeInternal internalNode = n as NodeInternal;
          bool Contains = false;
          for (int i = startIndex; i < n.entryCount; i++)
          {
            if (n.entries[i].Value.Contains(r))
            {
              parents.Push(internalNode.childNodes[i]);
              parentsEntry.Pop();
              parentsEntry.Push(i); // this becomes the start index when the child has been searched
              parentsEntry.Push(-1);
              Contains = true;
              break; // ie go to next iteration of while()
            }
          }
          if (Contains)
            continue;
        }
        else
        {
          NodeLeaf leaf = n as NodeLeaf;
          foundIndex = leaf.findEntry(ref r);
        }

        parents.Pop();
        parentsEntry.Pop();
      } // while not found

      if (foundIndex != -1)
      {
        NodeLeaf leaf = n as NodeLeaf;
        leaf.deleteEntry(foundIndex);
        leaf.condenseTree(this);
        size--;
      }

      // shrink the tree if possible (i.e. if root node has exactly one entry, and that entry is not a leaf node, delete the root (it's entry becomes the new root)
      NodeBase root = rootNode;
      while (root.entryCount == 1 && treeHeight > 1)
      {
        NodeInternal rootInternal = root as NodeInternal;
        root.entryCount = 0;
        rootNode = rootInternal.childNodes[0];
        treeHeight--;     
      }

      // if the tree is now empty, then set the MBR of the root node back to it's original state (this is only needed when the tree is empty,
      // as this is the only state where an empty node is not eliminated)
      if (size == 0)
        rootNode.minimumBoundingRectangle = new Rectangle(true);

#if RtreeCheck
      checkConsistency();
#endif

      return (foundIndex != -1);
    }

    /// <summary>
    /// Finds the nearest rectangles to the passed point. If multiple rectangles are equally near, they will all be returned.
    /// </summary>
    /// <param name="p">the point we are looking for</param>
    /// <param name="furthestDistance">The furthest distance away from the rectangle to search. Rectangles further than this will not be found.</param>
    /// <returns>a PriorityQue containing the found recatngles and their priorities (distances from point)</returns>
    public PriorityQueueRTree Nearest(Point p, double furthestDistance)
    {
      PriorityQueueRTree distanceQueue = new PriorityQueueRTree(); 

      double furthestDistanceSq = furthestDistance * furthestDistance;
      rootNode.Nearest(p, this, furthestDistanceSq, distanceQueue);
      return distanceQueue;
    }

    private PriorityQueueRTree createNearestNDistanceQueue(Point p, UInt32 count, double furthestDistance)
    {
      PriorityQueueRTree distanceQueue = new PriorityQueueRTree();

      //  return immediately if given an invalid "count" parameter
      if (count == 0)
        return distanceQueue;

      parents.Clear();
      parents.Push(rootNode);

      parentsEntry.Clear();
      parentsEntry.Push(-1);

      // TODO: possible shortcut here - could test for intersection with the MBR of the root node. If no intersection, return immediately.
      double furthestDistanceSq = furthestDistance * furthestDistance;

      while (parents.Count > 0)
      {
        NodeBase n = parents.Peek();
        int startIndex = parentsEntry.Peek() + 1;

        if (!n.IsLeaf)
        {
          // go through every entry in the index node to check if it could contain an entry closer than the farthest entry currently stored.
          bool near = false;
          NodeInternal nodeInternal = n as NodeInternal;
          for (int i = startIndex; i < n.entryCount; i++)
          {
            if (n.entries[i].Value.distanceSq(p.x, p.y) <= furthestDistanceSq)
            {
              parents.Push(nodeInternal.childNodes[i]);
              parentsEntry.Pop();
              parentsEntry.Push(i); // this becomes the start index when the child has been searched
              parentsEntry.Push(-1);
              near = true;
              break; // ie go to next iteration of while()
            }
          }
          if (near)
            continue;
        }
        else
        {
          // go through every entry in the leaf to check if it is currently one of the nearest N entries.
          for (int i = 0; i < n.entryCount; i++)
          {
            double entryDistanceSq = n.entries[i].Value.distanceSq(p.x, p.y);

            if (entryDistanceSq <= furthestDistanceSq)
            {
              distanceQueue.Insert(n.entries[i].Value, entryDistanceSq);

              while (distanceQueue.Count > count)
              {
                // normal case - we can simply remove the lowest priority (highest distance) entry
                Rectangle value = distanceQueue.ValuePeek;
                double distanceSq = distanceQueue.PriorityPeek;
                distanceQueue.Pop();

                // rare case - multiple items of the same priority (distance)
                if (distanceSq == distanceQueue.PriorityPeek)
                {
                  savedValues.Add(value);
                  savedPriority = distanceSq;
                }
                else
                  savedValues.Clear();
              }

              // if the saved values have the same distance as the next one in the tree, add them back in.
              if (savedValues.Count > 0 && savedPriority == distanceQueue.PriorityPeek)
              {
                for (int svi = 0; svi < savedValues.Count; svi++)
                  distanceQueue.Insert(savedValues[svi], savedPriority);
                savedValues.Clear();
              }

              // narrow the search, if we have already found N items
              if (distanceQueue.PriorityPeek < furthestDistanceSq && distanceQueue.Count >= count)
                furthestDistanceSq = distanceQueue.PriorityPeek;
            }
          }
        }
        parents.Pop();
        parentsEntry.Pop();
      }
      return distanceQueue;
    }

    /// <summary>
    /// Finds the N nearest rectangles to the passed point. If multiple rectangles are equally near, they will all (but total limited to N) be returned.
    /// </summary>
    /// <param name="p">the point we are looking for</param>
    /// <param name="count">max number of rectangles to look for</param>
    /// <param name="furthestDistance">The furthest distance away from the rectangle to search. Rectangles further than this will not be found.</param>
    /// <returns>a PriorityQue containing the found recatngles and their priorities (distances from point)</returns>
    public PriorityQueueRTree NearestN(Point p, UInt32 count, double furthestDistance)
    {
      return createNearestNDistanceQueue(p, count, furthestDistance);
    }
  
    /// <summary>
    /// Finds all rectangles that intersect the passed rectangle.
    /// </summary>
    /// <param name="r">the rectangle we are intersecting with</param>
    /// <param name="v">if returns true, search containues else search is ended</param>
    /// <returns>true if at least one intersection was found and v returns true</returns>
    public bool Intersects(Rectangle r, Func<Rectangle, bool> v)
    {
      return intersects(r, v, rootNode);
    }

    /// <summary>
    /// Finds all rectangles contained by the passed rectangle
    /// </summary>
    /// <param name="r">The rectangle for which this method finds contained rectangles.</param>
    /// <param name="v">if return true, continue seach</param>
    public void Contains(Rectangle r, Func<Rectangle, bool> v)
    {
      // find all rectangles in the tree that are contained by the passed rectangle written to be non-recursive (should model other searches on this?)
      parents.Clear();
      parents.Push(rootNode);

      parentsEntry.Clear();
      parentsEntry.Push(-1);

      // TODO: possible shortcut here - could test for intersection with the MBR of the root node. If no intersection, return immediately.
      while (parents.Count > 0)
      {
        NodeBase n = parents.Peek();
        int startIndex = parentsEntry.Peek() + 1;

        if (!n.IsLeaf)
        {
          NodeInternal nodeInternal = n as NodeInternal;
          // go through every entry in the index node to check if it intersects the passed rectangle. If so, it could contain entries that are contained.
          bool intersects = false;
          for (int i = startIndex; i < n.entryCount; i++)
          {
            if (r.Intersects(ref n.entries[i]))
            {
              parents.Push(nodeInternal.childNodes[i]);
              parentsEntry.Pop();
              parentsEntry.Push(i); // this becomes the start index when the child has been searched
              parentsEntry.Push(-1);
              intersects = true;
              break; // ie go to next iteration of while()
            }
          }
          if (intersects)
          {
            continue;
          }
        }
        else
        {
          // go through every entry in the leaf to check if it is contained by the passed rectangle
          for (int i = 0; i < n.entryCount; i++)
          {
            if (r.Contains(n.entries[i].Value))
            {
              if (!v(n.entries[i].Value))
              {
                return;
              }
            }
          }
        }
        parents.Pop();
        parentsEntry.Pop();
      }
    }

    /// <summary>
    /// Get the number of rectangles that are managed by the Rtree
    /// </summary>
    public int Count
    {
      get
      {
        return size;
      }
    }
 
    /// <summary>
    /// Gets the bounds of all the entries in the spatial index, or null if there are no entries.
    /// </summary>
    public Rectangle? Bounds
    {
      get
      {
        NodeBase n = rootNode;
        if (n != null && n.entryCount > 0)
          return n.minimumBoundingRectangle;
        return null;
      }
    }

    /// <summary>
    /// Recursively searches the tree for all intersecting entries.
    /// Calls the passed function when a matching entry is found. Return if the passed function returns false;
    /// </summary>
    /// <param name="r"></param>
    /// <param name="v"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    // TODO rewrite this to be non-recursive.
    private bool intersects(Rectangle r, Func<Rectangle, bool> v, NodeBase n)
    {
      for (int i = 0; i < n.entryCount; i++)
      {
        if (r.Intersects(ref n.entries[i]))
        {
          if (n.IsLeaf)
          {
            if (!v(n.entries[i].Value))
            {
              return false;
            }
          }
          else
          {
            NodeInternal nodeInternal = n as NodeInternal;
            NodeInternal childNode = nodeInternal.childNodes[i] as NodeInternal;
            if (!intersects(r, v, childNode))
            {
              return false;
            }
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Used by add(). Chooses a leaf to add the rectangle to.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="level"></param>
    /// <returns></returns> 
    private NodeBase chooseNode(Rectangle r, int level)
    {
      // CL1 [Initialize] Set N to be the root node
      NodeBase n = rootNode;
      parents.Clear();
      parentsEntry.Clear();

      // CL2 [Leaf check] If N is a leaf, return N
      while (true)
      {
        if (n == null)
          throw new UnexpectedException("Could not get root node (" + rootNode + ")");

        if (n.level == level)
          return n;

        NodeInternal nodeInternal = n as NodeInternal;
        // CL3 [Choose subtree] If N is not at the desired level, let F be the entry in N whose rectangle FI needs least enlargement to include EI. Resolve
        // ties by choosing the entry with the rectangle of smaller area.
        double leastEnlargement = n.entries[0].Value.Enlargement(ref r);
        int index = 0; // index of rectangle in subtree
        for (int i = 1; i < n.entryCount; i++)
        {
          double tempEnlargement = n.entries[i].Value.Enlargement(ref r);
          if (tempEnlargement < leastEnlargement || (tempEnlargement == leastEnlargement && n.entries[i].Value.Area < n.entries[index].Value.Area))
          {
            index = i;
            leastEnlargement = tempEnlargement;
          }
        }

        parents.Push(n);
        parentsEntry.Push(index);

        // CL4 [Descend until a leaf is reached] Set N to be the child node pointed to by Fp and repeat from CL2
        n = nodeInternal.childNodes[index];
      }
    }

    /// <summary>
    /// Check the consistency of the tree.
    /// </summary>
    /// <returns>false if an inconsistency is detected, true otherwise</returns> 
    public bool checkConsistency()
    {
      return checkConsistency(rootNode, treeHeight, null);
    }

    private bool checkConsistency(NodeBase n, int expectedLevel, Rectangle? expectedMBR)
    {
      // go through the tree, and check that the internal data structures of the tree are not corrupted.        

      if (n == null)
        throw new UnexpectedException($"Error: Could not read node {this}");

      // if tree is empty, then there should be exactly one node, at level 1
      // TODO: also check the MBR is as for a new node
      if (n == rootNode && Count == 0)
      {
        if (n.level != 1)
          throw new UnexpectedException("Error: tree is empty but root node is not at level 1");
      }

      if (n.level != expectedLevel)
        throw new UnexpectedException("Error: Node " + this + ", expected level " + expectedLevel + ", actual level " + n.level);

      Rectangle calculatedMBR = n.calculateMinimumBoundingRectangle();
      Rectangle actualMBR = n.minimumBoundingRectangle;
      if (!actualMBR.Equals(calculatedMBR))
      {
        if (actualMBR.MinX != n.minimumBoundingRectangle.MinX)
          throw new UnexpectedException($"  actualMinX={actualMBR.MinX}, calc={calculatedMBR.MinX}");
        if (actualMBR.MinY != n.minimumBoundingRectangle.MinY)
          throw new UnexpectedException($"  actualMinY={actualMBR.MinY}, calc={calculatedMBR.MinY}");
        if (actualMBR.MaxX != n.minimumBoundingRectangle.MaxX)
          throw new UnexpectedException($"  actualMaxX={actualMBR.MaxX}, calc={calculatedMBR.MaxX}");
        if (actualMBR.MaxY != n.minimumBoundingRectangle.MaxY)
          throw new UnexpectedException($"  actualMaxY={actualMBR.MaxY}, calc={calculatedMBR.MaxY}");
        throw new UnexpectedException("Error: Node " + this + ", calculated MBR does not equal stored MBR");
      }

      if (expectedMBR != null && !actualMBR.Equals(expectedMBR))
        throw new UnexpectedException("Error: Node " + this + ", expected MBR (from parent) does not equal stored MBR");

      for (int i = 0; i < n.entryCount; i++)
      {
        if (n.level > 1) // if not a leaf
        {
          NodeInternal nodeInternal = n as NodeInternal;
          if (nodeInternal.childNodes[i] == null)
            throw new UnexpectedException("Error: Node " + this + ", Entry " + i + " is null");
          if (!checkConsistency(nodeInternal.childNodes[i], n.level - 1, n.entries[i]))
          {
            return false;
          }
        }
      }
      return true;
    }

    public override void InitializeAfterRead(SessionBase session)
    {
      base.InitializeAfterRead(session);
      entryStatus = new byte[maxNodeEntries];
      initialEntryStatus = new byte[maxNodeEntries];

      for (int i = 0; i < maxNodeEntries; i++)
        initialEntryStatus[i] = ((byte)EntryStatus.unassigned);
    }
  }
}
#endif