using System;
using System.Collections.Generic;
using System.Text;

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
namespace VelocityDBExtensions.Geo.query
{
  using GeoHashSizeTable = VelocityDBExtensions.Geo.util.GeoHashSizeTable;

  /// <summary>
  /// This class returns the hashes covering a certain bounding box. There are
  /// either 1,2 or 4 susch hashes, depending on the position of the bounding box
  /// on the geohash grid.
  /// </summary>
  [Serializable]
  public class GeoHashBoundingBoxQuery : GeoHashQuery
  {
    private const long SerialVersionUID = 9223256928940522683L;
    /* there's not going to be more than 4 hashes. */
    private IList<GeoHash> m_searchHashes = new List<GeoHash>(4);
    /* the combined bounding box of those hashes. */
    private BoundingBox m_boundingBox;

    public GeoHashBoundingBoxQuery(BoundingBox bbox)
    {
      int fittingBits = GeoHashSizeTable.NumberOfBitsForOverlappingGeoHash(bbox);
      WGS84Point center = bbox.CenterPoint;
      GeoHash centerHash = GeoHash.WithBitPrecision(center.Latitude, center.Longitude, fittingBits);

      if (HashFits(centerHash, bbox))
      {
        AddSearchHash(centerHash);
      }
      else
      {
        ExpandSearch(centerHash, bbox);
      }
    }

    private void AddSearchHash(GeoHash hash)
    {
      if (m_boundingBox == null)
      {
        m_boundingBox = new BoundingBox(hash.BoundingBox);
      }
      else
      {
        m_boundingBox.ExpandToInclude(hash.BoundingBox);
      }
      m_searchHashes.Add(hash);
    }

    private void ExpandSearch(GeoHash centerHash, BoundingBox bbox)
    {
      AddSearchHash(centerHash);

      foreach (GeoHash adjacent in centerHash.Adjacent)
      {
        BoundingBox adjacentBox = adjacent.BoundingBox;
        if (adjacentBox.Intersects(bbox) && !m_searchHashes.Contains(adjacent))
        {
          AddSearchHash(adjacent);
        }
      }
    }

    private bool HashFits(GeoHash hash, BoundingBox bbox)
    {
      return hash.Contains(bbox.UpperLeft) && hash.Contains(bbox.LowerRight);
    }

    public bool Contains(GeoHash hash)
    {
      foreach (GeoHash searchHash in m_searchHashes)
      {
        if (hash.Within(searchHash))
        {
          return true;
        }
      }
      return false;
    }

    public bool Contains(WGS84Point point)
    {
      return Contains(GeoHash.WithBitPrecision(point.Latitude, point.Longitude, 64));
    }

    public IList<GeoHash> SearchHashes
    {
      get
      {
        return m_searchHashes;
      }
    }

    public override string ToString()
    {
      StringBuilder bui = new StringBuilder();
      foreach (GeoHash hash in m_searchHashes)
      {
        bui.Append(hash).Append("\n");
      }
      return bui.ToString();
    }

    public string WktBox
    {
      get
      {
        return "BOX(" + m_boundingBox.MinLon + " " + m_boundingBox.MinLat + "," + m_boundingBox.MaxLon + " " + m_boundingBox.MaxLat + ")";
      }
    }

    public BoundingBox BoundingBox => m_boundingBox;
  }
}