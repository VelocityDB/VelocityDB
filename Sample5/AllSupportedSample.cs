using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.AllSupportedSample;

namespace AllSupportedSample
{
  class AllSupportedSample
  {
    static readonly string s_systemDir = "AllSupportedSample"; // appended to SessionBase.BaseDatabasePath

    static int Main(string[] args)
    {
      UInt64 id;
      AllSupported allSupported, allSupported2;
      AllSuportedSub4 sub4;
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          session.BeginUpdate();
          sub4 = new AllSuportedSub4();
          session.Persist(sub4);
          id = sub4.Id;
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginRead();
          sub4 = (AllSuportedSub4)session.Open(id);
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginUpdate();
          allSupported = new AllSupported(3, session);
          session.Persist(allSupported);
          id = allSupported.Id;
         session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginRead();
          allSupported2 = (AllSupported)session.Open(id);
          session.Commit();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return 1;
      }
      return 0;
    }
  }
}
