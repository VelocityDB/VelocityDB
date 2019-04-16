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
using VelocityDbSchema.Samples.AllSupportedSample;

namespace NUnitTests
{
  [TestFixture]
  public class ConcurrentUpdaters
  {
    static readonly string windrive = Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = Path.Combine(windrive, "NUnitTestDbs");

    [TestCase(true, 2, true)]
    [TestCase(false, 2, true)]
    [TestCase(true, 2, false)]
    [TestCase(false, 2, false)]
    public void ConcurrentUpdates(bool serverSession, int numberofThreads, bool optimisticLocking)
    {
      using (SessionBase session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates start, Number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }

      using (SessionNoServer session = new SessionNoServer(s_systemDir))
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
              session = new ServerClientSession(s_systemDir, null, 500, optimisticLocking);
            else
              session = new SessionNoServer(s_systemDir, 500, optimisticLocking);
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
      using (SessionBase session = new SessionNoServer(s_systemDir))
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
    [Ignore("Figure out what corrupts schema before enabling")]
    public void ConcurrentUpdatesShared(bool serverSession, int numberofThreads, bool optimisticLocking)
    {
      SessionBase session;
      using (session = new SessionNoServerShared(s_systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates start, Number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }

      using (session = new SessionNoServerShared(s_systemDir))
      {
        session.BeginUpdate();
        FixedSize fixedSize = new FixedSize();
        session.Persist(fixedSize);
        //session.RegisterClass(typeof(Motorcycle));
        session.Commit();
      }
      try
      {
        if (serverSession)
          session = new ServerClientSessionShared(s_systemDir, null, 500, optimisticLocking);
        else
          session = new SessionNoServerShared(s_systemDir, 500, optimisticLocking);
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
      using (session = new SessionNoServer(s_systemDir))
      {
        session.Verify();
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase, false, false);
        if (db != null)
          Console.WriteLine("ConcurrentUpdates finished, number of FixedSize objects: " + db.AllObjects<FixedSize>().Count);
      }
    }

    [Test]
    public void Paralell()
    {
      AllSupported allSuported;
      using (var session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        for (int i = 0; i < 100000; i++)
        {
          allSuported = new AllSupported(3, session);
          session.Persist(allSuported);
        }
        session.Commit();
      }
      int ct = 0;
      using (var session = new SessionNoServerShared(s_systemDir))
      {
        session.BeginUpdate();
        var objects = session.AllObjects<AllSupported>();
        ct = (int)session.AllObjects<AllSupported>().Count;
        System.Threading.Tasks.Parallel.ForEach(objects, (obj) =>
        {
          session.UpdateObject(obj, () =>
          {
            obj.int64 = Int64.MaxValue;
          });
        });
        session.Commit();
      }

      using (var session = new SessionNoServerShared(s_systemDir))
      {
        session.BeginRead();
        var objects = session.AllObjects<AllSupported>(false);
        var wrongValues = new List<int>();
        foreach (var obj in objects)
        {
          if (obj.int64 != Int64.MaxValue)
            wrongValues.Add(ct);
          ++ct;
        }
        ct = 0;
        System.Threading.Tasks.Parallel.ForEach(objects, (obj) =>
        {
          if (obj.int64 != Int64.MaxValue)
            throw new Exception("wrong value");
          Interlocked.Increment(ref ct);
        });
        session.Commit();
      }
    }
  }
}
