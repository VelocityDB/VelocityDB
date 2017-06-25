using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VelocityDBExtensions.Spatial;

namespace NUnitTests
{
  [TestFixture]
  public class RtreeTest
  {
    internal Rectangle[] rects = null;
    internal Random r = new Random(0);

    internal class Counter
    {
      public int count = 0;
      public virtual bool execute(Rectangle arg0)
      {
        count++;
        return true;
      }
    }

    [TestCase(20000, 20, 80)]
 //   [TestCase(500000, 600, 1000)]
    // [TestCase(500, 1, 2)]
   // [TestCase(50000, 1, 3)]
   // [TestCase(500000, 5000, 10000)]
    public void testDeleteAllEntries(int numRects, int minNodeEntries, int maxNodeEntries)
    {
      Console.WriteLine("testDeleteAllEntries");

      rects = new Rectangle[numRects];
      for (int i = 0; i < numRects; i += 1)
        rects[i] = new Rectangle(r.NextDouble(), r.NextDouble(), r.NextDouble(), r.NextDouble());

      runDeleteAllEntries(minNodeEntries, maxNodeEntries, numRects);
      Console.WriteLine("Done with testDeleteAllEntries");
    }

    private void runDeleteAllEntries(int minNodeEntries, int maxNodeEntries, int numRects)
    {
      RTree rtree = new RTree(minNodeEntries, maxNodeEntries);

      for (int i = 0; i <= numRects; i += 100)
      {
        // add some entries
        for (int j = 0; j < i; j++)
        {
          rtree.Add(rects[j]);
        }
        Assert.True(rtree.checkConsistency());

        // now delete them all
        for (int j = 0; j < i; j++)
        {
          rtree.Remove(rects[j]);
        }
        Assert.True(rtree.Count == 0);
        Assert.True(rtree.checkConsistency());

        // check that we can make queries on an empty rtree without error.
        Rectangle testRect = new Rectangle(1, 2, 3, 4);
        Point testPoint = new Point(1, 2);

        Counter counter = new Counter();
        rtree.Intersects(testRect, counter.execute);
        Assert.True(counter.count == 0);

        PriorityQueueRTree priorityQue = rtree.Nearest(testPoint, float.MaxValue);
        Assert.True(priorityQue.Count == 0);

        priorityQue = rtree.NearestN(testPoint, 10, float.MaxValue);
        Assert.True(priorityQue.Count == 0);

        rtree.Contains(testRect, counter.execute);
        Assert.True(counter.count == 0);
      }
    }

    private Rectangle nextRect()
    {
      return new Rectangle(r.Next(100), r.Next(100), r.Next(100), r.Next(100));
    }

    [Test]
    public virtual void testMoveEntries()
    {
      runMoveEntries(4, 50, 4, 10);
    }


    private void runMoveEntries(UInt16 minNodeEntries, UInt16 maxNodeEntries, int numRects, int numMoves)
    {
      RTree rtree = new RTree(minNodeEntries, maxNodeEntries);

      Rectangle[] rects = new Rectangle[numRects];

      // first add the rects
      for (int i = 0; i < numRects; i++)
      {
        rects[i] = nextRect();
        rtree.Add(rects[i]);
      }

      // now move each one in turn
      for (int move = 0; move < numMoves; move++)
      {
        for (int i = 0; i < numRects; i++)
        {
          rtree.Remove(rects[i]);
          rects[i].set(nextRect());
          rtree.Add(rects[i]);
          Assert.True(rtree.checkConsistency());
        }
      }
    }

    [Test]
    public virtual void testMaxValue()
    {
      RTree rTree = new RTree();
      rTree.Add(new Rectangle(8.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(1.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(10.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(5.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(6.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(9.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(5.0f, 7.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(2.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(2.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(5.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(6.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(9.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(5.0f, 7.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(4.0f, 7.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(2.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(2.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(9.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(10.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 4.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(8.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(2.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(2.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(2.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(5.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(4.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(5.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(5.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(5.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(2.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(1.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(0.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(8.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(8.0f, 7.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(5.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 0.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(4.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(4.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(1.0f, 0.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(1.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(1.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(9.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 6.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(4.0f, 7.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(4.0f, 0.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(5.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(5.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(4.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(5.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(6.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(6.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(4.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(4.0f, 8.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(1.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(1.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(0.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 0.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 0.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(6.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(2.0f, 1.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(7.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 5.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(0.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(0.0f, 2.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(3.0f, 4.0f, float.MaxValue, float.MaxValue));
      rTree.Add(new Rectangle(3.0f, 3.0f, float.MaxValue, float.MaxValue));
      rTree.Remove(new Rectangle(8.0f, 7.0f, float.MaxValue, float.MaxValue));
    }
 
    [Test]
    public virtual void testIntersects()
    {
      Rectangle? r0_0_0_0 = new Rectangle(0, 0, 0, 0);
      Rectangle? r1_1_1_1 = new Rectangle(1, 1, 1, 1);
      Rectangle? r2_2_6_6 = new Rectangle(2, 2, 6, 6);
      Rectangle? r3_3_7_5 = new Rectangle(3, 3, 7, 5);
      Rectangle? r3_3_5_7 = new Rectangle(3, 3, 5, 7);
      Rectangle? r1_3_5_5 = new Rectangle(1, 3, 5, 5);
      Rectangle? r3_1_5_5 = new Rectangle(3, 1, 5, 5);

      // A rectangle always intersects itself
      Assert.True(r0_0_0_0.Value.Intersects(ref r0_0_0_0));
      Assert.True(r2_2_6_6.Value.Intersects(ref r2_2_6_6));

      Assert.True(r0_0_0_0.Value.Intersects(ref r1_1_1_1) == false);
      Assert.True(r1_1_1_1.Value.Intersects(ref r0_0_0_0) == false);

      // Rectangles that intersect only on the right-hand side
      Assert.True(r2_2_6_6.Value.Intersects(ref r3_3_7_5));
      Assert.True(r3_3_7_5.Value.Intersects(ref r2_2_6_6));

      // Rectangles that touch only on the right hand side
      //assertTrue(r

      // Rectangles that intersect only on the top side
      Assert.True(r2_2_6_6.Value.Intersects(ref r3_3_5_7));
      Assert.True(r3_3_5_7.Value.Intersects(ref r2_2_6_6));

      // Rectangles that intersect only on the left-hand side
      Assert.True(r2_2_6_6.Value.Intersects(ref r1_3_5_5));
      Assert.True(r1_3_5_5.Value.Intersects(ref r2_2_6_6));

      // Rectangles that intersect only on the bottom side
      Assert.True(r2_2_6_6.Value.Intersects(ref r3_1_5_5));
      Assert.True(r3_1_5_5.Value.Intersects(ref r2_2_6_6));
    }
  }
}
