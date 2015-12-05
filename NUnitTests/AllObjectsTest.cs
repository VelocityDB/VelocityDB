using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        for (int i = 0; i < 5; i++)
        {
          var classA = new BaseClassA();
          session.Persist(classA);
        }
        for (int i = 0; i < 5; i++)
        {
          var classB = new ClassB();
          session.Persist(classB);
        }

        for (int i = 0; i < 5; i++)
        {
          var classC = new ClassC();
          session.Persist(classC);
        }
        session.Commit();
      }

      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Assert.AreEqual(5, session.AllObjects<BaseClassA>(false).Count);
        Assert.AreEqual(15, session.AllObjects<BaseClassA>().Count);
        Assert.AreEqual(5, session.AllObjects<ClassB>(false).Count);
        Assert.AreEqual(5, session.AllObjects<ClassB>().Count);
        Assert.AreEqual(5, session.AllObjects<ClassC>(false).Count);
        Assert.AreEqual(5, session.AllObjects<ClassC>().Count);
        int ct = 0;
        foreach (var o in session.AllObjects<BaseClassA>(false))
          ++ct;
        Assert.AreEqual(5, ct);
        ct = 0;
        foreach (var o in session.AllObjects<BaseClassA>())
          ++ct;
        Assert.AreEqual(15, ct);
        ct = 0;
        foreach (var o in session.AllObjects<ClassB>())
          ++ct;
        Assert.AreEqual(5, ct);
        ct = 0;
        foreach (var o in session.AllObjects<ClassC>())
          ++ct;
        Assert.AreEqual(5, ct);
        session.Commit();
      }

      using (SessionBase session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        foreach (var o in session.AllObjects<BaseClassA>())
          o.Unpersist(session);
        Assert.AreEqual(0, session.AllObjects<BaseClassA>().Count);
        session.Commit();
      }
    }

  }
}
