using System;
using System.Diagnostics;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;
using VelocityDbSchema.Samples.Sample1;

namespace Sample1
{
  class Sample1
  {
    static readonly string systemDir = "Sample1Core"; // appended to SessionBase.BaseDatabasePath
    static int Main(string[] args)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Console.WriteLine($"Running with databases in directory: {session.SystemDirectory}");
        session.BeginUpdate();
        var person = new Person("Robin", "Hood", 30);
        session.Persist(person);
        person = new Person("Bill", "Gates", 56);
        session.Persist(person);
        person = new Person("Steve", "Jobs", 56);
        session.Persist(person);
        session.Commit();
      }
      return 0;
    }
  }
}
