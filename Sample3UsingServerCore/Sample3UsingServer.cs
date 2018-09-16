using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample3;

namespace Sample3UsingServer
{
  class Sample3UsingServer
  {
    /// <summary>
    /// <see cref="SessionBase.BaseDatabasePath"/> is prefixed to this relative path to make it a full path.
    /// </summary>
    static readonly string systemDir = "Sample3Core";

    static void Main(string[] args)
    {
      SessionBase.s_serverTcpIpPortNumber = 7032;
      SessionBase.DoWindowsAuthentication = false; // Make sure to use the same setting when starting VelocityDBServer, see http://www.velocitydb.com/UserGuide.aspx
      try
      {                                                           // initial DatabaseLocation directory and hostname
        using (ServerClientSession session = new ServerClientSession(systemDir, System.Net.Dns.GetHostName()))
        {
          session.BeginUpdate();
          // your code here
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
