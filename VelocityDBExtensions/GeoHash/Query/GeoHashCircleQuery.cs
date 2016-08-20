using System;
using System.Collections.Generic;

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
namespace VelocityDB.geohash.query
{
  using VincentyGeodesy = VelocityDB.geohash.util.VincentyGeodesy;

  /// <summary>
  /// represents a radius search around a specific point via geohashes.
  /// Approximates the circle with a square!
  /// </summary>
  [Serializable]
  public class GeoHashCircleQuery : GeoHashQuery
  {
    private const long SerialVersionUID = 1263295371663796291L;
    private double m_radius;
    private GeoHashBoundingBoxQuery m_query;
    private WGS84Point m_center;

    /// <summary>
    /// create a <seealso cref="GeoHashCircleQuery"/> with the given center point and a
    /// radius in meters.
    /// </summary>
    public GeoHashCircleQuery(WGS84Point center, double radius)
    {
      m_radius = radius;
      m_center = center;
      WGS84Point northEast = VincentyGeodesy.MoveInDirection(VincentyGeodesy.MoveInDirection(center, 0, radius), 90, radius);
      WGS84Point southWest = VincentyGeodesy.MoveInDirection(VincentyGeodesy.MoveInDirection(center, 180, radius), 270, radius);
      BoundingBox bbox = new BoundingBox(northEast, southWest);
      m_query = new GeoHashBoundingBoxQuery(bbox);
    }

    public bool Contains(GeoHash hash)
    {
      return m_query.Contains(hash);
    }

    public string WktBox
    {
      get
      {
        return m_query.WktBox;
      }
    }

    public IList<GeoHash> SearchHashes
    {
      get
      {
        return m_query.SearchHashes;
      }
    }

    public override string ToString()
    {
      return "Cicle Query [center=" + m_center + ", radius=" + RadiusString + "]";
    }

    private string RadiusString
    {
      get
      {
        if (m_radius > 1000)
        {
          return m_radius / 1000 + "km";
        }
        else
        {
          return m_radius + "m";
        }
      }
    }

    public bool Contains(WGS84Point point)
    {
      return m_query.Contains(point);
    }

    public BoundingBox BoundingBox => m_query.BoundingBox;
  }
}