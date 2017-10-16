using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.Session;
using VelocityDbSchema.Indexes;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class ConcurrentUpdaters
  {
    static readonly string windrive = Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string systemDir = Path.Combine(windrive, "NUnitTestDbs");

    [TestCase(true, 2, true)]
    [TestCase(false, 2, true)]
    [TestCase(true, 2, false)]
    [TestCase(false, 2, false)]
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
                    session.SetTraceDbActivity(2);
                    Trace.Listeners.Add(new ConsoleTraceListener());
                    session.CrossTransactionCacheAllDatabases();
                    for (int k = 0; k < 4200; k++)
                    {
                      FixedSize fixedSize = new FixedSize();
                      session.Persist(fixedSize);
                      if (k == 4000 && Thread.CurrentThread.ManagedThreadId % 3 != 0)
                        session.FlushUpdates();
                    }
                    transaction.Commit();
                  }
                  if (!serverSession)
                    session.Compact();
                  Console.WriteLine("Commit OK for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
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
            thread.Join(500);
          }
      }
      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.Verify();
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates finished, number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }
    }

    [TestCase(true, 5, false)]
    [TestCase(false, 5, false)]
    public void ConcurrentUpdatesShared(bool serverSession, int numberofThreads, bool optimisticLocking)
    {
      SessionBase session;
      using (session = new SessionNoServerShared(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates start, Number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }

      using (session = new SessionNoServerShared(systemDir))
      {
        session.BeginUpdate();
        FixedSize fixedSize = new FixedSize();
        session.Persist(fixedSize);
        session.Commit();
      }
      try
      {
        if (serverSession)
          session = new ServerClientSessionShared(systemDir, null, 500, optimisticLocking);
        else
          session = new SessionNoServerShared(systemDir, 500, optimisticLocking);
        Thread[] threads = new Thread[numberofThreads];
        using (var transaction = session.BeginUpdate())
        {
          for (int i = 0; i < numberofThreads; i++)
            threads[i] = new Thread(() =>
            {
              for (int j = 0; j < 10; j++)
                try
                {

                  session.SetTraceDbActivity(FixedSize.PlaceInDatabase);
                  session.SetTraceDbActivity(2);
                  Trace.Listeners.Add(new ConsoleTraceListener());
                  for (int k = 0; k < 4200; k++)
                  {
                    FixedSize fixedSize = new FixedSize();
                    session.Persist(fixedSize);
                    var mc = new Motorcycle();
                    session.Persist(mc);
                    if (k == 4000 && Thread.CurrentThread.ManagedThreadId % 3 != 0)
                      session.FlushUpdates();
                  }
                  Console.WriteLine("OK for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                }
                catch (PageUpdateLockException ex)
                {
                  Console.WriteLine("failed (OptimisticLockingFailed) for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
                }
                catch (OptimisticLockingFailed ex)
                {
                  Console.WriteLine("failed (OptimisticLockingFailed) for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                  Console.WriteLine("failed for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
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
                thread.Join(500);
              }
          }
        session.Commit();
        }
      }
      finally
      {
        session.Dispose();
      }
      using (session = new SessionNoServer(systemDir))
      {
        session.Verify();
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates finished, number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }
    }
  }
}
