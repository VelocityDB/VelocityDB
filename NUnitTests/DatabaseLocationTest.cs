using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class DatabaseLocationTest
  {
    const UInt32 locationStartDbNum = 10000;
    const UInt32 locationEndDbNum = 20000; 
    const string systemDir = "c:\\NUnitTestDbs\\";
    string location2Dir = systemDir + locationStartDbNum + "To" + locationEndDbNum;

    [Test]
    public void CreateLocationSameHost()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        DatabaseLocation remoteLocation = new DatabaseLocation(Dns.GetHostName(), location2Dir, locationStartDbNum, locationEndDbNum, session, PageInfo.compressionKind.LZ4, 0);
        remoteLocation = session.NewLocation(remoteLocation);
        Database database = session.NewDatabase(remoteLocation.StartDatabaseNumber);
        Assert.NotNull(database);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Database database = session.OpenDatabase(locationStartDbNum, false);
        Assert.NotNull(database);
        session.DeleteDatabase(database);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        session.DeleteLocation(session.DatabaseLocations.LocationForDb(locationStartDbNum));
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        session.Commit();
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CreateLocationSameHostServer(bool optimisticLocking)
    {
      using (ServerClientSession session = new ServerClientSession(systemDir, Dns.GetHostName(), 2000, optimisticLocking))
      {
        session.BeginUpdate();
        DatabaseLocation remoteLocation = new DatabaseLocation(Dns.GetHostName(), location2Dir, locationStartDbNum, locationEndDbNum, session, PageInfo.compressionKind.LZ4, 0);
        remoteLocation = session.NewLocation(remoteLocation);
        Database database = session.NewDatabase(remoteLocation.StartDatabaseNumber);
        Assert.NotNull(database);
        session.Commit();
      }
      using (ServerClientSession session = new ServerClientSession(systemDir, Dns.GetHostName(), 2000, optimisticLocking))
      {
        session.BeginUpdate();
        Database database = session.OpenDatabase(locationStartDbNum, false);
        Assert.NotNull(database);
        session.DeleteDatabase(database);
        session.Commit();
      }
      using (ServerClientSession session = new ServerClientSession(systemDir, Dns.GetHostName(), 2000, optimisticLocking))
      {
        session.BeginUpdate();
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        session.DeleteLocation(session.DatabaseLocations.LocationForDb(locationStartDbNum));
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        session.Commit();
      }
    }
  }
}
