using System;

/*
 * Ported to C# from Java by Mats Persson, VelocityDB, Inc.
 *
 * original Java code:
 * Copyright 2010, Silvio Heuberger @ IFS www.ifs.hsr.ch
 *
 * This code is release under the Apache License 2.0.
 * You should have received a copy of the license
 * in the LICENSE file. If you have not, see
 * http://www.apache.org/licenses/LICENSE-2.0
 */
namespace VelocityDB.geohash
{

  [Serializable]
  public class BoundingBox
  {
    private const long SerialVersionUID = -7145192134410261076L;
    private double m_minLat;
    private double m_maxLat;
    private double m_minLon;
    private double m_maxLon;

    /// <summary>
    /// create a bounding box defined by two coordinates
    /// </summary>
    public BoundingBox(WGS84Point p1, WGS84Point p2) : this(p1.Latitude, p2.Latitude, p1.Longitude, p2.Longitude)
    {
    }

    public BoundingBox(double y1, double y2, double x1, double x2)
    {
      m_minLon = Math.Min(x1, x2);
      m_maxLon = Math.Max(x1, x2);
      m_minLat = Math.Min(y1, y2);
      m_maxLat = Math.Max(y1, y2);
    }

    public BoundingBox(BoundingBox that) : this(that.m_minLat, that.m_maxLat, that.m_minLon, that.m_maxLon)
    {
    }

    public virtual WGS84Point UpperLeft
    {
      get
      {
        return new WGS84Point(m_maxLat, m_minLon);
      }
    }

    public virtual WGS84Point LowerRight
    {
      get
      {
        return new WGS84Point(m_minLat, m_maxLon);
      }
    }

    public virtual double LatitudeSize
    {
      get
      {
        return m_maxLat - m_minLat;
      }
    }

    public virtual double LongitudeSize
    {
      get
      {
        return m_maxLon - m_minLon;
      }
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
      {
        return true;
      }
      if (obj is BoundingBox)
      {
        BoundingBox that = (BoundingBox) obj;
        return m_minLat == that.m_minLat && m_minLon == that.m_minLon && m_maxLat == that.m_maxLat && m_maxLon == that.m_maxLon;
      }
      else
      {
        return false;
      }
    }

    public override int GetHashCode()
    {
      int result = 17;
      result = 37 * result + HashCode(m_minLat);
      result = 37 * result + HashCode(m_maxLat);
      result = 37 * result + HashCode(m_minLon);
      result = 37 * result + HashCode(m_maxLon);
      return result;
    }

    private static int HashCode(double x)
    {
      long f = BitConverter.DoubleToInt64Bits(x);
      return (int)(f ^ ((long)((ulong)f >> 32)));
    }

    public virtual bool Contains(WGS84Point point)
    {
      return (point.Latitude >= m_minLat) && (point.Longitude >= m_minLon) && (point.Latitude <= m_maxLat) && (point.Longitude <= m_maxLon);
    }

    public virtual bool Intersects(BoundingBox other)
    {
      return !(other.m_minLon > m_maxLon || other.m_maxLon < m_minLon || other.m_minLat > m_maxLat || other.m_maxLat < m_minLat);
    }

    public override string ToString()
    {
      return UpperLeft + " -> " + LowerRight;
    }

    public virtual WGS84Point CenterPoint
    {
      get
      {
        double centerLatitude = (m_minLat + m_maxLat) / 2;
        double centerLongitude = (m_minLon + m_maxLon) / 2;
        return new WGS84Point(centerLatitude, centerLongitude);
      }
    }

    public virtual void ExpandToInclude(BoundingBox other)
    {
      if (other.m_minLon < m_minLon)
      {
        m_minLon = other.m_minLon;
      }
      if (other.m_maxLon > m_maxLon)
      {
        m_maxLon = other.m_maxLon;
      }
      if (other.m_minLat < m_minLat)
      {
        m_minLat = other.m_minLat;
      }
      if (other.m_maxLat > m_maxLat)
      {
        m_maxLat = other.m_maxLat;
      }
    }

    public virtual double MinLon
    {
      get
      {
        return m_minLon;
      }
    }

    public virtual double MinLat
    {
      get
      {
        return m_minLat;
      }
    }

    public virtual double MaxLat
    {
      get
      {
        return m_maxLat;
      }
    }

    public virtual double MaxLon
    {
      get
      {
        return m_maxLon;
      }
    }
  }
}