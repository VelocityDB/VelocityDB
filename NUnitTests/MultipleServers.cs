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
using NUnit.Framework;
using System.IO;

namespace NUnitTests
{
  [TestFixture]
  public class MultipleServers
  {
    public const string systemDir = "c:/NUnitTestDbs";
    public const string copyDir = @"c:/NUnitTestDbs/copy";
    public const string systemDir2 = "c:/NUnitTestDbs2";
    public const string location2Dir = "c:/NUnitTestDbsLocation2";
    public string systemHost2 = "Asus";
    public string systemHost = "Asus2";
    //public string systemHost2 = "AcerLaptop";
    public string systemHost3 = "FindPriceBuy";
    //public string systemHost2 = "WINDOWS-9HKQ9DL";
    //public string systemHost2 = "192.168.1.75";
    //public string systemHost2 = "Vostro1720";
   //public string systemHost3 = "Gateway64bitPC";
     //public string systemHost2 = "Lidia-PC";

    [Test]  
    public void multipleServersInvalid()
    {
      Assert.Throws<InvalidChangeOfDefaultLocationException>(() =>
      {
        using (ServerClientSession session = new ServerClientSession(systemDir))
        {
          session.SetTraceDbActivity(2);
          try
          {
            DatabaseLocation localLocation = new DatabaseLocation(systemHost, location2Dir, 10000, 20000, session, PageInfo.compressionKind.LZ4, 0);
            Placement place = new Placement(10000, 2);
            session.BeginUpdate();
            session.NewLocation(localLocation);
            Man aMan = null;
            Woman aWoman = null;

            for (int j = 1; j <= 5; j++)
            {
              aMan = new Man(null, aMan, DateTime.UtcNow);
              aMan.Persist(place, session);
              aWoman = new Woman(aMan, aWoman);
              aWoman.Persist(place, session);
              aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
              if (j % 1000 == 0)
              {
                session.FlushUpdates();
              }
            }
            localLocation = new DatabaseLocation(systemHost, systemDir, 20001, 30000, session, PageInfo.compressionKind.None, 0);
            session.NewLocation(localLocation);
            place = new Placement(20001);
            //localDatabase = session.NewDatabase(20001, localLocation);
            for (int j = 1; j <= 5; j++)
            {
              aMan = new Man(null, aMan, DateTime.UtcNow);
              aMan.Persist(place, session);
              aWoman = new Woman(aMan, aWoman);
              aWoman.Persist(place, session);
              aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
              if (j % 1000 == 0)
              {
                session.FlushUpdates();
              }
            }
            DatabaseLocation serverLocation = new DatabaseLocation(systemHost2, location2Dir, 30001, 40000, session, PageInfo.compressionKind.LZ4, 0);
            session.NewLocation(serverLocation);
            place = new Placement(30001);
            for (int j = 1; j <= 5; j++)
            {
              aMan = new Man(null, aMan, DateTime.UtcNow);
              aMan.Persist(place, session);
              aWoman = new Woman(aMan, aWoman);
              aWoman.Persist(place, session);
              aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
              if (j % 1000 == 0)
              {
                session.FlushUpdates();
              }
            }
            session.Commit();
          }
          finally
          {
            //session.Close();
          }
        }
      });
    }

    [Test]
    [Repeat(3)] // remove when propagtion of optimistic locking flag is done to slave database locations TO DO (issue caused by CopyAllDatabasdesTo that uses pessimistic locking)
    public void multipleServersOK()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        session.Commit();
      }

      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {
        session.SetTraceDbActivity(0);
        DatabaseLocation localLocation = new DatabaseLocation(systemHost, location2Dir, 10000, 20000, session, PageInfo.compressionKind.LZ4, 0);
        Placement place = new Placement(10000, 2);
        session.BeginUpdate();
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        Console.WriteLine();
        session.NewLocation(localLocation);
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        Man aMan = null;
        Woman aWoman = null;

        for (int j = 1; j <= 5; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          aMan.Persist(place, session);
          aWoman = new Woman(aMan, aWoman);
          aWoman.Persist(place, session);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000 == 0)
          {
            session.FlushUpdates();
          }
        }
        localLocation = new DatabaseLocation(systemHost2, systemDir, 20001, 30000, session, PageInfo.compressionKind.LZ4, 0);
        session.NewLocation(localLocation);
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        place = new Placement(20001);
        //localDatabase = session.NewDatabase(20001, localLocation);
        for (int j = 1; j <= 5; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          aMan.Persist(place, session);
          aWoman = new Woman(aMan, aWoman);
          aWoman.Persist(place, session);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000 == 0)
          {
            session.FlushUpdates();
          }
        }
        DatabaseLocation serverLocation = new DatabaseLocation(systemHost2, location2Dir, 30001, 40000, session, PageInfo.compressionKind.LZ4, 0);
        session.NewLocation(serverLocation);
        foreach (DatabaseLocation loc in session.DatabaseLocations)
          Console.WriteLine(loc.ToStringDetails(session, false));
        place = new Placement(30001);
        for (int j = 1; j <= 5; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          aMan.Persist(place, session);
          aWoman = new Woman(aMan, aWoman);
          aWoman.Persist(place, session);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000 == 0)
          {
            session.FlushUpdates();
          }
        }
        localLocation = new DatabaseLocation(systemHost3, systemDir, 40001, 50000, session, PageInfo.compressionKind.None, 0);
        session.NewLocation(localLocation);
        place = new Placement(40001);
        //localDatabase = session.NewDatabase(20001, localLocation);
        for (int j = 1; j <= 5; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          aMan.Persist(place, session);
          aWoman = new Woman(aMan, aWoman);
          aWoman.Persist(place, session);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000 == 0)
          {
            session.FlushUpdates();
          }
        }
        session.Commit();
      }

      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {
        session.CopyAllDatabasesTo(copyDir);
        using (SessionNoServer copySession = new SessionNoServer(copyDir))
        {
          copySession.Verify();
        }
      }

      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost, 2000, false)) // TO DO, change back to use optimistic locking
      {
        //session.SetTraceDbActivity(0);
        session.BeginUpdate();
        Database db = session.OpenDatabase(10000);
        session.DeleteDatabase(db);
        db = session.OpenDatabase(20001);
        session.DeleteDatabase(db);
        db = session.OpenDatabase(30001);
        session.DeleteDatabase(db);
        db = session.OpenDatabase(40001);
        session.DeleteDatabase(db);
        session.Commit();
        Directory.Delete(copyDir, true);
      }

      System.GC.Collect();
      System.GC.WaitForPendingFinalizers();

      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {
        Assert.True(session.OptimisticLocking);
      }
    }

   [Test]
   public void multipleServersOKCleanup()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost, 2000, false)) // TO DO, change back to use optimistic locking
      {
        //session.SetTraceDbActivity(0);
        session.BeginUpdate();
        Database db = session.OpenDatabase(10000, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        db = session.OpenDatabase(20001, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        db = session.OpenDatabase(30001, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        db = session.OpenDatabase(40001, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        session.Commit();
        if (Directory.Exists(copyDir))
          Directory.Delete(copyDir, true);
      }

      System.GC.Collect();
      System.GC.WaitForPendingFinalizers();

      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {
        Assert.True(session.OptimisticLocking);
      }
    }

    [TestCase(50)]
    public void serverSessionOverhead(int numberOfSessions)
    {
      for (int i = 0; i < numberOfSessions; i++)
      {
        using (ServerClientSession session = new ServerClientSession(systemDir2, systemHost3))
        {
          session.BeginUpdate();
          session.Commit();
        }
      }
    }
  }
}
