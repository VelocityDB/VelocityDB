#if !NET2
using System;
using System.Collections.Generic;
#if !NET2
using System.Linq;
#endif
using System.Text;
using VelocityDb;
using VelocityDb.Exceptions;

namespace VelocityDBExtensions.Spatial
{
  internal class NodeLeaf : NodeBase
  {
    internal NodeLeaf(int level, int maxNodeEntries)
    {
      this.level = level;
      entries = new Rectangle?[maxNodeEntries];
    }

    internal NodeBase adjustTree(RTree rTree, NodeLeaf nn)
    {
      // AT1 [Initialize] Set N=L. If L was split previously, set NN to be the resulting second node.

      // AT2 [Check if done] If N is the root, stop
      while (level != rTree.treeHeight)
      {
        // AT3 [Adjust covering rectangle in parent entry] Let P be the parent node of N, and let En be N's entry in P. Adjust EnI so that it tightly encloses all entry rectangles in N.
        NodeInternal parent = rTree.parents.Pop() as NodeInternal;
        int entry = rTree.parentsEntry.Pop();

        if (parent.childNodes[entry] != this)
          throw new UnexpectedException("Error: entry " + entry + " in node " + parent + " should point to node " + this + "; actually points to node " + parent.childNodes[entry]);
        if (parent.entries[entry].Value.MinX != minimumBoundingRectangle.MinX || parent.entries[entry].Value.MinY != minimumBoundingRectangle.MinY ||
            parent.entries[entry].Value.MaxX != minimumBoundingRectangle.MaxX || parent.entries[entry].Value.MaxY != minimumBoundingRectangle.MaxY)
        {
          parent.entries[entry] = minimumBoundingRectangle;
          parent.recalculateMBR();
        }

        // AT4 [Propagate node split upward] If N has a partner NN resulting from an earlier split, create a new entry Enn with Ennp pointing to NN and 
        // Enni enclosing all rectangles in NN. Add Enn to P if there is room. Otherwise, invoke splitNode to produce P and PP containing Enn and all P's old entries.
        NodeInternal newNode = null;
        if (nn != null)
        {
          if (parent.entryCount < rTree.maxNodeEntries)
            parent.addEntry(ref nn.minimumBoundingRectangle, nn);
          else
            newNode = parent.splitNode(rTree, ref nn.minimumBoundingRectangle, nn);
        }
        // AT5 [Move up to next level] Set N = P and set NN = PP if a split occurred. Repeat from AT2
        return parent.adjustTree(rTree, newNode);
      }
      return nn;
    }

    internal override bool IsLeaf
    {
      get
      {
        return true;
      }
    }

    private int pickNext(RTree rTree, NodeLeaf newNode)
    {
      double maxDifference = double.NegativeInfinity;
      int next = 0;
      int nextGroup = 0;
      maxDifference = double.NegativeInfinity;
#if RtreeCheck
        Console.WriteLine("pickNext()");
#endif
      for (int i = 0; i < rTree.maxNodeEntries; i++)
      {
        if (rTree.entryStatus[i] == ((byte)RTree.EntryStatus.unassigned))
        {
          if (entries[i] == null)
            throw new UnexpectedException("Error: Node " + this + ", entry " + i + " is null");
          Rectangle entryR = entries[i].Value;
          double nIncrease = minimumBoundingRectangle.Enlargement(ref entryR);
          double newNodeIncrease = newNode.minimumBoundingRectangle.Enlargement(ref entryR);
          double difference = Math.Abs(nIncrease - newNodeIncrease);
          if (difference > maxDifference)
          {
            next = i;
            if (nIncrease < newNodeIncrease)
              nextGroup = 0;
            else if (newNodeIncrease < nIncrease)
              nextGroup = 1;
            else if (minimumBoundingRectangle.Area < newNode.minimumBoundingRectangle.Area)
              nextGroup = 0;
            else if (newNode.minimumBoundingRectangle.Area < minimumBoundingRectangle.Area)
              nextGroup = 1;
            else if (newNode.entryCount < rTree.maxNodeEntries / 2)
              nextGroup = 0;
            else
              nextGroup = 1;
            maxDifference = difference;
          }
#if RtreeCheck
            Console.WriteLine("Entry " + i + " group0 increase = " + nIncrease + ", group1 increase = " + newNodeIncrease + ", diff = " + difference + ", MaxDiff = " + maxDifference + " (entry " + next + ")");
#endif
        }
      }
      rTree.entryStatus[next] = ((byte)RTree.EntryStatus.assigned);
      if (nextGroup == 0)
      {
        Update();
        Rectangle r = entries[next].Value;
        if (r.MinX < minimumBoundingRectangle.MinX)
          minimumBoundingRectangle.MinX = r.MinX;
        if (r.MinY < minimumBoundingRectangle.MinY)
          minimumBoundingRectangle.MinY = r.MinY;
        if (r.MaxX > minimumBoundingRectangle.MaxX)
          minimumBoundingRectangle.MaxX = r.MaxX;
        if (r.MaxY > minimumBoundingRectangle.MaxY)
          minimumBoundingRectangle.MaxY = r.MaxY;
        entryCount++;
      }
      else
      {
        // move to new node.
        Rectangle r = entries[next].Value;
        newNode.addEntry(ref r);
        entries[next] = null;
      }
      return next;
    }

