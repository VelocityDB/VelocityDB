using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class ConcurrentUpdaters
  {
    static readonly string windrive = Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string systemDir = Path.Combine(windrive, "NUnitTestDbs");

    [TestCase(true, 3, true)]
    [TestCase(false, 3, true)]
    [TestCase(true, 3, false)]
    [TestCase(false, 3, false)]
    public void ConcurrentUpdates(bool serverSession, int numberofThreads, bool optimisticLocking)
    {
      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates start, Number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        FixedSize fixedSize = new FixedSize();
        session.Persist(fixedSize);
        session.Commit();
      }
      Thread[] threads = new Thread[numberofThreads];
      for (int i = 0; i < numberofThreads; i++)
        threads[i] = new Thread(() =>
          {
            SessionBase session;
            if (serverSession)
              session = new ServerClientSession(systemDir, null, 500, optimisticLocking);
            else
              session = new SessionNoServer(systemDir, 500, optimisticLocking);
            try
            {
              for (int j = 0; j < 10; j++)
                try
                {
                  using (var transaction = session.BeginUpdate())
                  {
                    session.SetTraceDbActivity(FixedSize.PlaceInDatabase);
                    session.CrossTransactionCacheAllDatabases();
                    for (int k = 0; k < 4200; k++)
                    {
                      FixedSize fixedSize = new FixedSize();
                      session.Persist(fixedSize);
                      if (k == 4000 && Thread.CurrentThread.ManagedThreadId % 3 != 0)
                        session.FlushUpdates();
                    }
                    session.Commit();
                    if (!serverSession)
                      session.Compact();
                    Console.WriteLine("Commit OK for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  }
                }
                catch (PageUpdateLockException ex)
                {
                  Console.WriteLine("Commit failed (OptimisticLockingFailed) for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
                }
                catch (OptimisticLockingFailed ex)
                {
                  Console.WriteLine("Commit failed (OptimisticLockingFailed) for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                  Console.WriteLine("Commit failed for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
                }
            }
            finally
            {
              session.Dispose();
            }
          });
      foreach (Thread thread in threads)
        thread.Start();
      bool keepWaiting = true;
      while (keepWaiting)
      {
        keepWaiting = false;
        foreach (Thread thread in threads)
          if (thread.IsAlive)
          {
            keepWaiting = true;
            thread.Join(5000);
          }
      }
      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates finished, number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }
    }
  }
}
