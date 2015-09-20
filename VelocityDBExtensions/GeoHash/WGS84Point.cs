using System;

/*
 * Ported to C# from Java by Mats Persson, VelocityDB, Inc.
 *
 * original Java code:
 * Copyright 2010, Silvio Heuberger @ IFS www.ifs.hsr.ch
 *
 * This code is release under the LGPL license.
 * You should have received a copy of the license
 * in the LICENSE file. If you have not, see
 * http://www.gnu.org/licenses/lgpl-3.0.txt
 */
namespace VelocityDB.geohash
{
  /// <summary>
  /// <seealso cref="WGS84Point"/> encapsulates coordinates on the earths surface.<br>
  /// Coordinate projections might end up using this class...
  /// </summary>
  [Serializable]
  public class WGS84Point
  {
    private const long SerialVersionUID = 7457963026513014856L;
    private readonly double m_longitude;
    private readonly double m_latitude;

    public WGS84Point(double latitude, double longitude)
    {
      m_latitude = latitude;
      m_longitude = longitude;
      if (Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180)
      {
        throw new System.ArgumentException("The supplied coordinates " + this + " are out of range.");
      }
    }

    public WGS84Point(WGS84Point other) : this(other.m_latitude, other.m_longitude)
    {
    }

    public virtual double Latitude
    {
      get
      {
        return m_latitude;
      }
    }

    public virtual double Longitude
    {
      get
      {
        return m_longitude;
      }
    }

    public override string ToString()
    {
      return string.Format("(" + m_latitude + "," + m_longitude + ")");
    }

    public override bool Equals(object obj)
    {
      if (obj is WGS84Point)
      {
        WGS84Point other = (WGS84Point) obj;
        return m_latitude == other.m_latitude && m_longitude == other.m_longitude;
      }
      return false;
    }

    public override int GetHashCode()
    {
      int result = 42;
      long latBits = BitConverter.DoubleToInt64Bits(m_latitude);
      long lonBits = BitConverter.DoubleToInt64Bits(m_longitude);
      result = 31 * result + (int)(latBits ^ ((long)((ulong)latBits >> 32)));
      result = 31 * result + (int)(lonBits ^ ((long)((ulong)lonBits >> 32)));
      return result;
    }
  }
}