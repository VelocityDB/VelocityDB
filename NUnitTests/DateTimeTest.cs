using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using NUnit.Framework;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class DateTimeTest
  {
    public const string systemDir = "c:/NUnitTestDbs";

    [Test]
    public void LocalDateTest()
    {
      LocalDate d1 = new LocalDate(2016, 1, 10);
      LocalDate d2 = new LocalDate(2016, 1, 1);
      LocalDate d1other = new LocalDate(2016, 1, 10);
      Assert.AreNotEqual(d1, d2);
      Assert.AreEqual(d1, d1other);

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        LocalDateField test1 = new LocalDateField("def", d1);
        session.Persist(test1);
        LocalDateField test = new LocalDateField("abc", d2);
        session.Persist(test);
        var result1 = session.AllObjects<LocalDateField>().First(t => t.Field2.Equals(d2)); // this works
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var result2 = session.AllObjects<LocalDateField>().First(t => 
        {
          var l = t.Field2;
          return l.Equals(d2);
        }); // this should work and doesnt
        session.Commit();
      }
    }
  }
}
