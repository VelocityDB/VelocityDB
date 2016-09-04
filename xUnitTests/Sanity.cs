using System;
using System.Collections.Generic;
using System.IO;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using Xunit;

namespace xUnitTests
{
  public class ADatabaseIteration
  {
    static readonly string drive = "c:\\";
    static readonly string licenseDbFile = Path.Combine(drive, "4.odb");
    public const string systemDir = "c:/NUnitTestDbs";
    public const string location2Dir = "c:/NUnitTestDbsLocation2";

    [Fact]
    public void PassingTest()
    {
      Assert.Equal(4, Add(2, 2));
    }

    int Add(int x, int y)
    {
      return x + y;
    }

    [Fact]
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

    [Fact]
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
          session.RegisterClass(typeof(BTreeSet<int>));
          session.RegisterClass(typeof(BTreeSet<long>));
          session.RegisterClass(typeof(BTreeSet<DateTime>));
          session.RegisterClass(typeof(BTreeSet<double>));
          session.RegisterClass(typeof(BTreeMap<string, double>));
          session.Commit();
        }
      });
    }

    [Theory]
    [InlineData(true, true)]
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
  }

  public class NotSharingPage : OptimizedPersistable
  {
    static long ct = 0;
    long someData;
    public NotSharingPage()
    {
      someData = ++ct;
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }
  }
}