    private void pickSeeds(RTree rTree, ref Rectangle r, NodeLeaf newNode)
    {
      // Find extreme rectangles along all dimension. Along each dimension, find the entry whose rectangle has the highest low side, and the one 
      // with the lowest high side. Record the separation.
      double maxNormalizedSeparation = -1; // initialize to -1 so that even overlapping rectangles will be considered for the seeds
      int highestLowIndex = -1;
      int lowestHighIndex = -1;
      Update();
      // for the purposes of picking seeds, take the MBR of the node to include the new rectangle aswell.
      if (r.MinX < minimumBoundingRectangle.MinX)
        minimumBoundingRectangle.MinX = r.MinX;
      if (r.MinY < minimumBoundingRectangle.MinY)
        minimumBoundingRectangle.MinY = r.MinY;
      if (r.MaxX > minimumBoundingRectangle.MaxX)
        minimumBoundingRectangle.MaxX = r.MaxX;
      if (r.MaxY > minimumBoundingRectangle.MaxY)
        minimumBoundingRectangle.MaxY = r.MaxY;

      double mbrLenX = minimumBoundingRectangle.MaxX - minimumBoundingRectangle.MinX;
      double mbrLenY = minimumBoundingRectangle.MaxY - minimumBoundingRectangle.MinY;

#if RtreeCheck
        Console.WriteLine("pickSeeds(): Node = " + this);
#endif

      double tempHighestLow = r.MinX;
      int tempHighestLowIndex = -1; // -1 indicates the new rectangle is the seed

      double tempLowestHigh = r.MaxX;
      int tempLowestHighIndex = -1; // -1 indicates the new rectangle is the seed

      for (int i = 0; i < entryCount; i++)
      {
        double tempLow = entries[i].Value.MinX;
        if (tempLow >= tempHighestLow)
        {
          tempHighestLow = tempLow;
          tempHighestLowIndex = i;
        } // ensure that the same index cannot be both lowestHigh and highestLow
        else
        {
          double tempHigh = entries[i].Value.MaxX;
          if (tempHigh <= tempLowestHigh)
          {
            tempLowestHigh = tempHigh;
            tempLowestHighIndex = i;
          }
        }

        // PS2 [Adjust for shape of the rectangle cluster] Normalize the separations by dividing by the widths of the entire set along the corresponding dimension
        double normalizedSeparation = mbrLenX == 0 ? 1 : (tempHighestLow - tempLowestHigh) / mbrLenX;
        if (normalizedSeparation > 1 || normalizedSeparation < -1)
        {
          Console.WriteLine("Invalid normalized separation X");
        }

#if RtreeCheck
          Console.WriteLine("Entry " + i + ", dimension X: HighestLow = " + tempHighestLow + " (index " + tempHighestLowIndex + ")" + ", LowestHigh = " + tempLowestHigh + " (index " + tempLowestHighIndex + ", NormalizedSeparation = " + normalizedSeparation);
#endif
        // PS3 [Select the most extreme pair] Choose the pair with the greatest normalized separation along any dimension.
        // Note that if negative it means the rectangles overlapped. However still include overlapping rectangles if that is the only choice available.
        if (normalizedSeparation >= maxNormalizedSeparation)
        {
          highestLowIndex = tempHighestLowIndex;
          lowestHighIndex = tempLowestHighIndex;
          maxNormalizedSeparation = normalizedSeparation;
        }
      }

      // Repeat for the Y dimension
      tempHighestLow = r.MinY;
      tempHighestLowIndex = -1; // -1 indicates the new rectangle is the seed

      tempLowestHigh = r.MaxY;
      tempLowestHighIndex = -1; // -1 indicates the new rectangle is the seed

      for (int i = 0; i < entryCount; i++)
      {
        double tempLow = entries[i].Value.MinY;
        if (tempLow >= tempHighestLow)
        {
          tempHighestLow = tempLow;
          tempHighestLowIndex = i;
        } // ensure that the same index cannot be both lowestHigh and highestLow
        else
        {
          double tempHigh = entries[i].Value.MaxY;
          if (tempHigh <= tempLowestHigh)
          {
            tempLowestHigh = tempHigh;
            tempLowestHighIndex = i;
          }
        }

        // PS2 [Adjust for shape of the rectangle cluster] Normalize the separations by dividing by the widths of the entire set along the corresponding dimension
        double normalizedSeparation = mbrLenY == 0 ? 1 : (tempHighestLow - tempLowestHigh) / mbrLenY;
        if (normalizedSeparation > 1 || normalizedSeparation < -1)
        {
          throw new UnexpectedException("Invalid normalized separation Y");
        }
#if RtreeCheck
          Console.WriteLine("Entry " + i + ", dimension Y: HighestLow = " + tempHighestLow + " (index " + tempHighestLowIndex + ")" + ", LowestHigh = " + tempLowestHigh + " (index " + tempLowestHighIndex + ", NormalizedSeparation = " + normalizedSeparation);
#endif
        // PS3 [Select the most extreme pair] Choose the pair with the greatest normalized separation along any dimension.
        // Note that if negative it means the rectangles overlapped. However still include overlapping rectangles if that is the only choice available.
        if (normalizedSeparation >= maxNormalizedSeparation)
        {
          highestLowIndex = tempHighestLowIndex;
          lowestHighIndex = tempLowestHighIndex;
          maxNormalizedSeparation = normalizedSeparation;
        }
      }

      // At this point it is possible that the new rectangle is both highestLow and lowestHigh. This can happen if all rectangles in the node overlap the new rectangle.
      // Resolve this by declaring that the highestLowIndex is the lowest Y and, the lowestHighIndex is the largest X (but always a different rectangle)
      if (highestLowIndex == lowestHighIndex)
      {
        highestLowIndex = -1;
        double tempMinY = r.MinY;
        lowestHighIndex = 0;
        double tempMaxX = entries[0].Value.MaxX;

        for (int i = 1; i < entryCount; i++)
        {
          if (entries[i].Value.MinY < tempMinY)
          {
            tempMinY = entries[i].Value.MinY;
            highestLowIndex = i;
          }
          else if (entries[i].Value.MaxX > tempMaxX)
          {
            tempMaxX = entries[i].Value.MaxX;
            lowestHighIndex = i;
          }
        }
      }

      // highestLowIndex is the seed for the new node.
      if (highestLowIndex == -1)
        newNode.addEntry(ref r);
      else
      {
        Rectangle entryRectangle = entries[highestLowIndex].Value;
        newNode.addEntry(ref entryRectangle);
        entries[highestLowIndex] = r;  // move the new rectangle into the space vacated by the seed for the new node
      }

      // lowestHighIndex is the seed for the original node. 
      if (lowestHighIndex == -1)
        lowestHighIndex = highestLowIndex;

      rTree.entryStatus[lowestHighIndex] = ((byte)RTree.EntryStatus.assigned);
      entryCount = 1;
      minimumBoundingRectangle = entries[lowestHighIndex].Value;
    }

