using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using NUnit.Framework;
using System.IO;

namespace NUnitTests
{
  [TestFixture]
  public class BackupRestoreTests
  {
    public const string systemDir = "c:\\NUnitTestDbs";
    public const string backupDir = "c:\\NUnitTestDbsBackup";
    public static readonly uint backupLocationStartDbNum = (uint)Math.Pow(2, 26);

    [TestCase(true)]
    //[TestCase(false)]
    public void CreateDataWithBackupServer(bool useServerSession)
    {
      int loops = 30000;
      UInt16 objectsPerPage = 300;
      UInt16 pagesPerDatabase = 65000;
      int j;
      if (Directory.Exists(backupDir))
      {
        foreach (string s in Directory.GetFiles(backupDir))
          File.Delete(s);
        foreach (string s in Directory.GetDirectories(backupDir))
          Directory.Delete(s, true);
      }
      else
        Directory.CreateDirectory(systemDir);
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        Placement place = new Placement(11, 1, 1, objectsPerPage, pagesPerDatabase);
        Man aMan = null;
        Woman aWoman = null;
        const bool isBackupLocation = true;
        session.BeginUpdate();
        // we need to have backup locations special since server is not supposed to do encryption or compression
        DatabaseLocation backupLocation = new DatabaseLocation(Dns.GetHostName(), backupDir, backupLocationStartDbNum, UInt32.MaxValue, session,
          PageInfo.compressionKind.None, PageInfo.encryptionKind.noEncryption, isBackupLocation, session.DatabaseLocations.Default());
        session.NewLocation(backupLocation);
        for (j = 1; j <= loops; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          aMan.Persist(place, session);
          aWoman = new Woman(aMan, aWoman);
          aWoman.Persist(place, session);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);               
          if (j % 1000000 == 0)
            Console.WriteLine("Loop # " + j);
        }
        UInt64 id = aWoman.Id;
        Console.WriteLine("Commit, done Loop # " + j);
        session.Commit();
      }
    }
    
    //[TestCase(true)]
    //[TestCase(false)]
    public void CreateDataWithBackupServerAutoPlacement(bool useServerSession)
    {
      int loops = 100000;
      int j;
      if (Directory.Exists(backupDir))
        Directory.Delete(backupDir, true); // remove systemDir from prior runs and all its databases.
      Directory.CreateDirectory(backupDir);
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        Man aMan = null;
        Woman aWoman = null;
        const bool isBackupLocation = true;
        session.BeginUpdate();
        // we need to have backup locations special since server is not supposed to do encryption or compression
        DatabaseLocation backupLocation = new DatabaseLocation(Dns.GetHostName(), backupDir, backupLocationStartDbNum, UInt32.MaxValue, session,
          PageInfo.compressionKind.None, PageInfo.encryptionKind.noEncryption, isBackupLocation, session.DatabaseLocations.Default());
        session.NewLocation(backupLocation);
        for (j = 1; j <= loops; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          session.Persist(aMan);
          aWoman = new Woman(aMan, aWoman);
          session.Persist(aWoman);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);
          if (j % 1000000 == 0)
            Console.WriteLine("Loop # " + j);
        }
        UInt64 id = aWoman.Id;
        Console.WriteLine("Commit, done Loop # " + j);
        session.Commit();
      }
    }
    [Test]
    public void CreateMoreDataWithBackupServer()
    {
      int loops = 30000;
      UInt16 objectsPerPage = 350;
      UInt16 pagesPerDatabase = 65000;
      int j;
      using (ServerClientSession session = new ServerClientSession(systemDir, Dns.GetHostName()))
      {
        Placement place = new Placement(11, 1, 1, objectsPerPage, pagesPerDatabase);
        Man aMan = null;
        Woman aWoman = null;
        session.BeginUpdate();
        for (j = 1; j <= loops; j++)
        {
          aMan = new Man(null, aMan, DateTime.UtcNow);
          aMan.Persist(place, session);
          aWoman = new Woman(aMan, aWoman);
          aWoman.Persist(place, session);
          aMan.m_spouse = new WeakIOptimizedPersistableReference<VelocityDbSchema.Person>(aWoman);          
          if (j % 1000000 == 0)
            Console.WriteLine("Loop # " + j);
        }
        UInt64 id = aWoman.Id;
        Console.WriteLine("Commit, done Loop # " + j);
        session.Commit();
      }
    }

    [Test]
    public void RestoreAll()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        DatabaseLocation backupLocation = session.DatabaseLocations.LocationForDb(backupLocationStartDbNum);
        session.RestoreFrom(backupLocation, DateTime.UtcNow);
        session.Commit(false, true);
      }
    }

    [Test]
    public void RestoreAllViaServer()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir, null, 2000, false)) // don't use optimistic locking for restore
      {
        session.BeginUpdate();
        DatabaseLocation backupLocation = new DatabaseLocation(Dns.GetHostName(), backupDir, backupLocationStartDbNum, UInt32.MaxValue, session,
          PageInfo.compressionKind.None, PageInfo.encryptionKind.noEncryption, true, session.DatabaseLocations.Default());
        session.RestoreFrom(backupLocation, DateTime.UtcNow);
        session.Commit(false, true);
      }
    }
  }
}
