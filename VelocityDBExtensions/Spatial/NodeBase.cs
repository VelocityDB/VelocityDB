#if !NET2
using System;
using System.Collections.Generic;
#if !NET2
using System.Linq;
#endif
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDb.Collection.Spatial
{
  internal abstract class NodeBase : OptimizedPersistable
  {
    internal int level;
    internal int entryCount;
    internal Rectangle?[] entries;
    internal Rectangle minimumBoundingRectangle = new Rectangle(true);

    internal abstract bool IsLeaf { get; }
    internal abstract double Nearest(Point p, RTree rTree, double furthestDistanceSq, PriorityQueueRTree nearestRectangles);
    internal abstract void reorganize(RTree rtree);
    internal void addEntry(ref Rectangle r)
    {
      Update();
      entries[entryCount++] = r;
      if (r.MinX < minimumBoundingRectangle.MinX)
        minimumBoundingRectangle.MinX = r.MinX;
      if (r.MinY < minimumBoundingRectangle.MinY)
        minimumBoundingRectangle.MinY = r.MinY;
      if (r.MaxX > minimumBoundingRectangle.MaxX)
        minimumBoundingRectangle.MaxX = r.MaxX;
      if (r.MaxY > minimumBoundingRectangle.MaxY)
        minimumBoundingRectangle.MaxY = r.MaxY;
    }
    
    /// <summary>
    /// Given a node object, calculate the node Minimum Bounding Rectangle from it's entries. Used in consistency checking
    /// </summary>
    /// <returns></returns>  
    internal Rectangle calculateMinimumBoundingRectangle()
    {
      minimumBoundingRectangle = new Rectangle(true);
      Update();
      for (int i = 0; i < entryCount; i++)
      {
        if (entries[i].Value.MinX < minimumBoundingRectangle.MinX)
        {
          minimumBoundingRectangle.MinX = entries[i].Value.MinX;
        }
        if (entries[i].Value.MinY < minimumBoundingRectangle.MinY)
        {
          minimumBoundingRectangle.MinY = entries[i].Value.MinY;
        }
        if (entries[i].Value.MaxX > minimumBoundingRectangle.MaxX)
        {
          minimumBoundingRectangle.MaxX = entries[i].Value.MaxX;
        }
        if (entries[i].Value.MaxY > minimumBoundingRectangle.MaxY)
        {
          minimumBoundingRectangle.MaxY = entries[i].Value.MaxY;
        }
      }
      return minimumBoundingRectangle;
    }

    internal Rectangle calculateMBR()
    {
      Rectangle mbr = new Rectangle(true);

      for (int i = 0; i < entryCount; i++)
      {
        if (entries[i].Value.MinX < mbr.MinX)
          mbr.MinX = entries[i].Value.MinX;
        if (entries[i].Value.MinY < mbr.MinY)
          mbr.MinY = entries[i].Value.MinY;
        if (entries[i].Value.MaxX > mbr.MaxX)
          mbr.MaxX = entries[i].Value.MaxX;
        if (entries[i].Value.MaxY > mbr.MaxY)
          mbr.MaxY = entries[i].Value.MaxY;
      }
      return mbr;
    }

    internal virtual void recalculateMBR()
    {
      Update();
      minimumBoundingRectangle = entries[0].Value;

      for (int i = 1; i < entryCount; i++)
      {
        if (entries[i].Value.MinX < minimumBoundingRectangle.MinX)
          minimumBoundingRectangle.MinX = entries[i].Value.MinX;
        if (entries[i].Value.MinY < minimumBoundingRectangle.MinY)
          minimumBoundingRectangle.MinY = entries[i].Value.MinY;
        if (entries[i].Value.MaxX > minimumBoundingRectangle.MaxX)
          minimumBoundingRectangle.MaxX = entries[i].Value.MaxX;
        if (entries[i].Value.MaxY > minimumBoundingRectangle.MaxY)
          minimumBoundingRectangle.MaxY = entries[i].Value.MaxY;
      }
    }

    // deletedMin/MaxX/Y is a rectangle that has just been deleted or made smaller. Thus, the MBR is only recalculated if the deleted rectangle influenced the old MBR
    internal virtual void recalculateMBRIfInfluencedBy(ref Rectangle r)
    {
      if (minimumBoundingRectangle.MinX == r.MinX || minimumBoundingRectangle.MinY == r.MinY || minimumBoundingRectangle.MaxX == r.MaxX || minimumBoundingRectangle.MaxY == r.MaxY)
        recalculateMBR();
    }

    public override ushort ObjectsPerPage
    {
      get
      {
        return 1;
      }
    }
  }
}
#endif