    internal override void reorganize(RTree rtree)
    {
      int countdownIndex = rtree.maxNodeEntries - 1;
      for (int index = 0; index < entryCount; index++)
      {
        if (entries[index] == null)
        {
          while (entries[countdownIndex] == null && countdownIndex > index)
            countdownIndex--;
          entries[index] = entries[countdownIndex];
          entries[countdownIndex] = null;
        }
      }
    }

    /// <summary>
    /// Used by delete(). Ensures that all nodes from the passed node up to the root have the minimum number of entries.
    /// Note that the parent and parentEntry stacks are expected to contain the nodeIds of all parents up to the root.
    /// </summary>
    /// <param name="rTree"></param>
    internal void condenseTree(RTree rTree)
    {
      // CT1 [Initialize] Set n=l. Set the list of eliminated nodes to be empty.
      NodeBase n = this;
      NodeInternal parent = null;
      int parentEntry = 0;
      Stack<NodeBase> eliminatedNodes = new Stack<NodeBase>();

      // CT2 [Find parent entry] If N is the root, go to CT6. Otherwise 
      // let P be the parent of N, and let En be N's entry in P  
      while (n.level != rTree.treeHeight)
      {
        parent = rTree.parents.Pop() as NodeInternal;
        parentEntry = rTree.parentsEntry.Pop();

        // CT3 [Eliminiate under-full node] If N has too few entries,
        // delete En from P and add N to the list of eliminated nodes
        if (n.entryCount < rTree.minNodeEntries)
        {
          parent.deleteEntry(parentEntry);
          eliminatedNodes.Push(n);
        }
        else
        {
          // CT4 [Adjust covering rectangle] If N has not been eliminated,
          // adjust EnI to tightly contain all entries in N
          if (n.minimumBoundingRectangle.MinX != parent.entries[parentEntry].Value.MinX || n.minimumBoundingRectangle.MinY != parent.entries[parentEntry].Value.MinY ||
              n.minimumBoundingRectangle.MaxX != parent.entries[parentEntry].Value.MaxX || n.minimumBoundingRectangle.MaxY != parent.entries[parentEntry].Value.MaxY)
          {
            Rectangle d = parent.entries[parentEntry].Value;
            parent.entries[parentEntry] = n.minimumBoundingRectangle;
            parent.recalculateMBRIfInfluencedBy(ref d);
          }
        }
        // CT5 [Move up one level in tree] Set N=P and repeat from CT2
        n = parent;
      }

      // CT6 [Reinsert orphaned entries] Reinsert all entries of nodes in set Q. Entries from eliminated leaf nodes are reinserted in tree leaves as in 
      // Insert(), but entries from higher level nodes must be placed higher in the tree, so that leaves of their dependent subtrees will be on the same
      // level as leaves of the main tree
      while (eliminatedNodes.Count > 0)
      {
        NodeBase e = eliminatedNodes.Pop();
        for (int j = 0; j < e.entryCount; j++)
        {
            if (e.level == 1)
              rTree.AddInternal(e.entries[j].Value);
            else
            {
              NodeInternal nInternal = e as NodeInternal;
              rTree.AddInternal(e.entries[j].Value, e.level, nInternal.childNodes[j]);
            }
        }
      }
    }

