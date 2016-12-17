using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using VelocityDbSchema.Samples.AllSupportedSample;
using VelocityDbSchema.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;
using VelocityDb.Exceptions;

namespace NUnitTests
{
  [TestFixture]
  public class MultipleUpdaters
  {
    public const string systemDir = "c:\\NUnitTestDbs";
    static object s_lockObj = new object();

    [Test]
    public void AppendFile()
    {
      Placement place = new Placement(798, 1, 1, 1, UInt16.MaxValue);
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        ObjWithArray a = new ObjWithArray(10);
        a.Persist(place, session);
        session.Commit(); // commit Database 798
      }
      place = new Placement(798, 2, 1, 100, UInt16.MaxValue);
      for (int i = 0; i < 25; i++)
      {
        using (ServerClientSession session = new ServerClientSession(systemDir))
        {
          //session.SetTraceAllDbActivity();
          session.BeginUpdate();
          for (int j = 0; j < 1000; j++)
          {
            ObjWithArray a = new ObjWithArray(j * 10);
            a.Persist(place, session);
          }
          session.FlushUpdates();
          session.FlushUpdatesServers(); // check if this will cause file to be appended
          Database db = session.NewDatabase(3567);
          using (ServerClientSession session2 = new ServerClientSession(systemDir))
          {
            session2.BeginUpdate();
            ObjWithArray a = new ObjWithArray(10);
            a.Persist(place, session2);
            session2.Commit();
          }
          session.Abort(); // appended page space now unused? Need tidy?
        }
      }
    }

    [Test]
    public void TwoUpdaters1()
    {
      Assert.Throws<OptimisticLockingFailed>(() =>
      {
        UInt64 id;
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          Man man = new Man();
          man.Persist(session, man);
          id = man.Id;
          session.Commit();
          session.BeginUpdate();
          man.Age = ++man.Age;
          Database db = session.NewDatabase(3567);
          using (SessionNoServer session2 = new SessionNoServer(systemDir))
          {
            session2.BeginUpdate();
            Man man2 = (Man)session2.Open(id);
            Assert.Less(man2.Age, man.Age);
            man2.Age = ++man.Age;
            session2.Commit();
          }
          session.DeleteDatabase(db);
          session.Commit(); // OptimisticLockingFailed here
        }
      });
    }

    [Test]
    public void TwoUpdaters2()
    {
      Assert.Throws<OpenDatabaseException>(() =>
      {
        UInt64 id;
        try
        {
          using (SessionNoServer session = new SessionNoServer(systemDir))
          {
            session.BeginUpdate();
            Man man = new Man();
            man.Persist(session, man);
            id = man.Id;
            session.Commit();
            session.BeginUpdate();
            man.Age = ++man.Age;
            session.FlushUpdates();
            using (SessionNoServer session2 = new SessionNoServer(systemDir))
            {
              session2.BeginRead();
              Man man2 = (Man)session2.Open(id);
              Assert.Less(man2.Age, man.Age);
              man2.Age = ++man.Age; // We'll get the OpenDatabase exception here since we are not in an update transaction
              session2.Commit();
            }
            session.Commit();
          }
        }
        finally
        {
          System.GC.Collect();
        }
      });
    }

    [Test]
    public void TwoUpdaters3()
    {
      Assert.Throws<OptimisticLockingFailed>(() =>
      {
        UInt64 id;
        try
        {
          using (SessionNoServer session = new SessionNoServer(systemDir))
          {
            session.BeginUpdate();
            Man man = new Man();
            man.Persist(session, man);
            id = man.Id;
            session.Commit();
            session.BeginUpdate();
            man.Age = ++man.Age;
            session.FlushUpdates(); // fStream set for updated databases will cause other write sessions to fail updating these databases
            using (SessionNoServer session2 = new SessionNoServer(systemDir))
            {
              session2.BeginUpdate();
              Man man2 = (Man)session2.Open(id);
              Assert.Less(man2.Age, man.Age);
              man2.Age = ++man.Age;
              session2.Commit(); // OptimisticLockingFailed here
            }
            session.Commit();
          }
        }
        finally
        {
          System.GC.Collect();
        }
      });
    }

    [Test]
    [Repeat(2)]
    public void MultipleThreadsAdding()
    {
      bool doClearAll = SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase;
      SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase = false;
      try
      {
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          session.RegisterClass(typeof(AutoPlacement)); // build in type but not yet registered as a one
          session.RegisterClass(typeof(ObservableList<int>));
          session.RegisterClass(typeof(Dokument));
          UInt32 dbNum = session.DatabaseNumberOf(typeof(Dokument));
          Database db = session.OpenDatabase(dbNum, false, false);
          if (db == null)
            db = session.NewDatabase(dbNum, 0, typeof(Dokument).ToGenericTypeString());
          Dokument doc = new Dokument();
          session.Persist(doc);
          session.Commit();
        }
        using (ServerClientSessionShared sharedReadSession = new ServerClientSessionShared(systemDir))
        {
          sharedReadSession.BeginRead();
          Parallel.ForEach(Enumerable.Range(1, 3), (num) => LockConflict(sharedReadSession));
        }
      }
      finally
      {
        SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase = doClearAll;
      }
    }

    void LockConflict(SessionBase sharedReadSession)
    {
      string host = null;
      Random r = new Random(5);
      SessionPool sessionPool = new SessionPool(3, () => new ServerClientSession(systemDir, host, 2000, false));
      try
      {
        int iCounter = 0;
        int sessionId1 = -1;
        SessionBase session1 = null;
        for (int i = 0; i < 50; i++)
        {
          try
          {
            session1 = sessionPool.GetSession(out sessionId1);
            session1.BeginUpdate();
            Dokument Doc_A = new Dokument();
            Doc_A.Name = "Test A";
            session1.Persist(Doc_A);
            Console.WriteLine(Doc_A.ToString());
            int sessionId2 = -1;
            SessionBase session2 = null;
            try
            {
              session2 = sessionPool.GetSession(out sessionId2);
              session2.BeginUpdate();
              Dokument Doc_B = new Dokument();
              Doc_B.Name = "Test_B";
              session2.Persist(Doc_B);
              Console.WriteLine(Doc_B.ToString());
              session2.Commit();
            }
            finally
            {
              sessionPool.FreeSession(sessionId2, session2);
            }
            session1.Commit();
            sharedReadSession.ForceDatabaseCacheValidation();
            session1.BeginRead();
            ulong ct = session1.AllObjects<Dokument>(false).Count;
            Console.WriteLine("Number of Dokument found by normal session: " + ct);
            session1.Commit();
            ct = sharedReadSession.AllObjects<Dokument>(false).Count;
            Console.WriteLine("Number of Dokument found by shared read session: " + ct);
          }
          finally
          {
            sessionPool.FreeSession(sessionId1, session1);
          }
          iCounter++;
          Console.WriteLine(" -> " + iCounter.ToString());
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        throw;
      }
    }
  }
}