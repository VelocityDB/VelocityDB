using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.Session;
using VelocityDbSchema;
using VelocityGraph;

namespace NUnitTests
{
  [TestFixture]
  public class ADatabaseIteration
  {
    static readonly string drive = "c:\\";
    static readonly string licenseDbFile = Path.Combine(drive, "4.odb");
    public const string systemDir = "c:/NUnitTestDbs";
    public const string location2Dir = "c:/NUnitTestDbsLocation2";


    [Test]
    public void aaaDeleteDatabasesFromPriorRun()
    {
      SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;

      if (Directory.Exists(systemDir))
      {
        foreach (string s in Directory.GetFiles(systemDir))
          File.Delete(s);
        foreach (string s in Directory.GetDirectories(systemDir))
          Directory.Delete(s, true);
      }
      else
        Directory.CreateDirectory(systemDir);
      if (Directory.Exists(location2Dir))
      {
        foreach (string s in Directory.GetFiles(location2Dir))
          File.Delete(s);
        foreach (string s in Directory.GetDirectories(location2Dir))
          Directory.Delete(s, true);
      }
      else
        Directory.CreateDirectory(location2Dir);
    }

    [Test]
    public void aaaFakeLicenseDatabase()
    {
      Assert.Throws<NoValidVelocityDBLicenseFoundException>(() =>
      {
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          Database database;
          License license = new License("Mats", 1, null, null, null, 99999, DateTime.MaxValue, 9999, 99, 9999);
          Placement placer = new Placement(License.PlaceInDatabase, 1, 1, 1);
          license.Persist(placer, session);
          for (uint i = 10; i < 20; i++)
          {
            database = session.NewDatabase(i);
            Assert.NotNull(database);
          }
          session.Commit();
          File.Copy(Path.Combine(systemDir, "20.odb"), Path.Combine(systemDir, "4.odb"));
          session.BeginUpdate();
          for (uint i = 21; i < 30; i++)
          {
            database = session.NewDatabase(i);
            Assert.NotNull(database);
          }
          session.RegisterClass(typeof(VelocityDbSchema.Samples.Sample1.Person));
          Graph g = new Graph(session);
          session.Persist(g);
          session.Commit();
        }
      });
    }

