using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDbSchema;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class HashSetTest
  {
    static readonly string s_windrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = System.IO.Path.Combine(s_windrive, "NUnitTestDbs");

    [Test]
    public void HashSetAddNonPersisted()
    {
      UInt64 id;
      Person person = null;
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        var hashSet = new HashSet<Person>();
        for (int i = 0; i < 100; i++)
        {
          var p = new Person();
          session.Persist(p);
          hashSet.Add(p);
          if (i == 47)
          {
            person = p;
            Assert.IsFalse(hashSet.Add(person));
          }
        }
        id = session.Persist(hashSet);
        Assert.IsTrue(hashSet.Contains(person));
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        var hashSet = session.Open<HashSet<Person>>(id);
        Assert.AreEqual(100, hashSet.Count);
        int ct = 0;
        foreach (Person p in hashSet)
          ct++;
        Assert.IsTrue(hashSet.Contains(person));
        Assert.IsFalse(hashSet.Add(person));
        Assert.AreEqual(100, ct);
        session.Commit();
      }
    }
  }
}