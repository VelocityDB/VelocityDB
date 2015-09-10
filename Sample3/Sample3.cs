using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample3;

namespace Sample3
{
  class Sample3
  {
    static readonly string systemDir = "Sample3"; // appended to SessionBase.BaseDatabasePath

    static void Main(string[] args)
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          session.BeginUpdate();
          Person robinHood = new Person("Robin", "Hood", 30);
          Person billGates = new Person("Bill", "Gates", 56, robinHood);
          Person steveJobs = new Person("Steve", "Jobs", 56, billGates);
          robinHood.BestFriend = billGates;
          session.Persist(steveJobs);
          steveJobs.Friends.Add(billGates);
          steveJobs.Friends.Add(robinHood);
          billGates.Friends.Add(billGates);
          robinHood.Friends.Add(steveJobs);
          session.Commit();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}
