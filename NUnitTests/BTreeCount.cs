using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.Session;

namespace NUnitTests
{
  class BTreeCount
  {
    public class PersistentClass : OptimizedPersistable
    {
      [Index]
      private string _name;

      public PersistentClass(string name)
      {
        _name = name;
      }
    }


    [TestFixture]
    public class CountExceptionTest
    {
      static readonly string testDir = "g:/CountExceptionTest/";
      static readonly string licensePath = "C:/4.odb";

      private SessionBase _session;

      [SetUp]
      public void InitDatabase()
      {
        if (Directory.Exists(testDir))
          Directory.Delete(testDir, true);
        Directory.CreateDirectory(testDir);
        File.Copy(licensePath, testDir + "4.odb");
        _session = new SessionNoServer(testDir);
        _session.BeginUpdate();
      }

      [TearDown]
      public void CloseDatabase()
      {
        _session.Dispose();
      }

      [Test]
      public void TestAllObjects()
      {
        var obj1 = new PersistentClass("OBJ1");
        _session.Persist(obj1);
        var obj2 = new PersistentClass("OBJ2");
        _session.Persist(obj2);
        _session.Commit();
        _session.BeginUpdate();
        var list = _session.AllObjects<PersistentClass>();
        var computedCount = Enumerable.Count(list);
        Assert.AreEqual(2, computedCount);
        Assert.AreEqual(2, list.Count);
      }

      [Test]
      public void TestIndex()
      {
        var obj1 = new PersistentClass("OBJ1");
        _session.Persist(obj1);
        var obj2 = new PersistentClass("OBJ2");
        _session.Persist(obj2);
        _session.Commit();
        _session.BeginUpdate();
        var index = _session.Index<PersistentClass>("_name");
        var computedCount = Enumerable.Count(index);
        Assert.AreEqual(2, computedCount);
        Assert.AreEqual(2, index.Count);
      }
    }
  }
}
