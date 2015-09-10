using System;
using System.Collections.Generic;
#if !NET2
using System.Linq;
#endif
using System.Text;
using VelocityDb;

namespace VelocityDb.Collection.Spatial
{
  /// <summary>
  /// 
  /// </summary>
  public struct Rectangle : IComparable
  { 
    double minX;
    double minY;
    double maxX;
    double maxY;

    /// <summary>
    /// The low x coordinate
    /// </summary>
    public double MinX
    {
      get
      {
        return minX;
      }
      set
      {
        minX = value;
      }
    }

    /// <summary>
    /// The high x coordinate
    /// </summary>
    public double MaxX
    {
      get
      {
        return maxX;
      }
      set
      {
        maxX = value;
      }
    }

    /// <summary>
    /// The low y coordinate
    /// </summary>
    public double MinY
    {
      get
      {
        return minY;
      }
      set
      {
        minY = value;
      }
    }

    /// <summary>
    /// The high y coordinate
    /// </summary>
    public double MaxY
    {
      get
      {
        return maxY;
      }
      set
      {
        maxY = value;
      }
    }

    internal Rectangle(bool s)
    {
      minX = double.MaxValue;
      minY = double.MaxValue;
      maxX = double.MinValue;
      maxY = double.MinValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x1">coordinate of any corner of the rectangle</param>
    /// <param name="y1">(see x1)</param>
    /// <param name="x2">coordinate of the opposite corner</param>
    /// <param name="y2">(see x2)</param>
    public Rectangle(double x1, double y1, double x2, double y2)
    {
      minX = Math.Min(x1, x2);
      maxX = Math.Max(x1, x2);
      minY = Math.Min(y1, y2);
      maxY = Math.Max(y1, y2);
    }
    
    /// <summary>
    /// Sets the size of this rectangle to equal the passed rectangle.
    /// </summary>
    /// <param name="r"></param>  
    public void set(Rectangle r)
    {
      minX = r.minX;
      minY = r.minY;
      maxX = r.maxX;
      maxY = r.maxY;
    }
   
    /// <summary>
    /// Determine whether an edge of this rectangle overlies the equivalent edge of the passed rectangle
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public bool edgeOverlaps(Rectangle r)
    {
      return minX == r.minX || maxX == r.maxX || minY == r.minY || maxY == r.maxY;
    }

    public bool Equals(Rectangle r)
    {
      return minX == r.minX && minY == r.minY && maxX == r.maxX && maxY == r.maxY;
    }

    public int CompareTo(object o)
    {
      Rectangle r = (Rectangle) o;
      if (minX < r.minX)
        return -1;
      if (r.minX < minX)
        return 1;
      if (minY < r.minY)
        return -1;
      if (r.minY < minY)
        return 1;
      if (maxX < r.maxX)
        return -1;
      if (r.maxX < maxX)
        return 1;
      if (maxY < r.maxY)
        return -1;
      if (r.maxY < maxY)
        return 1;
      return 0;
    }

    /// <summary>
    /// Determine whether this rectangle intersects the passed rectangle
    /// </summary>
    /// <param name="r">The rectangle that might intersect this rectangle</param>
    /// <returns>return true if the rectangles intersect, false if they do not intersect</returns>
    public bool Intersects(ref Rectangle? r)
    {
      return maxX >= r.Value.minX && minX <= r.Value.maxX && maxY >= r.Value.minY && minY <= r.Value.maxY;
    }

    /// <summary>
    /// Determine whether this rectangle contains the passed rectangle
    /// </summary>
    /// <param name="r">The rectangle that might be contained by this rectangle</param>
    /// <returns>return true if this rectangle contains the passed rectangle, false if it does not</returns>
    public bool Contains(Rectangle r)
    {
      return maxX >= r.maxX && minX <= r.minX && maxY >= r.maxY && minY <= r.minY;
    }

    /// <summary>
    /// Determine whether this rectangle is contained by the passed rectangle
    /// </summary>
    /// <param name="r">The rectangle that might contain this rectangle</param>
    /// <returns>return true if the passed rectangle contains this rectangle, false if it does not</returns> 
    public bool containedBy(Rectangle r)
    {
      return r.maxX >= maxX && r.minX <= minX && r.maxY >= maxY && r.minY <= minY;
    }

    /// <summary>
    /// Return the distance between this rectangle and the passed point. If the rectangle contains the point, the distance is zero.
    /// </summary>
    /// <param name="p">Point to find the distance to</param>
    /// <returns>return distance beween this rectangle and the passed point.</returns>
    public double Distance(Point p)
    {
      double distanceSquared = 0;

      double temp = minX - p.x;
      if (temp < 0)
      {
        temp = p.x - maxX;
      }

      if (temp > 0)
      {
        distanceSquared += (temp * temp);
      }

      temp = minY - p.y;
      if (temp < 0)
      {
        temp = p.y - maxY;
      }

      if (temp > 0)
      {
        distanceSquared += (temp * temp);
      }

      return (double)Math.Sqrt(distanceSquared);
    }

    /// <summary>
    /// Return the distance between a rectangle and a point. If the rectangle contains the point, the distance is zero.
    /// </summary>
    /// <param name="pX">X coordinate of point</param>
    /// <param name="pY">Y coordinate of point</param>
    /// <returns>return distance beween this rectangle and the passed point.</returns>  
    public double distance(double pX, double pY)
    {
      return (double)Math.Sqrt(distanceSq(pX, pY));
    }

    /// <summary>
    /// Get the square of the distance between two points.
    /// </summary>
    /// <param name="pX">the x coordinate of point</param>
    /// <param name="pY">the y coordinate of point</param>
    /// <returns>the square of the distance between two points.</returns>
    public double distanceSq(double pX, double pY)
    {
      double distanceSqX = 0;
      double distanceSqY = 0;

      if (minX > pX)
      {
        distanceSqX = minX - pX;
        distanceSqX *= distanceSqX;
      }
      else if (pX > maxX)
      {
        distanceSqX = pX - maxX;
        distanceSqX *= distanceSqX;
      }

      if (minY > pY)
      {
        distanceSqY = minY - pY;
        distanceSqY *= distanceSqY;
      }
      else if (pY > maxY)
      {
        distanceSqY = pY - maxY;
        distanceSqY *= distanceSqY;
      }

      return distanceSqX + distanceSqY;
    }

    /// <summary>
    /// Return the distance between this rectangle and the passed rectangle. If the rectangles overlap, the distance is zero.
    /// </summary>
    /// <param name="r">Rectangle to find the distance to</param>
    /// <returns>return distance between this rectangle and the passed rectangle</returns>
    public double Distance(Rectangle r)
    {
      double distanceSquared = 0;
      double greatestMin = Math.Max(minX, r.minX);
      double leastMax = Math.Min(maxX, r.maxX);
      if (greatestMin > leastMax)
      {
        distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
      }
      greatestMin = Math.Max(minY, r.minY);
      leastMax = Math.Min(maxY, r.maxY);
      if (greatestMin > leastMax)
      {
        distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
      }
      return (double)Math.Sqrt(distanceSquared);
    }
 
    /// <summary>
    /// Calculate the area by which this rectangle would be enlarged if added to the passed rectangle. Neither rectangle is altered.
    /// </summary>
    /// <param name="r">Rectangle to union with this rectangle, in order to compute the difference in area of the union and the original rectangle</param>
    /// <returns>enlargement</returns> 
    public double Enlargement(ref Rectangle r)
    {
      double enlargedArea = (Math.Max(maxX, r.maxX) - Math.Min(minX, r.minX)) * (Math.Max(maxY, r.maxY) - Math.Min(minY, r.minY));

      return enlargedArea - Area;
    }

    /// <summary>
    /// Calculate the area by which a rectangle would be enlarged if added to the passed rectangle
    /// </summary>
    /// <param name="r1">minimum X coordinate of rectangle 1</param>
    /// <param name="r2">minimum X coordinate of rectangle 2</param>
    /// <returns>return enlargement</returns> 
    public static double enlargement(Rectangle r1, Rectangle r2)
    {
      double r1Area = (r1.maxX - r1.minX) * (r1.maxY - r1.minY);

      if (r1Area == double.PositiveInfinity)
        return 0; // cannot enlarge an infinite rectangle...

      double r1r2UnionArea = Math.Max(r1.maxX, r2.maxX) - Math.Min(r1.minX, r2.minX) * (Math.Max(r1.maxY, r2.maxY) - Math.Min(r1.minY, r2.minY));

      if (r1r2UnionArea == double.PositiveInfinity)        
        return double.PositiveInfinity;  // if a finite rectangle is enlarged and becomes infinite, then the enlargement must be infinite.
      return r1r2UnionArea - r1Area;
    }

    /// <summary>
    /// Compute the area of this rectangle.
    /// </summary>
    /// <returns>The area of this rectangle</returns>  
    public double Area
    {
      get
      {
        return (maxX - minX) * (maxY - minY);
      }
    }

    /// <summary>
    /// Computes the union of this rectangle and the passed rectangle, storing the result in this rectangle.
    /// </summary>
    /// <param name="r">Rectangle to add to this rectangle</param> 
    public void Add(Rectangle r)
    {
      if (r.minX < minX)
        minX = r.minX;
      if (r.maxX > maxX)
        maxX = r.maxX;
      if (r.minY < minY)
        minY = r.minY;
      if (r.maxY > maxY)
        maxY = r.maxY;
    }

    /// <summary>
    /// Computes the union of this rectangle and the passed point, storing the result in this rectangle.
    /// </summary>
    /// <param name="p">Point to add to this rectangle</param>  
    public void add(Point p)
    {
      if (p.x < minX)
        minX = p.x;
      if (p.x > maxX)
        maxX = p.x;
      if (p.y < minY)
        minY = p.y;
      if (p.y > maxY)
        maxY = p.y;
    }

    /// <summary>
    /// Find the the union of this rectangle and the passed rectangle.Neither rectangle is altered
    /// </summary>
    /// <param name="r">The rectangle to union with this rectangle</param>
    /// <returns></returns>
    public Rectangle union(Rectangle r)
    {
      Rectangle union = this;
      union.Add(r);
      return union;
    }

    /// <summary>
    /// Customized hash code using the coordinates of the rectangle 37 * minX * minY * maxX * maxY
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return (int) (37 * minX * minY * maxX * maxY);
    }

