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
  public class ComplexClass
  {
    public const string systemDir = "c:\\NUnitTestDbs";
    [Test]
    public void AllSupported()
    {
      UInt64 id;
      AllSupported allSuported, allSupported2;
      AllSuportedSub1 allSuportedSub1, allSuportedSub2;
      AllSuportedSub2 allSuportedSub2_1, allSuportedSub2_2;
      AllSuportedSub3 allSuportedSub3_1, allSuportedSub3_2;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        allSuportedSub1 = new AllSuportedSub1(3);
        allSuportedSub1.Persist(session, allSuportedSub1);
        id = allSuportedSub1.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        allSuportedSub2 = (AllSuportedSub1)session.Open(id);
        Assert.NotNull(allSuportedSub2);
        session.Commit();
      }     
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        allSuportedSub2_1 = new AllSuportedSub2(3);
        allSuportedSub2_1.Persist(session, allSuportedSub2_1);
        id = allSuportedSub2_1.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        allSuportedSub2_2 = (AllSuportedSub2)session.Open(id);
        Assert.NotNull(allSuportedSub2_2);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        allSuportedSub3_1 = new AllSuportedSub3(3);
        allSuportedSub3_1.Persist(session, allSuportedSub3_1);
        id = allSuportedSub3_1.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        allSuportedSub3_2 = (AllSuportedSub3)session.Open(id);
        Assert.NotNull(allSuportedSub3_2);
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var x = new AllSuportedSub5();
        session.Persist(x);
        id = x.Id;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var x = (AllSuportedSub5)session.Open(id);
        x.Update();
        x.nullableaDouble = 0.5;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var x = (AllSuportedSub5)session.Open(id);
        Assert.NotNull(x);
        Assert.AreEqual(x.nullableaDouble, 0.5);
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        allSuported = new AllSupported(3);
        allSuported.Persist(session, allSuported);
        allSuported.m_weakRefArray[0] = new WeakIOptimizedPersistableReference<IOptimizedPersistable>(allSuported);
        allSuported.m_objectArray[0] = new WeakIOptimizedPersistableReference<IOptimizedPersistable>(allSuported);
        id = allSuported.Id;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        allSupported2 = (AllSupported)session.Open(id);
        allSupported2.Update();
        allSupported2.nullableaDouble = 0.5;
        allSupported2.NullableDateTime = DateTime.MaxValue;
        allSupported2 = null;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        allSupported2 = (AllSupported)session.Open(id);
        Assert.NotNull(allSupported2);
        Assert.AreEqual(allSupported2.nullableaDouble, 0.5);
        Assert.AreEqual(allSupported2.NullableDateTime, DateTime.MaxValue);
        session.Commit();
        session.BeginUpdate();
        allSupported2.NullableDateTime = DateTime.UtcNow;

        session.Commit();
        session.BeginRead();
        allSupported2 = (AllSupported)session.Open(id);
        Assert.AreEqual(DateTimeKind.Utc, allSupported2.NullableDateTime.Value.Kind);
        session.Commit();
      }
    }
  }
}
