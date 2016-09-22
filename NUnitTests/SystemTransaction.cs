using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using VelocityDb.Session;

namespace NUnitTests
{
  [TestFixture]
  public class SystemTransaction
  {
    static readonly string s_windrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = System.IO.Path.Combine(s_windrive, "NUnitTestDbs");

    [Test]
    public void GermanString()
    {
      UInt64 id = 0;
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        using (var trans = new TransactionScope())
        {
          session.BeginUpdate();
          VelocityDbSchema.Person person = new VelocityDbSchema.Person();
          person.LastName = "Med vänliga hälsningar";
          id = session.Persist(person);
          trans.Complete();
        }
      }
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        using (var trans = new TransactionScope())
        {
          session.BeginUpdate();
          VelocityDbSchema.Person person = session.Open<VelocityDbSchema.Person>(id);
          person.LastName = "Mit freundlichen Grüßen";
          trans.Complete();
        }
      }
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        using (var trans = new TransactionScope())
        {
          session.BeginUpdate();
          VelocityDbSchema.Person person = session.Open<VelocityDbSchema.Person>(id);
          person.LastName = "Med vänliga hälsningar";
          trans.Complete();
        }
      }
    }
  }
}
