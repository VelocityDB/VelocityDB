using System;
using System.Collections.Generic;
using System.Linq;
using VelocityDb.Session;
using VelocityDBExtensions;
using VelocityDbSchema.Samples.Sample4;

namespace JsonExportImport
{
  class JsonExportImport
  {
    static readonly string s_systemDirToImport = "Sample4"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_systemDir = "JsonExportImport"; // appended to SessionBase.BaseDatabasePath

    static void Main(string[] args)
    {
      try
      {
        int personCt = 0;
        using (SessionBase session = new SessionNoServer(s_systemDirToImport))
        {
          session.BeginRead();
          IEnumerable<string> personStringEnum = session.ExportToJson<Person>();
          using (SessionBase sessionImport = new SessionNoServer(s_systemDir))
          {
            sessionImport.BeginUpdate();
            foreach (string json in personStringEnum)
            {
              Person person = sessionImport.ImportJson<Person>(json);
              sessionImport.Persist(person);
              personCt++;
            }
            session.Commit();
            sessionImport.Commit();
            Console.WriteLine("Imported " + personCt + " from Json strings");
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}
