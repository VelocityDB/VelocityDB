using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using VelocityDbSchema.NUnit;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class FixedSizeData
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    [Test]
    public void FixedSizeTest()
    {
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        FixedSize fixedSize = new FixedSize();
        fixedSize.Persist(session, fixedSize);
        id = fixedSize.Id;
        session.Commit();
        session.Compact();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        FixedSize fixedSize = (FixedSize)session.Open(id);
        Assert.NotNull(fixedSize);
        session.Commit();
        session.Compact();
      }
    }

    [TestCase(1000)]
    public void FixedSizeManyTest(int howMany)
    {
      using (var session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        FixedSize fixedSize;
        FixedSize fixedSizePrior = new FixedSize();
        for (int i = 0; i < howMany; i++)
        {
          fixedSize = new FixedSize();
          fixedSize.Persist(session, fixedSizePrior);
          fixedSizePrior = fixedSize;
        }
        session.Commit();
      }
      using (var session = new SessionNoServerShared(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(FixedSize.PlaceInDatabase);
        foreach (Page page in db)
          if (page.PageNumber > 0)
          foreach (FixedSize fixedSize in page)
          {
            --howMany;
            Assert.NotNull(fixedSize);
          }
        session.Commit();
      }
    }
  }
}
