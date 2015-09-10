// This application shows how to accomplish High Availability with VelocityDB. 
// It can be done with in memory data or persisted data replicated to a backup server that can take over as a master server on demand.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema;

namespace BackupRestore
{
  class HighAvailability
  {
    static readonly string s_systemDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      "VelocityDB" + Path.DirectorySeparatorChar + "Databases" + Path.DirectorySeparatorChar + "HighAvailability");
    static readonly string s_licenseDbFile = "c:/4.odb";
    static string systemHost = Dns.GetHostName();
    //static string backupHost = "Qlap-Tech0195"; // modify to second server name that you are using (make sure VelocityDB is installed on that server first)
    static string backupHost = "FindPriceBuy"; // modify to second server name that you are using (make sure VelocityDB is installed on that server first)
    static readonly string backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VelocityDB", "Databases" + Path.DirectorySeparatorChar + "HighAvailabilityBackup");
    static bool inMemoryOnly = false;
    public static readonly uint backupLocationStartDbNum = (uint)Math.Pow(2, 26);
    public void CreateDataWithBackupServer()
    {
      int loops = 30000;
      int j;
      using (ServerClientSession session = new ServerClientSession(s_systemDir, systemHost, 1000, true, inMemoryOnly))
      {
        Man aMan = null;
        Woman aWoman = null;
        const bool isBackupLocation = true;
        session.BeginUpdate();
        // we need to have backup locations special since server is not supposed to do encryption or compression
        DatabaseLocation backupLocation = new DatabaseLocation(backupHost, backupDir, backupLocationStartDbNum, UInt32.MaxValue, session,
          PageInfo.compressionKind.LZ4, PageInfo.encryptionKind.noEncryption, isBackupLocation, session.DatabaseLocations.Default());
        session.NewLocation(backupLocation);
        session.Commit();
        session.BeginUpdate();
        for (j = 1; j <= loops; j++)
        {
          aMan = new Man(null, aMan, DateTime.Now);
          session.Persist(aMan);
          aWoman = new Woman(aMan, aWoman);
          session.Persist(aWoman);
          aMan.spouse = new VelocityDb.WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000000 == 0)
            Console.WriteLine("Loop # " + j);
        }
        UInt64 id = aWoman.Id;
        Console.WriteLine("Commit, done Loop # " + j);
        session.Commit();
      }
    }

    public void CreateMoreDataWithBackupServer()
    {
      int loops = 1000;
      int j;
      using (ServerClientSession session = new ServerClientSession(s_systemDir, systemHost, 1000, true, inMemoryOnly))
      {
        Man aMan = null;
        Woman aWoman = null;
        session.BeginUpdate();
        for (j = 1; j <= loops; j++)
        {
          aMan = new Man(null, aMan, DateTime.Now);
          session.Persist(aMan);
          aWoman = new Woman(aMan, aWoman);
          session.Persist(aWoman);
          aMan.spouse = new VelocityDb.WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000000 == 0)
            Console.WriteLine("Loop # " + j);
        }
        UInt64 id = aWoman.Id;
        Console.WriteLine("Commit, done Loop # " + j);
        session.FlushUpdates();
        ReadSomeData(); // read some with uncommited cached server data
        session.Commit();
      }
    }

    public void ReadSomeData()
    {
      int ct = 0;
      using (ServerClientSession session = new ServerClientSession(s_systemDir, systemHost, 1000, true, inMemoryOnly))
      {
        session.BeginRead();
        foreach (Man man in session.AllObjects<Man>())
        {
          ct++;
        }
        Console.WriteLine("Commit, number of Men found: " + ct);
        session.Commit();
      }
    }

    public void RestoreToBackupServer()
    {
      using (ServerClientSession session = new ServerClientSession(s_systemDir, systemHost))
      {
        session.ClearServerCache(); // normally don't use this function but use it here to simulate a server going down and restarting
      }

      using (ServerClientSession session = new ServerClientSession(s_systemDir, backupHost, 1000, true, inMemoryOnly))
      {
        session.BeginUpdate();
        DatabaseLocation backupLocation = new DatabaseLocation(backupHost, backupDir, backupLocationStartDbNum, UInt32.MaxValue, session,
          PageInfo.compressionKind.LZ4, PageInfo.encryptionKind.noEncryption, true, session.DatabaseLocations.Default());
        session.RestoreFrom(backupLocation, DateTime.MaxValue);
        session.Commit(false, true);
      }
    }

    public void DeleteBackupLocation()
    {
      using (ServerClientSession session = new ServerClientSession(s_systemDir, systemHost, 1000, true, inMemoryOnly))
      {
        session.BeginUpdate();
        DatabaseLocation backupLocation = session.DatabaseLocations.LocationForDb(backupLocationStartDbNum);
        List<Database> dbList = session.OpenLocationDatabases(backupLocation, true);
        foreach (Database db in dbList)
          session.DeleteDatabase(db);
        session.DeleteLocation(backupLocation);
        session.Commit();
      }
    }
    public void DeleteDefaultLocation()
    {
      using (ServerClientSession session = new ServerClientSession(s_systemDir, systemHost, 1000, true, inMemoryOnly))
      {
        session.BeginUpdate();
        DatabaseLocation defaultLocation = session.DatabaseLocations.Default();
        List<Database> dbList = session.OpenLocationDatabases(defaultLocation, true);
        foreach (Database db in dbList)
          if (db.DatabaseNumber > Database.InitialReservedDatabaseNumbers)
            session.DeleteDatabase(db);
        session.DeleteLocation(defaultLocation);
        session.Commit();
      }
    }

    static void Main(string[] args)
    {
      if (args.Length > 0) // pass any argument to to command line to force use of persisted data.
        inMemoryOnly = false;
      HighAvailability ha = new HighAvailability();
      try
      {
        if (!Directory.Exists(s_systemDir))
          Directory.CreateDirectory(s_systemDir);
        File.Copy(s_licenseDbFile, Path.Combine(s_systemDir, "4.odb"));
        ha.CreateDataWithBackupServer();
        ha.ReadSomeData();
        ha.CreateMoreDataWithBackupServer();
        ha.ReadSomeData();
        if (Directory.Exists(s_systemDir))
          Directory.Delete(s_systemDir, true); // remove our current systemDir and all its databases.
        ha.RestoreToBackupServer();
        string t = systemHost; // swap backupHost and systemHost
        systemHost = backupHost;
        backupHost = t;
        ha.ReadSomeData();
        ha.CreateMoreDataWithBackupServer();
        ha.ReadSomeData();
        ha.DeleteBackupLocation();
        ha.DeleteDefaultLocation(); // so that we can rerun this sample
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception: {0}", ex);

      }
    }
  }
}
