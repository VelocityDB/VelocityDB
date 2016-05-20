using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class StructWrapper
  {
    static readonly string s_windrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = System.IO.Path.Combine(s_windrive, "NUnitTestDbs");

    [Test]
    public void ListWrapperTest()
    {
      // max length of an array is int.MaxValue, objects on a page are serialized to a single byte[] so this array must have a length < int.MaxValue
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        var f = new FourPerPage(1);
        id = session.Persist(f);
        Assert.True(f.IsOK());
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        var f = session.Open<FourPerPage>(id);
        Assert.True(f.IsOK());
        session.Commit();
      }
    }
  }
}
