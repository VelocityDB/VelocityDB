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
  //
  // Priority Queue that stores values and priorities. Uses a Heap to sort the priorities; the values are sorted "in step" with the priorities.
  // A Heap is simply an array that is kept semi sorted; in particular if the elements of the array are arranged in a tree structure; ie
  //                                   00 
  //                                 /     \ 
  //                             01          02 
  //                            /  \        /  \ 
  //                          03    04    05    06 
  //                          /\    /\    /\    /\ 
  //                       07 08 09 10 11 12 13 14
  //
  // then each parent is kept sorted with respect to it's immediate children.
  // This means that the array appears to be sorted, as long as we only ever look at element 0.
  // Inserting new elements is much faster than if the entire array was kept sorted; a new element is appended to the array, and then recursively swapped
  // with each parent to maintain the "parent is sorted w.r.t it's children" property.
  // 
  // To return the "next" value it is necessary to remove the root element. The last element in the array is placed in the root of the tree, and is
  // recursively swapped with one of it's children until the "parent is sorted w.r.t it's children" property is restored.
  // 
  // Random access is slow (eg for deleting a particular value), and is not implemented here - if this functionality is required, then a heap probably
  // isn't the right data structure.
  //
 
  /// <summary>
  /// Priority Queue that stores values and priorities.
  /// </summary>
  public class PriorityQueue<Priority, Value> where Priority : IComparable
  {
    private List<Value> values;
    private List<Priority> priorities;
    bool sortOrderAscending;

    private static bool INTERNAL_CONSISTENCY_CHECKING = false;

    /// <summary>
    /// Creates a PriorityQue
    /// </summary>
    /// <param name="sortOrderAscending">order ascending if true otherwise order descending</param>
    /// <param name="initialCapacity">improve performance by setting expected size initially</param>
    public PriorityQueue(bool sortOrderAscending = true, int initialCapacity = 25)
    {
      this.sortOrderAscending = sortOrderAscending;
      values = new List<Value>(initialCapacity);
      priorities = new List<Priority>(initialCapacity);
    }

    /// <summary>
    /// Determine if p1 should be ordered before p2 or vice versa
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns>true if p1 has an earlier sort order than p2</returns> 
    private bool sortsEarlierThan(Priority p1, Priority p2)
    {
      if (sortOrderAscending)
        return p1.CompareTo(p2) < 0;
      return p2.CompareTo(p1) < 0;
    }

    /// <summary>
    /// Insert a value, append it to the arrays, then reheapify by promoting it to the correct place.
    /// </summary>
    /// <param name="value">the value inserted</param>
    /// <param name="priority">the priority of the value inserted</param>
    public virtual void Insert(Value value, Priority priority)
    {
      values.Add(value);
      priorities.Add(priority);

      promote(values.Count - 1, value, priority);
    }

    private void promote(int index, Value value, Priority priority)
    {
      // Consider the index to be a "hole"; i.e. don't swap priorities/values when moving up the tree, simply copy the parent into the hole and
      // then consider the parent to be the hole. Finally, copy the value/priority into the hole.
      while (index > 0)
      {
        int parentIndex = (index - 1) / 2;
        Priority parentPriority = priorities[parentIndex];

        if (sortsEarlierThan(parentPriority, priority))
        {
          break;
        }

        // copy the parent entry into the current index.
        values[index] = values[parentIndex];
        priorities[index] = parentPriority;
        index = parentIndex;
      }

      values[index] = value;
      priorities[index] = priority;

      if (INTERNAL_CONSISTENCY_CHECKING)
      {
        check();
      }
    }

    /// <summary>
    /// The number of values and priorities that are in the que
    /// </summary>
    public virtual int Count
    {
      get
      {
        return values.Count;
      }
    }

    /// <summary>
    /// Removes all values and priorities from the que
    /// </summary>
    public void Clear()
    {
      values.Clear();
      priorities.Clear();
    }

    /// <summary>
    /// Peek at the next value
    /// </summary>
    public Value ValuePeek
    {
      get
      {
        return values[0];
      }
    }

    /// <summary>
    /// Peek at the next Priority
    /// </summary>
    public Priority PriorityPeek
    {
      get
      {
        return priorities[0];
      }
    }

    private void demote(int index, Value value, Priority priority)
    {
      int childIndex = (index * 2) + 1; // left child

      while (childIndex < values.Count)
      {
        Priority childPriority = priorities[childIndex];

        if (childIndex + 1 < values.Count)
        {
          Priority rightPriority = priorities[childIndex + 1];
          if (sortsEarlierThan(rightPriority, childPriority))
          {
            childPriority = rightPriority;
            childIndex++; // right child
          }
        }

        if (sortsEarlierThan(childPriority, priority))
        {
          priorities.Insert(index, childPriority);
          values.Insert(index, values[childIndex]);
          index = childIndex;
          childIndex = (index * 2) + 1;
        }
        else
        {
          break;
        }
      }

      values.Insert(index, value);
      priorities.Insert(index, priority);
    }

    // 
    /// <summary>
    /// Get the value with the lowest priority creates a "hole" at the root of the tree. The algorithm swaps the hole with the appropriate child, until 
    /// the last entry will fit correctly into the hole (ie is lower priority than its children)
    /// </summary>
    /// <returns>the Value with the lowest priority</returns>
    public virtual Value Pop()
    {
      Value ret = values[0];

      // record the value/priority of the last entry
      int lastIndex = values.Count - 1;
      Value tempValue = values[lastIndex];
      Priority tempPriority = priorities[lastIndex];

      values.RemoveAt(lastIndex);
      priorities.RemoveAt(lastIndex);

      if (lastIndex > 0)
        demote(0, tempValue, tempPriority);

      if (INTERNAL_CONSISTENCY_CHECKING)
        check();

      return ret;
    }

    /// <summary>
    /// Set or get sorting order, ascending if true or descending if false. 
    /// </summary>
    public virtual bool SortOrderAscending
    {
      get
      {
        return sortOrderAscending;
      }
      set
      {
        if (sortOrderAscending != value)
        {
          sortOrderAscending = value;
          // reheapify the arrays
          for (int i = (values.Count / 2) - 1; i >= 0; i--)
            demote(i, values[i], priorities[i]);
        }
        if (INTERNAL_CONSISTENCY_CHECKING)
          check();
      }
    }

    private void check()
    {
      // for each entry, check that the child entries have a lower or equal priority
      int lastIndex = values.Count - 1;

      for (int i = 0; i < values.Count / 2; i++)
      {
        Priority currentPriority = priorities[i];

        int leftIndex = (i * 2) + 1;
        if (leftIndex <= lastIndex)
        {
          Priority leftPriority = priorities[leftIndex];
          if (sortsEarlierThan(leftPriority, currentPriority))
            throw new UnexpectedException("Internal error in PriorityQueue");
        }

        int rightIndex = (i * 2) + 2;
        if (rightIndex <= lastIndex)
        {
          Priority rightPriority = priorities[rightIndex];
          if (sortsEarlierThan(rightPriority, currentPriority))
            throw new UnexpectedException("Internal error in PriorityQueue");
        }
      }
    }
  }

  /// <summary>
  /// RTree uses double for priorities and Rectangle as values
  /// </summary>
  public class PriorityQueueRTree : PriorityQueue<double, Rectangle>
  {
    public PriorityQueueRTree(bool sortOrderAscending = true, int initialCapacity = 25)
      : base(sortOrderAscending, initialCapacity)
    {
    }
  }
}