    // delete entry. This is done by setting it to null and copying the last entry into its space.
    internal virtual void deleteEntry(int i)
    {
      int lastIndex = entryCount - 1;
      Update();
      Rectangle d = entries[i].Value;

      if (i != lastIndex)
        entries[i] = entries[lastIndex];
      entryCount--;

      // adjust the MBR
      if (entryCount > 0)
        recalculateMBRIfInfluencedBy(ref d);
    }

    // Return the index of the found entry, or -1 if not found
    internal virtual int findEntry(ref Rectangle r)
    {
      for (int i = 0; i < entryCount; i++)
      {
        if (entries[i].Value.Equals(r))
          return i;
      }
      return -1;
    }

    internal override double Nearest(Point p, RTree rTree, double furthestDistanceSq, PriorityQueueRTree nearestRectangles)
    {
      for (int i = 0; i < entryCount; i++)
      {
        double tempDistanceSq = entries[i].Value.distanceSq(p.x, p.y);
        if (tempDistanceSq < furthestDistanceSq)
        {
          furthestDistanceSq = tempDistanceSq;
          nearestRectangles.Clear();
        }
        if (tempDistanceSq <= furthestDistanceSq)
          nearestRectangles.Insert(entries[i].Value, tempDistanceSq);
      }
      return furthestDistanceSq;
    }

