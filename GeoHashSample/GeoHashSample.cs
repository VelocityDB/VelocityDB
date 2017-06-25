using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDbSchema;

namespace GeoHashSample
{
  class GeoHashSample
  {
    static readonly string s_systemDir = "GeoHashSample";
    static readonly string s_licenseDbFile = "c:/4.odb";
    static readonly int s_numberOfSamples = 10000000;
    static void CreateData()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        bool dirExist = Directory.Exists(session.SystemDirectory);
        if (dirExist)
        {
          return;
          Directory.Delete(session.SystemDirectory, true); // remove systemDir from prior runs and all its databases.
        }
        Directory.CreateDirectory(session.SystemDirectory);
        File.Copy(s_licenseDbFile, Path.Combine(session.SystemDirectory, "4.odb"));
        SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;
        session.BeginUpdate();
        CompareGeoObj comparer = new CompareGeoObj();
        var btreeSet = new BTreeSet<GeoObj>(comparer, session, 1000);
        for (int i = 0; i < s_numberOfSamples; i++)
        {
          var g = new GeoObj();
          btreeSet.Add(g);
        }
        session.Persist(btreeSet);
        session.Commit();
        Console.Out.WriteLine($@"Done creating {s_numberOfSamples} GeoHashSample GeoObj ");
      }
    }

    static void Main(string[] args)
    {
      CreateData();
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        int ct = GeoHashQuery.SearchGeoHashIndex(session, 32.715736, -117.161087, 10000).Count;
        Console.WriteLine($@"GeoObj located in San Diego by 10000 meter radius: {ct}");
        ct = GeoHashQuery.SearchGeoHashIndex(session, 32.715736, -117.161087, 100000).Count;
        Console.WriteLine($@"GeoObj located in San Diego by 100000 meter radius: {ct}");
        ct = GeoHashQuery.SearchGeoHashIndex(session, 40.730610, -73.935242, 10000).Count;
        Console.WriteLine($@"GeoObj located in New York City by 10000 meter radius: {ct}");
        ct = GeoHashQuery.SearchGeoHashIndex(session, 40.730610, -73.935242, 100000).Count;
        Console.WriteLine($@"GeoObj located in New York City by 100000 meter radius: {ct}");
        // Sweden bounding box
        ct = GeoHashQuery.SearchGeoHashIndex(session, 55.34, 10.96, 69.06, 24.17).Count;
        Console.WriteLine($@"GeoObj located in Sweden: {ct}");
        // Alaska bounding box
        ct = GeoHashQuery.SearchGeoHashIndex(session, 51.21, -169.01, 71.39, -129.99).Count;
        Console.WriteLine($@"GeoObj located in Alaska: {ct}");
        // California bounding box
        ct = GeoHashQuery.SearchGeoHashIndex(session, 32.53, -124.42, 42.01, -114.13).Count;
        Console.WriteLine($@"GeoObj located in California: {ct}");
        // USA bounding box
        //GeoHashQuery.SearchGeoHashIndex(session, 18.9, -67.0, 71.4, 172.4);
        //Console.WriteLine($@"Persons located in USA: {ct}");
      }
    }
  }
}
