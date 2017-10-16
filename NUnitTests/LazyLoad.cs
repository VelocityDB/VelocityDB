using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDbSchema;
using VelocityDbSchema.NUnit;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class LazyLoadTest
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    [Test]
    public void LazyLoadProperty()
    {
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        LazyLoadPropertyClass lazy = null;
        for (uint i = 1; i <= 10000; i++)
          lazy = new LazyLoadPropertyClass(i, lazy);
        session.Persist(lazy);
        id = lazy.Id;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        UInt32 ct = 10000;
        session.BeginRead();
        LazyLoadPropertyClass lazy = (LazyLoadPropertyClass)session.Open(id);
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);
        lazy = lazy.MyRef;
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);
        lazy = lazy.MyRef;
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);        
        lazy = lazy.MyRef;
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);
        session.Commit();
      }
    }
    
    [Test]
    public void LazyLoadDepth()
    {
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        LazyLoadByDepth lazy = null;
        for (uint i = 1; i <= 100; i++)
          lazy = new LazyLoadByDepth(i, lazy);
        session.Persist(lazy);
        id = lazy.Id;
        session.Commit();
      }

      using (var session = new SessionNoServerShared(systemDir))
      {
        UInt32 ct = 100;
        session.BeginRead();
        LazyLoadByDepth lazy = (LazyLoadByDepth)session.Open(id, false, false, 0); // load only the root of the object graph
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);
        lazy = lazy.MyRef;
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);
        lazy = lazy.MyRef;
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);        
        lazy = lazy.MyRef;
        Assert.AreEqual(ct--, lazy.MyCt);
        Assert.IsNull(lazy.MyRefPeek);
        Assert.NotNull(lazy.MyRef);
        Assert.NotNull(lazy.MyRefPeek);
        session.Commit();
      }
    }
  }
}
