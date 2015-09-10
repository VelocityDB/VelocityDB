using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample2;

namespace Sample2
{
  class Program
  {
    static readonly string systemDir =
System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
"VelocityDB" + Path.DirectorySeparatorChar + "Databases" + Path.DirectorySeparatorChar + "Sample2"); 

    static void Main(string[] args)
    {
      SessionNoServer session = new SessionNoServer(systemDir);
      session.BeginUpdate();
      Person robinHood = new Person("Robin", "Hood", 30);
      Person billGates = new Person("Bill", "Gates", 56, robinHood);
      Person steveJobs = new Person("Steve", "Jobs", 56, billGates);
      robinHood.BestFriend = billGates;
      session.Persist(steveJobs); // the other persons will be persisted implicetly by reachability from "Steve Jobs" person object
      session.Commit();
    }
  }
}
