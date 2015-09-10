using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.TypeInfo;
using VelocityDb.Session;
using VelocityDbSchema.Samples.UpdateClass;
using System.IO;
using System.Diagnostics;

namespace UpdateClass
{
  /// <summary>
  /// Test updating a class that already exist in the Database schema, rerun multiple times. Modify class in between runs. Check objects by browsing them with VelocityDbBrowser
  /// </summary>
  public class UpdateClass
  {
    static readonly string s_systemDir = "UpdateClass"; // appended to SessionBase.BaseDatabasePath

    static int Main(string[] args)
    {
      try
      {
        UpdatedClass updatedClassObject;
        int ct1 = 0;
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginUpdate();
          session.UpdateClass(typeof(UpdatedClass)); // call this when you have updated the class since first storing instances of this type or since last call to UpdateClass
          UInt32 dbNum = session.DatabaseNumberOf(typeof(UpdatedClass));
          foreach (UpdatedClass obj in session.AllObjects<UpdatedClass>())
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
            foreach (UpdatedClass obj in db.AllObjects<UpdatedClass>())
              ct2++;
          Debug.Assert(ct1 == ct2);         
          updatedClassObject = new UpdatedClass();
          session.Persist(updatedClassObject);
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
