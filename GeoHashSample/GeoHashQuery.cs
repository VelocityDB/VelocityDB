using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDB.geohash;
using VelocityDB.geohash.query;
using VelocityDbSchema;

namespace GeoHashSample
{
  class GeoHashQuery
  {
    public static HashSet<Person> SearchGeoHashIndex(string bootDirectory, double minLat, double minLon, double maxLat, double maxLon)
    {
      HashSet<Person> resultSet = new HashSet<Person>();
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
      using (SessionNoServer session = new SessionNoServer(bootDirectory))
      {
        session.BeginRead();
        BTreeMap<Int64, VelocityDbList<Person>> btreeMap = session.AllObjects<BTreeMap<Int64, VelocityDbList<Person>>>().FirstOrDefault();
        foreach (GeoHash hash in query.SearchHashes)
        {
          BTreeMapIterator<Int64, VelocityDbList<Person>> itr = btreeMap.Iterator();
          itr.GoTo(hash.LongValue);
          var current = itr.Current();
          while (current.Value != null)
          {
            GeoHash geoHash = GeoHash.FromLongValue(current.Key);
            if (geoHash.Within(hash) || (geoHash.SignificantBits > hash.SignificantBits && hash.Within(geoHash)))
            {
              foreach (Person person in current.Value)
              {
                resultSet.Add(person);
              }
              current = itr.Next();
            }
            else
              break;
          }
        }
        // actual geohash bounding box may be including some that are not within requested bounding box so remove such items if any
        HashSet<Person> notWithin = new HashSet<Person>();
        foreach (Person person in resultSet)
        {
          if (person.Lattitude < min.Latitude || person.Lattitude > max.Latitude || person.Longitude < min.Longitude || person.Lattitude > max.Latitude)
            notWithin.Add(person);
        }
        foreach (Person person in notWithin)
          resultSet.Remove(person);
        foreach (Person person in resultSet)
        {
          Console.WriteLine(person.ToString() + " Lattitude: " + person.Lattitude + " Longitude: " + person.Longitude);
        }
        session.Commit();
      }

      return resultSet;
    }
  }
}
