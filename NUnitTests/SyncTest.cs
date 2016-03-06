using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDBExtensions;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class SyncTest 
  {
    static readonly string s_sync1 = "sync1";
    static readonly string s_sync2 = "sync2";
    static readonly string s_sync3 = "sync3";

    [Test]
    public void aaaCleanupPriorRuns()
    {
      using (SessionBase session = new SessionNoServer(s_sync1))
      {
        string dir = session.SystemDirectory;
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
      }
      using (SessionBase session = new SessionNoServer(s_sync2))
      {
        string dir = session.SystemDirectory;
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
      }
      using (SessionBase session = new SessionNoServer(s_sync3))
      {
        string dir = session.SystemDirectory;
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
      }
    }

    [Test]
    public void aSyncNewDatabases()
    {
      using (SessionBase session = new SessionNoServer(s_sync1))
      {
        session.EnableSyncByTrackingChanges = true;
        using (var trans = session.BeginUpdate())
        {
          for (uint i = 10; i < 50; i++)
          {
            var database = session.NewDatabase(i);
            Assert.NotNull(database);
          }
          session.Commit();
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
            updateSession.SyncWith(readFromSession);
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        readFromSession.BeginRead();
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          using (var trans = updateSession.BeginRead())
          {
            Assert.AreEqual(updateSession.OpenAllDatabases().Count, readFromSession.OpenAllDatabases().Count - 1); // - 1 due to additional change tracking databases in original 
          }
        }
      }
    }

    [Test]
    public void bSyncDeletedDatabases()
    {
      using (SessionBase session = new SessionNoServer(s_sync1))
      {
        using (var trans = session.BeginUpdate())
        {
          for (uint i = 10; i < 14; i++)
          {
            var database = session.OpenDatabase(i);
            session.DeleteDatabase(database);
          }
          session.Commit();
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          updateSession.SyncWith(readFromSession);
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        readFromSession.BeginRead();
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          using (var trans = updateSession.BeginRead())
          {
            Assert.AreEqual(updateSession.OpenAllDatabases().Count, readFromSession.OpenAllDatabases().Count - 1); // - 1 due to additional change tracking databases in original 
          }
        }
      }
    }

    [Test]
    public void cSyncNewPages()
    {
      using (SessionBase session = new SessionNoServer(s_sync1))
      {
        using (var trans = session.BeginUpdate())
        {
          for (uint i = 0; i < 100; i++)
          {
            FourPerPage fourPerPage = new FourPerPage(i);
            session.Persist(fourPerPage);
          }
          session.Commit();
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          updateSession.SyncWith(readFromSession);
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        readFromSession.BeginRead();
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          using (var trans = updateSession.BeginRead())
          {
            Assert.AreEqual(updateSession.AllObjects<FourPerPage>().Count, readFromSession.AllObjects<FourPerPage>().Count);
          }
        }
      }
    }

    [Test]
    public void dSyncDeletePages()
    {
      using (SessionBase session = new SessionNoServer(s_sync1))
      {
        using (var trans = session.BeginUpdate())
        {
          foreach (FourPerPage fourPerPage in session.AllObjects<FourPerPage>())
            fourPerPage.Unpersist(session);
          session.Commit();
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          updateSession.SyncWith(readFromSession);
        }
      }

      using (SessionBase readFromSession = new SessionNoServer(s_sync1))
      {
        readFromSession.BeginRead();
        using (SessionBase updateSession = new SessionNoServer(s_sync2))
        {
          using (var trans = updateSession.BeginRead())
          {
            Assert.AreEqual(updateSession.AllObjects<FourPerPage>().Count, readFromSession.AllObjects<FourPerPage>().Count);
          }
        }
      }
    }
  }
}
