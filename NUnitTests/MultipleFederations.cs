using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace NUnitTests
{
  [TestFixture]
  public class MultipleFederations
  {
    [Test]
    public void UseManyFederations()
    {
      for (int i = 0; i < 50; i++)
      {
        using (SessionBase session = new ServerClientSession("Federation" + i))
        {
          session.BeginUpdate();
          session.Commit();
        }
      }
    }
  }
}