    internal NodeLeaf splitNode(RTree rTree, Rectangle r)
    {
      // [Pick first entry for each group] Apply algorithm pickSeeds to 
      // choose two entries to be the first elements of the groups. Assign
      // each to a group.

      // debug code
      /*double initialArea = 0;
           if (log.isDebugEnabled())
           {
             double unionMinX = Math.Min(n.mbrMinX, newRectMinX);
             double unionMinY = Math.Min(n.mbrMinY, newRectMinY);
             double unionMaxX = Math.Max(n.mbrMaxX, newRectMaxX);
             double unionMaxY = Math.Max(n.mbrMaxY, newRectMaxY);

             initialArea = (unionMaxX - unionMinX) * (unionMaxY - unionMinY);
           }*/

      System.Array.Copy(rTree.initialEntryStatus, 0, rTree.entryStatus, 0, rTree.maxNodeEntries);
      Update();
      NodeLeaf newNode = null;
      newNode = new NodeLeaf(level, rTree.maxNodeEntries);

      pickSeeds(rTree, ref r, newNode); // this also sets the entryCount to 1

      // [Check if done] If all entries have been assigned, stop. If one group has so few entries that all the rest must be assigned to it in 
      // order for it to have the minimum number m, assign them and stop. 
      while (entryCount + newNode.entryCount < rTree.maxNodeEntries + 1)
      {
        if (rTree.maxNodeEntries + 1 - newNode.entryCount == rTree.minNodeEntries)
        {
          // assign all remaining entries to original node
          for (int i = 0; i < rTree.maxNodeEntries; i++)
          {
            if (rTree.entryStatus[i] == ((byte)RTree.EntryStatus.unassigned))
            {
              rTree.entryStatus[i] = ((byte)RTree.EntryStatus.assigned);

              if (entries[i].Value.MinX < minimumBoundingRectangle.MinX)
                minimumBoundingRectangle.MinX = entries[i].Value.MinX;
              if (entries[i].Value.MinY < minimumBoundingRectangle.MinY)
                minimumBoundingRectangle.MinY = entries[i].Value.MinY;
              if (entries[i].Value.MaxX > minimumBoundingRectangle.MaxX)
                minimumBoundingRectangle.MaxX = entries[i].Value.MaxX;
              if (entries[i].Value.MaxY > minimumBoundingRectangle.MaxY)
                minimumBoundingRectangle.MaxY = entries[i].Value.MaxY;
              entryCount++;
            }
          }
          break;
        }
        if (rTree.maxNodeEntries + 1 - entryCount == rTree.minNodeEntries)
        {
          // assign all remaining entries to new node
          for (int i = 0; i < rTree.maxNodeEntries; i++)
          {
            if (rTree.entryStatus[i] == ((byte)RTree.EntryStatus.unassigned))
            {
              rTree.entryStatus[i] = ((byte)RTree.EntryStatus.assigned);
              Rectangle entryR = entries[i].Value;
              newNode.addEntry(ref entryR);
              entries[i] = null;
            }
          }
          break;
        }

        // [Select entry to assign] Invoke algorithm pickNext to choose the next entry to assign. Add it to the group whose covering rectangle 
        // will have to be enlarged least to accommodate it. Resolve ties by adding the entry to the group with smaller area, then to the 
        // the one with fewer entries, then to either. Repeat from S2
        pickNext(rTree, newNode);
      }

      reorganize(rTree);

      // check that the MBR stored for each node is correct.
#if RtreeCheck
        if (!this.minimumBoundingRectangle.Equals(calculateMBR()))
        {
          throw new UnexpectedException("Error: splitNode old node MBR wrong");
        }
        if (!newNode.minimumBoundingRectangle.Equals(newNode.calculateMBR()))
        {
          throw new UnexpectedException("Error: splitNode new node MBR wrong");
        }
#endif

#if RtreeCheck
        double newArea = minimumBoundingRectangle.Area + newNode.minimumBoundingRectangle.Area;
        double percentageIncrease = (100 * (newArea - initialArea)) / initialArea;
        Console.WriteLine("Node " + this + " split. New area increased by " + percentageIncrease + "%");
#endif

      return newNode;
    }
  }
}
#endif