using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class LargePage
  {
    static readonly string s_windrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = System.IO.Path.Combine(s_windrive, "NUnitTestDbs");

    [Test]
    public void LargeObject()
    {
      // max length of an array is int.MaxValue, objects on a page are serialized to a single byte[] so this array must have a length < int.MaxValue
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate(); 
        var large = new LargeObject((int) Math.Pow(2, 27));
        id = session.Persist(large);
        Assert.True(large.IsOK());
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        var large = session.Open<LargeObject>(id);
        Assert.True(large.IsOK());
        session.Commit();
      }
    }

    [Test]
    public void TooLargeObject()
    {
      Assert.Throws<OverflowException>(() =>
      {
        UInt64 id;
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginUpdate();
          var large = new LargeObject((int)Math.Pow(2, 28));
          id = session.Persist(large);
          Assert.True(large.IsOK());
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginRead();
          var large = session.Open<LargeObject>(id);
          Assert.True(large.IsOK());
          session.Commit();
        }
      });
    }
  }
}
