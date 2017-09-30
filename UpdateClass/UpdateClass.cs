using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.TypeInfo;
using VelocityDb.Session;
using System.IO;
using System.Diagnostics;

namespace UpdateClass
{
  /// <summary>
  /// Test updating a class that already exist in the Database schema, rerun multiple times. Modify class in between runs. Check objects by browsing them with VelocityDbBrowser
  /// </summary>
  public class UpdatesClass
  {
    static readonly string s_systemDir = "UpdateClass"; // appended to SessionBase.BaseDatabasePath

    static int Main(string[] args)
    {
      try
      {
        Trace.Listeners.Add(new ConsoleTraceListener());
        VelocityDbSchema.Samples.UpdateClass.UpdatedClass updatedClassObject;
        int ct1 = 0;
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.SetTraceDbActivity(Schema.SchemaDB);
          session.BeginUpdate();
          session.UpdateClass(typeof(VelocityDbSchema.Samples.UpdateClass.UpdatedClass)); // call this when you have updated the class since first storing instances of this type or since last call to UpdateClass
          UInt32 dbNum = session.DatabaseNumberOf(typeof(VelocityDbSchema.Samples.UpdateClass.UpdatedClass));
          foreach (var obj in session.AllObjects<VelocityDbSchema.Samples.UpdateClass.UpdatedClass>())
          {
            Console.Write(obj.ToString() + " has members: ");
            foreach (DataMember member in obj.GetDataMembers())
            {
              Console.Write(member.ToString() + " ");
            }
            Console.WriteLine();
            obj.UpdateTypeVersion(); // comment out if you DO NOT want to migrate this object to the latest version of the class
            ct1++;
          }
          int ct2 = 0;
          Database db = session.OpenDatabase(dbNum, true, false);
          if (db != null)
            foreach (var obj in db.AllObjects<VelocityDbSchema.Samples.UpdateClass.UpdatedClass>())
              ct2++;
          Debug.Assert(ct1 == ct2);
          updatedClassObject = new VelocityDbSchema.Samples.UpdateClass.UpdatedClass();
          session.Persist(updatedClassObject);
          session.Commit();
          MoveToDifferentFullClassName();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return 1;
      }
      return 0;
    }

    static int MoveToDifferentFullClassName()
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.SetTraceDbActivity(Schema.SchemaDB);
          session.BeginUpdate();
          session.ReplacePersistedType(typeof(VelocityDbSchema.Samples.UpdateClass.UpdatedClass), typeof(UpdateClass.UpdatedClass));
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.Verify();
          session.BeginRead();
          foreach (var obj in session.AllObjects<UpdateClass.UpdatedClass>())
          {
            Console.Write(obj.ToString() + " has members: ");
            foreach (DataMember member in obj.GetDataMembers())
            {
              Console.Write(member.ToString() + " ");
            }
          }
          foreach (var obj in session.AllObjects<VelocityDbSchema.Samples.UpdateClass.UpdatedClass>())
          {
            throw new Exception("not expected");
          }
          session.Commit();
        }

        // change it back to original
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.SetTraceDbActivity(Schema.SchemaDB);
          session.BeginUpdate();
          session.ReplacePersistedType(typeof(UpdateClass.UpdatedClass).AssemblyQualifiedName, typeof(VelocityDbSchema.Samples.UpdateClass.UpdatedClass));
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.Verify();
          session.BeginRead();
          foreach (var obj in session.AllObjects<VelocityDbSchema.Samples.UpdateClass.UpdatedClass>())
          {
            Console.Write(obj.ToString() + " has members: ");
            foreach (DataMember member in obj.GetDataMembers())
            {
              Console.Write(member.ToString() + " ");
            }
          }
           foreach (var obj in session.AllObjects<UpdateClass.UpdatedClass>())
           {
            throw new Exception("not expected");
          }
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