    /// <summary>
    /// Determine whether this rectangle is the same as another object. Note that two rectangles can be equal but not the same object, if they both have the same bounds.
    /// </summary>
    /// <param name="o">The object to compare with this rectangle</param>
    /// <returns></returns>   
    public bool sameObject(object o)
    {
      return base.Equals(o);
    }

    /// <summary>
    /// Return a string representation of this rectangle, in the form: (1.2, 3.4), (5.6, 7.8)
    /// </summary>
    /// <returns>return String String representation of this rectangle</returns>   
    public override string ToString()
    {
      return "(" + minX + ", " + minY + "), (" + maxX + ", " + maxY + ")";
    }

    /// <summary>
    /// The width of a rectangle
    /// </summary>
    public double Width
    {
      get
      {
        return maxX - minX;
      }
    }
    /// <summary>
    /// The height of a rectangle
    /// </summary>
    public double Height
    {
      get
      {
        return maxY - minY;
      }
    }

    /// <summary>
    /// The aspect ratio denotes the ratio of length to width of the rectangle (Width / Height)
    /// </summary>
    public double AspectRatio
    {
      get
      {
        return Width / Height;
      }
    }

    /// <summary>
    /// Gets the centre of the rectangle
    /// </summary>
    public Point Centre
    {
      get
      {
        return new Point((minX + maxX) / 2, (minY + maxY) / 2);
      }
    }
  }
}