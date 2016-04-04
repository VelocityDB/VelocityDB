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
    public void MultipleThreadsAdding()
    {
      bool doClearAll = SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase;
      SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase = false;
      try
      {
        Parallel.ForEach(Enumerable.Range(1, 3), (num) => LockConflict());
      }
      finally
      {
        SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase = doClearAll;
      }
    }

    void LockConflict()
    {
      string host = null;
      Random r = new Random(5);
      SessionPool sessionPool = new SessionPool(2, () => new ServerClientSession(systemDir, host, 5000, false));
      int sessionId0 = -1;
      SessionBase Session0 = null;
      try
      {
        int iCounter = 0;
        Placement Place_A = new Placement(100, (UInt16)Math.Max(r.Next() % UInt16.MaxValue, 1));
        Placement Place_B = new Placement(1001, (UInt16)Math.Max(r.Next() % UInt16.MaxValue, 1));
        lock (s_lockObj)
        {
          try
          {
            Session0 = sessionPool.GetSession(out sessionId0);
            Session0.BeginUpdate();
            Session0.RegisterClass(typeof(Dokument));
            Session0.Commit();
          }
          finally
          {
            sessionPool.FreeSession(sessionId0, Session0);
          }
        }

        int sessionId1 = -1;
        SessionBase Session1 = null;
        for (int i = 0; i < 80; i++)
        {
          try
          {
            Session1 = sessionPool.GetSession(out sessionId1);
            Session1.BeginUpdate();
            Dokument Doc_A = new Dokument();
            Doc_A.Name = "Test A";
            while (true)
            {
              try
              {
                Doc_A.Persist(Place_A, Session1);
                Console.WriteLine(Doc_A.ToString());
                break;
              }
              catch (Exception ex)
              {
                if (ex is PageUpdateLockException || ex is PageReadLockException)
                {
                  Place_A.TryPageNumber = (UInt16)Math.Max(r.Next() % UInt16.MaxValue, 1);
                }
                else
                  throw;
                Console.WriteLine(ex.Message);
              }
            }
            int sessionId2 = -1;
            SessionBase Session2 = null;
            try
            {
              Session2 = sessionPool.GetSession(out sessionId2);
              Session2.BeginUpdate();
              Dokument Doc_B = new Dokument();
              Doc_B.Name = "Test_B";
              while (true)
              {
                try
                {
                  Doc_B.Persist(Place_B, Session2);
                  Console.WriteLine(Doc_B.ToString());
                  break;
                }
                catch (Exception ex)
                {
                  if (ex is PageUpdateLockException || ex is PageReadLockException)
                  {
                    Place_B.TryPageNumber = (UInt16)Math.Max(r.Next() % UInt16.MaxValue, 1);
                  }
                  else
                    throw;
                  Console.WriteLine(ex.Message);
                }
              }
              Session2.Commit();
            }
            finally
            {
              sessionPool.FreeSession(sessionId2, Session2);
            }
            Session1.Commit();
            Session1.BeginRead();
            Database db = Session1.OpenDatabase(100);
            ulong ct = db.AllObjects<Dokument>(false).Count;
            db = Session1.OpenDatabase(1001);
            ct += db.AllObjects<Dokument>(false).Count;
            Console.WriteLine("Number of Dokument found: " + ct);
            Session1.Commit();
          }
          finally
          {
            sessionPool.FreeSession(sessionId1, Session1);
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