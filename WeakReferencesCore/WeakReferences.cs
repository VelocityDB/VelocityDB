using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.WeakIOptimizedPersistableReferences;

namespace WeakIOptimizedPersistableReferences
{
  class Program
  {
    static readonly string s_systemDir = "WeakIOptimizedPersistableReferencesCore"; // appended to SessionBase.BaseDatabasePath

    const long numberOfPersons = 10000000000; // a billion Person objects - let's say it is more than we can fit in memory
    static void Main(string[] args)
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          session.BeginUpdate();
          Person person = new Person();
          for (long i = 0; i < numberOfPersons; i++)
          {
            // Each WeakIOptimizedPersistableReference require a persisted object (non null Oid) so that object reference can be substituted with an Oid.
            session.Persist(person);
            // create new Person and make prior Person his/her friend (see constructor of Person)
            person = new Person(person);
          }
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
