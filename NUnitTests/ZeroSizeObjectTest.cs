using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class ZeroSizeObjectTest
  {
    public const string systemDir = "c:/NUnitTestDbs";
    [Test]
    public void zeroSizeByInterface()
    {
      UInt64 id = 0;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (int i = 0; i < 100000; i++)
        {
          var o = new EFTPOSMachineParent();
          session.Persist(o);
          if (i % 25000 == 0)
            id = o.Id;
        }
        var o2 = (EFTPOSMachineParent)session.Open(id);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var o2 = (EFTPOSMachineParent)session.Open(id);
        Assert.NotNull(o2);
        Assert.NotNull(o2.TransactionFacilitator);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var o2 = (EFTPOSMachineParent)session.Open(id);
        Assert.NotNull(o2);
        Assert.NotNull(o2.TransactionFacilitator);
        o2.Unpersist(session);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var o2 = (EFTPOSMachineParent)session.Open(id);
        Assert.Null(o2);
        id++;
        o2 = (EFTPOSMachineParent)session.Open(id);
        Assert.NotNull(o2);
        Assert.NotNull(o2.TransactionFacilitator);
        session.Commit();
      }
    }
  }
}
