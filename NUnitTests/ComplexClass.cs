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
using VelocityDb.TypeInfo;

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
      AllSuportedSub4 allSuportedSub4;

      AllSupported[,] a1 = new AllSupported[10, 5];
      AllSupported[,,] a2 = new AllSupported[8, 4, 3];
      AllSupported[,,,] a3 = new AllSupported[7, 6, 2, 1];
      Dictionary<int, string>[,,] a4 = new Dictionary<int, string>[2, 4, 33];
      string s1str = DataMember.TypeToString(a1.GetType());
      string s2str = DataMember.TypeToString(a2.GetType());
      string s3str = DataMember.TypeToString(a3.GetType());
      string s4str = DataMember.TypeToString(a4.GetType());
      bool typeUpdated;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Type t1 = DataMember.StringToType(s1str, session, out typeUpdated);
        Type t2 = DataMember.StringToType(s2str, session, out typeUpdated);
        Type t3 = DataMember.StringToType(s3str, session, out typeUpdated);
        Type t4 = DataMember.StringToType(s4str, session, out typeUpdated);
        Assert.AreEqual(t1, a1.GetType());
        Assert.AreEqual(t2, a2.GetType());
        Assert.AreEqual(t3, a3.GetType());
        Assert.AreEqual(t4, a4.GetType());
        allSuportedSub4 = new AllSuportedSub4();
        session.Persist(allSuportedSub4);
        id = allSuportedSub4.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        allSuportedSub4 = (AllSuportedSub4)session.Open(id);
        Assert.NotNull(allSuportedSub4);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        allSuportedSub1 = new AllSuportedSub1(3);
        allSuportedSub1.Persist(session, allSuportedSub1);
        foreach (var o in allSuportedSub1.PetListOidShort)
          session.Persist(o, allSuportedSub1);
        id = allSuportedSub1.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        allSuportedSub2 = (AllSuportedSub1)session.Open(id);
        Assert.NotNull(allSuportedSub2);
        Assert.AreEqual(allSuportedSub2.m_type[0], typeof(Pet));
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
      using (var session = new SessionNoServerShared(systemDir))
      {
        session.BeginRead();
        allSuportedSub3_2 = (AllSuportedSub3)session.Open(id);
        Assert.NotNull(allSuportedSub3_2);
        session.Commit();
      }

      using (var session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var x = new AllSuportedSub5();
        session.Persist(x);
        id = x.Id;
        session.Commit();
      }

      using (var session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var x = (AllSuportedSub5)session.Open(id);
        x.Update();
        x.nullableaDouble = 0.5;
        session.Commit();
      }
      using (var session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var x = new AllSuportedSub6();
        session.Persist(x);
        id = x.Id;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var x = (AllSuportedSub6)session.Open(id);
        x.Update();
        session.Commit();
      }

      using (var session = new SessionNoServer(systemDir))
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

      using (var session = new SessionNoServer(systemDir))
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
