using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using VelocityDbSchema.NUnit;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class MultipleSessions
  {
    public const string systemDir = "c:\\NUnitTestDbs";
    public const string location2Dir = "c:\\NUnitTestDbsLocation2";
    public string systemHost = Dns.GetHostName();

    [Test]
    public void PagesWritten()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {      
        Placement place = new Placement(789);
        session.BeginUpdate();
        session.RegisterClass(typeof(Person));
        session.RegisterClass(typeof(VelocityDbList<WeakIOptimizedPersistableReference<Person>>));
        session.RegisterClass(typeof(VelocityDbSchema.Samples.AllSupportedSample.Pet));
        session.RegisterClass(typeof(System.Guid));
        session.RegisterClass(typeof(AutoPlacement));
        session.Commit();
        session.BeginUpdate();
        Person person = new Person();
        person.friends.Persist(place, session);
        person.Persist(place, session);
        session.FlushUpdates();
        person = new Person();
        person.Persist(place, session);
        using (ServerClientSession session2 = new ServerClientSession(systemDir, systemHost))
        {
          Placement place2 = new Placement(7891);
          session2.BeginUpdate();
          Person person2 = new Person();
          person2.friends.Persist(place2, session2);
          person2.Persist(place2, session2);
          session2.FlushUpdates();
          person2 = new Person();
          person2.Persist(place2, session2);
          session2.Commit();
        }
        using (ServerClientSession session2 = new ServerClientSession(systemDir, systemHost))
        {
          Placement place2 = new Placement(7892);
          session2.BeginUpdate();
          Person person2 = new Person();
          person2.friends.Persist(place2, session2);
          person2.Persist(place2, session2);
          session2.FlushUpdates();
          person2 = new Person();
          person2.friends.Persist(place2, session2);
          person2.Persist(place2, session2);
          session2.Commit();
        }
        using (ServerClientSession session2 = new ServerClientSession(systemDir, systemHost))
        {
          Placement place2 = new Placement(7893);
          session2.BeginUpdate();
          Person person2 = new Person();
          person2.friends.Persist(place2, session2);
          person2.Persist(place2, session2);
          session2.FlushUpdates();
          person2 = new Person();
          person2.Persist(place2, session2);
          using (ServerClientSession session3 = new ServerClientSession(systemDir, systemHost))
          {
            Placement place3 = new Placement(7894);
            session3.BeginUpdate();
            Person person3 = new Person();
            person3.friends.Persist(place3, session3);
            person3.Persist(place3, session3);
            session3.FlushUpdates();
            person3 = new Person();
            person3.friends.Persist(place3, session3);
            person3.Persist(place3, session3);
            session.Commit();
            session2.Commit();
            session3.Commit();
          }
        }
      }
    }

    [Test]
    public void ServerPageFlush()
    {     
      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {
        //DatabaseLocation location = new DatabaseLocation(systemHost, location2Dir, 700, UInt32.MaxValue, session, false, PageInfo.encryptionKind.noEncryption);
        Placement place = new Placement(789, 1, 1, 1);
        //session.SetTraceAllDbActivity();
        session.BeginUpdate();
        session.RegisterClass(typeof(ObjWithArray)); // avoid lock failure by registrty ahead of running parallell sessions.
        session.RegisterClass(typeof(Person)); // avoid lock failure by registrty ahead of running parallell sessions.
        session.RegisterClass(typeof(VelocityDbSchema.Samples.AllSupportedSample.Pet)); // avoid lock failure by registrty ahead of running parallell sessions.
        session.RegisterClass(typeof(VelocityDbList<WeakIOptimizedPersistableReference<Person>>)); // avoid lock failure by registrty ahead of running parallell sessions.
       // session.NewLocation(location);
        session.Commit();
        session.BeginUpdate();
        for (int i = 0; i < 4000; i++)
        {
          ObjWithArray person = new ObjWithArray(i * 10);
          person.Persist(place, session);
        }
        session.FlushUpdates();
        for (int i = 0; i < 1000; i++)
        {
          ObjWithArray person = new ObjWithArray(i);
          person.Persist(place, session);
        }
        using (ServerClientSession session2 = new ServerClientSession(systemDir, systemHost))
        {
          Placement place2 = new Placement(7891, 1, 1, 1);
          session2.BeginUpdate();
          for (int i = 0; i < 1000; i++)
          {
            ObjWithArray person = new ObjWithArray(i);
            person.Persist(place2, session2);
          }
          session2.FlushUpdates();
          for (int i = 0; i < 1000; i++)
          {
            ObjWithArray person = new ObjWithArray(i);
            person.Persist(place2, session2);
          }
          session2.Commit();
        }
        using (ServerClientSession session2 = new ServerClientSession(systemDir, systemHost))
        {
          Placement place2 = new Placement(7894, 1, 1, 1);
          session2.BeginUpdate();
          for (int i = 0; i < 5000; i++)
          {
            ObjWithArray person = new ObjWithArray(i);
            person.Persist(place2, session2);
          }
          session2.FlushUpdates();
          for (int i = 0; i < 1000; i++)
          {
            ObjWithArray person = new ObjWithArray(i);
            person.Persist(place2, session2);
          }
          session2.Commit();
        }
        using (ServerClientSession session2 = new ServerClientSession(systemDir, systemHost))
        {
          Placement place2 = new Placement(7897, 1, 1, 1);
          session2.BeginUpdate();
          for (int i = 0; i < 5000; i++)
          {
            ObjWithArray person = new ObjWithArray(i);
            person.Persist(place2, session2);
          }
          session2.FlushUpdates();
          for (int i = 0; i < 1000; i++)
          {
            ObjWithArray person = new ObjWithArray(i);
            person.Persist(place2, session2);
          }
          using (ServerClientSession session3 = new ServerClientSession(systemDir, systemHost))
          {
            Placement place3 = new Placement(7900, 1, 1, 1);
            session3.BeginUpdate();
            for (int i = 0; i < 1000; i++)
            {
              ObjWithArray person = new ObjWithArray(i);
              person.Persist(place3, session3);
            }
            session3.FlushUpdates();
            for (int i = 0; i < 1000; i++)
            {
              ObjWithArray person = new ObjWithArray(i);
              person.Persist(place3, session3);
            }
            session.Commit();
            session2.Commit();
            session3.Commit();
          }
        }
      }
    }
  }
}
