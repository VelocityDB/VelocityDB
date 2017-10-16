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

namespace NUnitTests
{
  [TestFixture]
  public class ReadersWithUpdater
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    [Test]
    public void SingleReaderSingleUpdater1()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Man man = new Man();
        man.Persist(session, man);
        session.Commit();
      }
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(systemDir, 5000))
      {
        session.BeginUpdate();
        Man man = new Man();
        man.Persist(session, man);
        id = man.Id;
        using (SessionNoServer session2 = new SessionNoServer(systemDir))
        {
          session2.BeginRead();
          Man man2 = (Man)session2.Open(id);
          Assert.Null(man2);
          session2.Commit();
        }
        session.Commit();
      }
    }

    [Test]
    public void SingleReaderSingleUpdater2()
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
        using (var session2 = new SessionNoServerShared(systemDir))
        {
          session2.BeginRead();
          Man man2 = (Man)session2.Open(id);
          Assert.Less(man2.Age, man.Age);
          session2.Commit();
        }
        session.Commit();
      }
      System.GC.Collect();
    }

    [Test]
    public void SingleReaderSingleUpdater3()
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
        session.FlushUpdates();
        using (SessionNoServer session2 = new SessionNoServer(systemDir))
        {
          session2.BeginRead();
          Man man2 = (Man)session2.Open(id);
          Assert.Less(man2.Age, man.Age);
          session2.Commit();
        }
        session.Commit();
      }
    }

    [TestCase(true)]
    [TestCase(false)] // test will fail if pessimistic locking is used
    public void SingleReaderSingleUpdater4(bool useReaderCommit)
    {
      using (SessionNoServer updater = new SessionNoServer(systemDir, 5000)) 
      using (SessionNoServer reader = new SessionNoServer(systemDir, 5000))
      {
        //updater.SetTraceAllDbActivity();
        //reader.SetTraceAllDbActivity();
        updater.BeginUpdate();
        UInt32 dbNum = updater.DatabaseNumberOf(typeof(Man));
        Database db = updater.OpenDatabase(dbNum, true, false);
        if (db != null)
          updater.DeleteDatabase(db);
        updater.Commit();
        updater.BeginUpdate();
        Man man = new Man();
        for (int i = 0; i < 100; i++)
        {
          man = new Man();
          updater.Persist(man);
        }
        updater.Commit();
        reader.BeginRead();
        updater.BeginUpdate();
        db = reader.OpenDatabase(dbNum);
        foreach (Page page in db)
          Assert.True(page.PageInfo.VersionNumber == 1);
        if (useReaderCommit)
          reader.Commit();
        if (useReaderCommit)
          reader.BeginRead();
        else
          reader.ForceDatabaseCacheValidation();
        for (int i = 1; i < 25; i++)
        {
          db = reader.OpenDatabase(dbNum);
          foreach (Page page in db)
          {
            if (page.PageNumber > 1) // skip AutoPlacemnt page
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i);
              Man man2 = (Man)reader.Open(dbNum, page.PageNumber, 1, false);
              Man manUpdated = (Man)updater.Open(dbNum, page.PageNumber, 1, true);
            }
          }
          if (useReaderCommit)
          {
            reader.Commit();
            reader.BeginRead();
          }
          else
            reader.ForceDatabaseCacheValidation();
          updater.Commit();
          updater.BeginUpdate();
        }
        Database db2 = reader.OpenDatabase(dbNum);
        db = updater.OpenDatabase(dbNum);
        for (int i = 25; i < 50; i++)
        {
          foreach (Page page in db)
          {
            if (page.PageNumber > 1)
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i);
              Man manUpdated = (Man)updater.Open(dbNum, page.PageNumber, 1, true);
            }
          }
          updater.Commit();
          updater.BeginUpdate();
          db2 = reader.OpenDatabase(dbNum);
          foreach (Page page in db2)
          {
            if (page.PageNumber > 1)
            {
              //Assert.True(page.PageInfo.VersionNumber == (ulong)i + 1);
              Man man2 = (Man)reader.Open(dbNum, page.PageNumber, 1, false);
            }
          }
          reader.ClearPageCache();
          System.GC.Collect();
        }
        reader.Commit();
        updater.Commit();
      }
    }

    [Test]
    public void SingleServerReaderSingleServerUpdater1()
    {
      ServerClientSession updater = new ServerClientSession(systemDir);
      ServerClientSession reader = new ServerClientSession(systemDir);
      const UInt32 dbNum = 345;
      try
      {
        updater.BeginUpdate();
        Man man;
        Placement place = new Placement(dbNum, 1, 1, 2);
        for (int i = 0; i < 100; i++)
        {
          man = new Man();
          man.Persist(place, updater);
        }
        updater.Commit();
        reader.BeginRead();
        Database db = reader.OpenDatabase(dbNum);
        foreach (Page page in db)
          Assert.True(page.PageInfo.VersionNumber == 1);
        reader.Commit();
        updater.BeginUpdate();
        reader.BeginRead();
        for (int i = 1; i < 25; i++)
        {
          db = reader.OpenDatabase(dbNum);
          foreach (Page page in db)
          {
            if (page.PageNumber > 0)
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i);
              Man man2 = (Man)reader.Open(dbNum, page.PageNumber, 1, false);
              Man manUpdated = (Man)updater.Open(dbNum, page.PageNumber, 1, true);
            }
          }
          reader.Commit();
          reader.BeginRead();
          updater.Commit();
          updater.BeginUpdate();
          reader.ForceDatabaseCacheValidation(); // we now validate on BeginRead so to make this test pass, we need to add this call after updater commit.
        }
        Database db2 = reader.OpenDatabase(dbNum);
        db = updater.OpenDatabase(dbNum);
        for (int i = 25; i < 50; i++)
        {
          foreach (Page page in db)
          {
            if (page.PageNumber > 0)
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i);
              Man manUpdated = (Man)updater.Open(dbNum, page.PageNumber, 1, true);
            }
          }
          updater.Commit();
          updater.BeginUpdate();
          foreach (Page page in db2)
          {
            if (page.PageNumber > 0)
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i + 1);
              Man man2 = (Man)reader.Open(dbNum, page.PageNumber, 1, false);
            }
          }
          reader.ClearPageCache(); // required or else we will use cached page and Assert (see line above) will fail
          System.GC.Collect(); // force weak referenced pages to be garbage collected (again to avoid Assert failure)
        }
        reader.Commit();
        updater.DeleteDatabase(db);
        updater.Commit();
      }
      finally
      {
        updater.Dispose();
        reader.Dispose();
      }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SingleServerReaderSingleServerUpdater2(bool useReaderCommit)
    {
      const UInt32 dbNum = 567;
      using (ServerClientSession updater = new ServerClientSession(systemDir))
      using (ServerClientSession reader = new ServerClientSession(systemDir, null, 2000, true, false, false, CacheEnum.No)) // CacheEnum.No or cache validating on session.Begin() - makes test fail
      {
        updater.BeginUpdate();
        Database db = updater.OpenDatabase(dbNum, true, false);
        if (db != null)
          updater.DeleteDatabase(db);
        updater.Commit();
        updater.BeginUpdate();
        Man man;
        Placement place = new Placement(dbNum, 1, 1, 2);
        for (int i = 0; i < 100; i++)
        {
          man = new Man();
          man.Persist(place, updater);
        }
        updater.Commit();
        reader.BeginRead();
        db = reader.OpenDatabase(dbNum);
        foreach (Page page in db)
          Assert.True(page.PageInfo.VersionNumber == 1);
        if (useReaderCommit)
          reader.Commit();
        updater.BeginUpdate();
        if (useReaderCommit)
          reader.BeginRead();
        else
          reader.ForceDatabaseCacheValidation();
        for (int i = 1; i < 25; i++)
        {
          db = reader.OpenDatabase(dbNum);
          foreach (Page page in db)
          {
            if (page.PageNumber > 0)
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i);
              Man man2 = (Man)reader.Open(dbNum, page.PageNumber, 1, false);
              Man manUpdated = (Man)updater.Open(dbNum, page.PageNumber, 1, true);
            }
          }
          updater.Commit(); // must commit updater first, we validate and update reader on BeginRead or ForceDatabaseCacheValidation
          if (useReaderCommit)
          {
            reader.Commit();
            reader.BeginRead();
          }
          else
            reader.ForceDatabaseCacheValidation();
          updater.BeginUpdate();
        }
        Database db2 = reader.OpenDatabase(dbNum);
        db = updater.OpenDatabase(dbNum);
        for (int i = 25; i < 50; i++)
        {
          foreach (Page page in db)
          {
            if (page.PageNumber > 0)
            {
              Assert.True(page.PageInfo.VersionNumber == (ulong)i);
              Man manUpdated = (Man)updater.Open(dbNum, page.PageNumber, 1, true);
            }
          }
          updater.FlushUpdates(); // now server will see updated version of pages
          foreach (Page page in db2)
          {
            if (page.PageNumber > 0)
            {                           // BUG Nov 8, 2011 1.0.4.0 reader sees version 28 when it should see version 27 
              Assert.True(page.PageInfo.VersionNumber == (ulong)i); // reader should see the commited version of the page, not the uncommited updated version
              Man man2 = (Man)reader.Open(dbNum, page.PageNumber, 1, false);
            }
          }
          reader.ClearPageCache(); // required or else we will use cached page and Assert (see line above) will fail
          System.GC.Collect(); // force weak referenced pages to be garbage collected (again to avoid Assert failure)
          updater.Commit();
          updater.BeginUpdate();
        }
        reader.Commit();
        updater.DeleteDatabase(db);
        updater.Commit();
      }
    }
  }
}
