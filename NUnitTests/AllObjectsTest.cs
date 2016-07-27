using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  class AllObjectsTest
  {
    static readonly string drive = "c:\\";
    static readonly string licenseDbFile = Path.Combine(drive, "4.odb");
    public const string systemDir = "c:/NUnitTestDbs";

    [Test]
    public void Create()
    {
      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        WeakReferenceList<BaseClassA> baseClassAList = new WeakReferenceList<BaseClassA>();
        session.Persist(baseClassAList);
        for (int i = 0; i < 5; i++)
        {
          var classA = new BaseClassA();
          session.Persist(classA);
          baseClassAList.Add(classA);
        }        
        WeakReferenceList<ClassB> classBList = new WeakReferenceList<ClassB>();
        session.Persist(classBList);
        for (int i = 0; i < 5; i++)
        {
          var classB = new ClassB();
          classBList.Add(classB);
          baseClassAList.Add(classB);
        }
        var aList = (from aClass in baseClassAList orderby aClass.RandomOrder select aClass).ToList();
        WeakReferenceList < ClassC > classCList = new WeakReferenceList<ClassC>();
        session.Persist(classCList);
        for (int i = 0; i < 5; i++)
        {
          var classC = new ClassC();
          classCList.Add(classC);
        }
        ClassD d = new ClassD();
        session.Persist(d);

        for (int i = 0; i < 5; i++)
        {
          var classFromB = new ClassFromB();
          session.Persist(classFromB);
        }
        session.Commit();
      }

      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Assert.AreEqual(5, session.AllObjects<BaseClassA>(false).Count);
        int ct = session.AllObjects<BaseClassA>(true).Skip(5).Count();
        Assert.Less(5, ct);
        object obj = session.AllObjects<BaseClassA>(true).Skip(5).First();
        Assert.NotNull(obj);
        obj = session.AllObjects<BaseClassA>(true).Skip(6).First();
        Assert.NotNull(obj);
        Assert.AreEqual(20, session.AllObjects<BaseClassA>().Count);
        Assert.AreEqual(5, session.AllObjects<ClassB>(false).Count);
        Assert.AreEqual(10, session.AllObjects<ClassB>().Count);
        obj = session.AllObjects<ClassB>(true).Skip(5).First();
        Assert.NotNull(obj);
        obj = session.AllObjects<ClassB>(true).Skip(4).First();
        Assert.NotNull(obj);
        obj = session.AllObjects<ClassB>(true).Skip(6).First();
        Assert.NotNull(obj);
        Assert.AreEqual(5, session.AllObjects<ClassC>(false).Count);
        Assert.AreEqual(5, session.AllObjects<ClassC>().Count);
        ct = 0;
        foreach (var o in session.AllObjects<BaseClassA>(false))
          ++ct;
        Assert.AreEqual(5, ct);
        ct = 0;
        foreach (var o in session.AllObjects<BaseClassA>())
          ++ct;
        Assert.AreEqual(20, ct);
        ct = 0;
        foreach (var o in session.AllObjects<ClassB>())
          ++ct;
        Assert.AreEqual(10, ct);
        ct = 0;
        foreach (var o in session.AllObjects<ClassC>())
          ++ct;
        Assert.AreEqual(5, ct);
        ct = 0;
        foreach (var o in session.AllObjects<IOptimizedPersistable>())
          ++ct;
        int ct2 = 0;
        foreach (var o in session.AllObjects<OptimizedPersistable>())
          ++ct2;
        int ct3 = 0;
        foreach (var o in session.AllObjects<IHasClassName>())
          ++ct3;
        session.Commit();
      }

      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        foreach (var o in session.AllObjects<WeakReferenceList<BaseClassA>>())
          o.Unpersist(session);
        foreach (var o in session.AllObjects<WeakReferenceList<ClassB>>())
          o.Unpersist(session);
        foreach (var o in session.AllObjects<WeakReferenceList<ClassC>>())
          o.ClearAndUnpersistContainedObjects(session);
        foreach (var o in session.AllObjects<IHasClassName>())
          session.Unpersist(o);
        Assert.AreEqual(0, session.AllObjects<WeakReferenceList<BaseClassA>>().Count);
        Assert.AreEqual(0, session.AllObjects<BaseClassA>().Count);
        Assert.AreEqual(0, session.AllObjects<IHasClassName>().Count);
        session.Commit();
      }
    }

  }
}
