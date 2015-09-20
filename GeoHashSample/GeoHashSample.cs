using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDB.geohash;
using VelocityDbSchema;

namespace GeoHashSample
{
  class GeoHashSample
  {
    static readonly string s_systemDir = "GeoHashSample";
    static readonly string s_licenseDbFile = "c:/4.odb";

    static void CreateData()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        bool dirExist = Directory.Exists(session.SystemDirectory);
        if (dirExist)
          Directory.Delete(session.SystemDirectory, true); // remove systemDir from prior runs and all its databases.
        Directory.CreateDirectory(session.SystemDirectory);
        File.Copy(s_licenseDbFile, Path.Combine(session.SystemDirectory, "4.odb"));
        DataCache.MaximumMemoryUse = 10000000000; // 10 GB, set this to what fits your case
        SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;
        session.BeginUpdate();
        BTreeMap<Int64, VelocityDbList<Person>> btreeMap = new BTreeMap<Int64, VelocityDbList<Person>>(null, session);
        session.Persist(btreeMap);
        for (int i = 0; i < 100000; i++)
        {
          Person p = new Person();
          GeoHash geohash = GeoHash.WithBitPrecision(p.Lattitude, p.Longitude);
          VelocityDbList<Person> personList;
          if (btreeMap.TryGetValue(geohash.LongValue, out personList))
            personList.Add(p);
          else
          {
            personList = new VelocityDbList<Person>(1);
            //session.Persist(p);
            personList.Add(p);
            session.Persist(personList);
            btreeMap.Add(geohash.LongValue, personList);
          }
        }
        session.Commit();
      }
    }

    static void Main(string[] args)
    {
      CreateData();
      // Sweden bounding box
      GeoHashQuery.SearchGeoHashIndex(s_systemDir, 55.34, 10.96, 69.06, 24.17);
      // Alaska bounding box
      GeoHashQuery.SearchGeoHashIndex(s_systemDir, 51.21,-169.01, 71.39, -129.99);
      // California bounding box
      GeoHashQuery.SearchGeoHashIndex(s_systemDir, 32.53,-124.42, 42.01, -114.13);
      // USA bounding box
      //GeoHashQuery.SearchGeoHashIndex(s_systemDir, 18.9, -67.0, 71.4, 172.4);
    }
  }
}
