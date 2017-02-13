using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Relations;

namespace Relations
{
  class Relation
  {
    static readonly string systemDir = "Relations"; // appended to SessionBase.BaseDatabasePath

    static int Main(string[] args)
    {
      using (var session = new SessionNoServer(systemDir))
      {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Console.WriteLine($"Running with databases in directory: {session.SystemDirectory}");
        try
        {
          session.BeginUpdate();
          var user = new User { Name = "Mats", Surame = "Persson" };
          var user2 = new User { Name = "Robin", Surame = "Persson" };
          session.Persist(user);
          session.Persist(user2);
          user.Backup = user2;
          var customer = new Customer { Name = "Ben", Balance = 0 };
          session.Persist(customer);
          var interaction = new Interaction(customer, user, session);
          session.Commit();
        }
        catch(Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
      using (var session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        try
        {
          foreach (var user in session.AllObjects<User>())
            user.Backup = null; // remove all backup relations (see what happens if you don't !)
          foreach (var user in session.AllObjects<User>())
            user.Unpersist(session);
          foreach (var customer in session.AllObjects<Customer>())
            customer.Unpersist(session);
          foreach (var customer in session.AllObjects<Interaction>())
           throw new UnexpectedException("should not exist any longer");
          session.Commit();
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
      return 0;
    }
  }
}
