using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDBExtensions.geohash;
using VelocityDBExtensions.geohash.query;
using VelocityDbSchema;

namespace GeoHashSample
{
  class GeoHashQuery
  {
    public static HashSet<GeoObj> SearchGeoHashIndex(SessionBase session, double minLat, double minLon, double maxLat, double maxLon)
    {
      HashSet<GeoObj> resultSet = new HashSet<GeoObj>();
      if (minLat > maxLat)
      {
        double t = minLat;
        minLat = maxLat;
        maxLat = t;
      }
      if (minLon > maxLon)
      {
        double t = minLon;
        minLon = maxLon;
        maxLon = t;
      }
      WGS84Point min = new WGS84Point(minLat, minLon);
      WGS84Point max = new WGS84Point(maxLat, maxLon);
      BoundingBox bbox = new BoundingBox(min, max);
      GeoHashBoundingBoxQuery query = new GeoHashBoundingBoxQuery(bbox);
      var btreeSet = session.AllObjects<BTreeSet<GeoObj>>().FirstOrDefault();
      foreach (GeoHash hash in query.SearchHashes)
      {
        var itr = btreeSet.Iterator();
        itr.GoTo(new GeoObj(hash.LongValue));
        var current = itr.Current();
        while (current != null)
        {
          GeoHash geoHash = GeoHash.FromLongValue(current.GeoHash);
          if ((geoHash.SignificantBits >= hash.SignificantBits && geoHash.Within(hash)) || (geoHash.SignificantBits < hash.SignificantBits && hash.Within(geoHash)))
          {
            if (!(current.Latitude < bbox.MinLat || current.Latitude > bbox.MaxLat || current.Longitude < bbox.MinLon || current.Longitude > bbox.MaxLon))
              resultSet.Add(current);
            current = itr.Next();
          }
          else
            break;
        }
      }
      return resultSet;
    }

    public static HashSet<GeoObj> SearchGeoHashIndex(SessionBase session, double lat, double lon, double radius)
    {
      HashSet<GeoObj> resultSet = new HashSet<GeoObj>();
      WGS84Point center = new WGS84Point(lat, lon);
      GeoHashCircleQuery query = new GeoHashCircleQuery(center, radius); // radius in meters
      BoundingBox bbox = query.BoundingBox;
      var btreeSet = session.AllObjects<BTreeSet<GeoObj>>().FirstOrDefault();
      foreach (GeoHash hash in query.SearchHashes)
      {
        var itr = btreeSet.Iterator();
        itr.GoTo(new GeoObj(hash.LongValue));
        var current = itr.Current();
        while (current != null)
        {
          GeoHash geoHash = GeoHash.FromLongValue(current.GeoHash);
          if ((geoHash.SignificantBits >= hash.SignificantBits && geoHash.Within(hash)) || (geoHash.SignificantBits < hash.SignificantBits && hash.Within(geoHash)))
          {
            if (!(current.Latitude < bbox.MinLat || current.Latitude > bbox.MaxLat || current.Longitude < bbox.MinLon || current.Longitude > bbox.MaxLon))
              resultSet.Add(current);
            current = itr.Next();
          }
          else
            break;
        }
      }
      return resultSet;
    }

  }
}
