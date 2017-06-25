using System;
using System.Collections.Generic;
#if !NET2
using System.Linq;
#endif
using System.Text;

namespace VelocityDBExtensions.Spatial
{
  /// <summary>
  /// Points are most often considered within the framework of Euclidean geometry, where they are one of the fundamental objects. Euclid originally defined the point as "that which has no part". In two-dimensional Euclidean space, a point is represented by an ordered pair (x,?y) of numbers, where the first number conventionally represents the horizontal and is often denoted by x, and the second number conventionally represents the vertical and is often denoted by y.
  /// </summary>
  public struct Point
  {
    /// <summary>
    /// The (x, y) coordinates of the point.
    /// </summary>
    public double x, y;

    /// <summary>
    /// Creates a Point instance
    /// </summary>
    /// <param name="x">The x coordinate of the point</param>
    /// <param name="y">The y coordinate of the point</param>   
    public Point(double x, double y)
    {
      this.x = x;
      this.y = y;
    }
 
    /// <summary>
    /// return "(" + x + ", " + y + ")";
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "(" + x + ", " + y + ")";
    }
  }
}