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

namespace NUnitTests
{
  [TestFixture]
  public class MultipleUpdaters
  {
    public const string systemDir = "c:\\NUnitTestDbs";

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
    [ExpectedException(typeof(OptimisticLockingFailed))]
    public void TwoUpdaters1()
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
    }

    [Test]
    [ExpectedException(typeof(OpenDatabaseException))] 
    public void TwoUpdaters2()
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
    }

    [Test]
    [ExpectedException(typeof(OptimisticLockingFailed))]
    public void TwoUpdaters3()
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
    }
  }
}