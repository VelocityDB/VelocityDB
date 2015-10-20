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

    [TestCase(true, 7)]
    [TestCase(false, 5)]
    public void ConcurrentUpdates(bool serverSession, int numberofThreads)
    {
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
              session = new ServerClientSession(systemDir, null, 500);
            else
              session = new SessionNoServer(systemDir, 500);
            try
            {
              for (int j = 0; j < 30; j++)
                try
                {
                  session.BeginUpdate();
                  session.SetTraceDbActivity(878);
                  session.CrossTransactionCacheAllDatabases();
                  for (int k = 0; k < 5000; k++)
                  {
                    FixedSize fixedSize = new FixedSize();
                    session.Persist(fixedSize);
                    if (k == 9000 && Thread.CurrentThread.ManagedThreadId % 3 != 0)
                      session.FlushUpdates();
                  }
                  session.Commit();
                  if (!serverSession)
                    session.Compact();
                  Console.WriteLine("Commit OK for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                }
                catch (OptimisticLockingFailed ex)
                {
                  session.Abort();
                  Console.WriteLine("Commit failed (OptimisticLockingFailed) for thread " + Thread.CurrentThread.ManagedThreadId + " Transaction: " + j);
                  Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                  session.Abort();
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
    }
  }
}