    [TestCase(true, true)]
    public void aaaFakeLicenseDatabaseCleanup(bool deleteLocationProperly, bool useServerSession)
    {
      if (deleteLocationProperly)
        using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          DatabaseLocation defaultLocation = session.DatabaseLocations.Default();
          List<Database> dbList = session.OpenLocationDatabases(defaultLocation, true);
          foreach (Database db in dbList)
            if (db.DatabaseNumber > Database.InitialReservedDatabaseNumbers)
              session.DeleteDatabase(db);
          session.DeleteLocation(defaultLocation);
          session.Commit();
        }
      foreach (string s in Directory.GetFiles(systemDir))
        File.Delete(s);
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(NotSharingPage));
        session.NewDatabase(dbNum);
        session.Commit();
      }
      File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
    }

    [TestCase(100000)]
    public void CreateDataAndIterateDb(int numObj)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.Verify();
        session.BeginUpdate();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(NotSharingPage));
        Database db = session.OpenDatabase(dbNum, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeA));
        db = session.OpenDatabase(dbNum, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeB));
        db = session.OpenDatabase(dbNum, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        session.Commit();
      }

      using (var session = new SessionNoServerShared(systemDir))
      {
        session.Verify();
        session.BeginUpdate();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeB));
        Placement place = new Placement(dbNum, 100);
        for (int i = 0; i < numObj; i++)
        {
          NotSharingPage ns = new NotSharingPage();
          session.Persist(ns);
          SharingPageTypeA sA = new SharingPageTypeA();
          session.Persist(sA);
          SharingPageTypeB sB = new SharingPageTypeB();
          if (i % 5 == 0)
            sB.Persist(session, place);
          else if (i % 1001 == 0)
            sB.Persist(session, sA);
          else if (i % 3001 == 0)
            sB.Persist(session, ns);
          else
            session.Persist(sB);
        }
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        session.Verify();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(NotSharingPage));
        Database db = session.OpenDatabase(dbNum);
        AllObjects<NotSharingPage> all = db.AllObjects<NotSharingPage>();
        ulong ct = all.Count();
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeA));
        OfType ofType = db.OfType(typeof(NotSharingPage));
        ulong ct2 = ofType.Count();
        Assert.AreEqual(ct, ct2);
        Database dbA = session.OpenDatabase(dbNum);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeB));
        Database dbB = session.OpenDatabase(dbNum);
        AllObjects<SharingPageTypeA> allA = dbA.AllObjects<SharingPageTypeA>();
        AllObjects<SharingPageTypeB> allB = dbB.AllObjects<SharingPageTypeB>();
        OfType allA2 = dbA.OfType(typeof(SharingPageTypeA));
        int start = numObj / 2;
        NotSharingPage ns = all.ElementAt(numObj);
        SharingPageTypeA sA = allA.ElementAt(numObj);
        SharingPageTypeA sA2 = (SharingPageTypeA)allA2.ElementAt(numObj);
        Assert.AreEqual(sA, sA2);
        sA = allA.ElementAt(10);
        sA2 = (SharingPageTypeA)allA2.ElementAt(10);
        Assert.AreEqual(sA, sA2);
        //MethodInfo method = typeof(Database).GetMethod("AllObjects");
        //MethodInfo generic = method.MakeGenericMethod(sA.GetType());
        //dynamic itr = generic.Invoke(dbA, new object[]{ true });
        //SharingPageTypeA sAb = itr.ElementAt(numObj);
        //Assert.AreEqual(sA, sAb);
        //SharingPageTypeA sAc = itr.ElementAt(numObj);
        SharingPageTypeB sB = allB.ElementAt(numObj);
        List<NotSharingPage> notSharingPageList = all.Skip(100).ToList();
        List<SharingPageTypeA> sharingPageTypeA = allA.Take(5).Skip(100).ToList();
        for (int i = start; i < numObj; i++)
          sA = allA.ElementAt(i);
        for (int i = start; i < numObj; i += 5)
          ns = all.ElementAt(i);
        for (int i = start; i < numObj; i += 5)
          sB = allB.ElementAt(i);
        for (int i = 0; i < numObj; i += 45000)
          ns = all.ElementAt(i);
        int allB_count = (int) allB.Count();
        for (int i = 0; i < allB_count - 1; i++)
        {
          Assert.NotNull(allB.ElementAt(i));
        }
        session.Commit();
        session.BeginUpdate();
        session.DeleteDatabase(db);
        session.DeleteDatabase(dbA);
        session.DeleteDatabase(dbB);
        session.Commit();
      }
    }
    [TestCase(50000)]
    public void CreateDataAndIterateSession(int numObj)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(NotSharingPage));
        Database db = session.OpenDatabase(dbNum, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeA));
        db = session.OpenDatabase(dbNum, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeB));
        db = session.OpenDatabase(dbNum, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeB));
        Placement place = new Placement(dbNum, 100);
        for (int i = 0; i < numObj; i++)
        {
          NotSharingPage ns = new NotSharingPage();
          session.Persist(ns);
          SharingPageTypeA sA = new SharingPageTypeA();
          session.Persist(sA);
          SharingPageTypeB sB = new SharingPageTypeB();
          if (i % 5 == 0)
            sB.Persist(session, place);
          else if (i % 1001 == 0)
            sB.Persist(session, sA);
          else if (i % 3001 == 0)
            sB.Persist(session, ns);
          else
            session.Persist(sB);
        }
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(NotSharingPage));
        Database db = session.OpenDatabase(dbNum);
        AllObjects<NotSharingPage> all = session.AllObjects<NotSharingPage>(true, false);
        OfType all2 = session.OfType(typeof(NotSharingPage), true, false);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeA));
        Database dbA = session.OpenDatabase(dbNum);
        dbNum = session.DatabaseNumberOf(typeof(SharingPageTypeB));
        Database dbB = session.OpenDatabase(dbNum);
        AllObjects<SharingPageTypeA> allA = session.AllObjects<SharingPageTypeA>(true, false);
        AllObjects<SharingPageTypeB> allB = session.AllObjects<SharingPageTypeB>(true, false);
        int start = numObj / 2;
        NotSharingPage ns = all.ElementAt(numObj - 1); // zero based index so deduct one
        NotSharingPage ns2 = (NotSharingPage)all2.ElementAt(numObj - 1);
        Assert.AreEqual(ns, ns2);
        SharingPageTypeA sA = allA.ElementAt(15);
        SharingPageTypeB sB = allB.ElementAt(10);
        for (int i = start; i < numObj; i++)
          ns = all.ElementAt(i);
        //for (int i = start; i < numObj; i++)
        //  ns = all.Skip(i).T
        //for (int i = start; i < numObj; i++)
        //  sA = allA.ElementAt(i);
        all.Skip(100);
        all2.Skip(100);
        for (int i = start; i < numObj; i += 5)
        {
          ns = all.ElementAt(i);
          ns2 = (NotSharingPage)all2.ElementAt(i);
          Assert.AreEqual(ns, ns2);
        }
        for (int i = 5; i < 100; i += 5)
          sB = allB.ElementAt(i);
        for (int i = 0; i < numObj; i += 45000)
          ns = all.ElementAt(i);
        session.Commit();
        session.BeginUpdate();
        session.DeleteDatabase(db);
        session.DeleteDatabase(dbA);
        session.DeleteDatabase(dbB);
        session.Commit();
      }
    }
  }
}
