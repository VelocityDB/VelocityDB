using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample4;

namespace NUnitTests
{
  [TestFixture]
  public class MoveDatabaseLocations
  {
    public const string systemDir = "MoveDatabaseLocationTest";
    public const string systemDir2 = "MoveDatabaseLocationTest2";
    public string systemHost = "Asus";
    public string systemHost2 = "FindPriceBuy";

    public void verifyDatabaseLocations(SessionBase session)
    {
      session.BeginRead();
      foreach (Person person in session.AllObjects<Person>())
      {
        Console.WriteLine(person.ToString());
        Assert.AreEqual(person.FirstName, "Mats");
      }
      session.Commit();
      session.Verify();
    }

    public void createDatabaseLocations(SessionBase session)
    {
      session.BeginUpdate();
      Person person = new Person("Mats", "Persson", 54);
      session.Persist(person);
      session.Commit();
      verifyDatabaseLocations(session);
    }

    public void moveDatabaseLocations(SessionBase session, string updatedHostName, string newPath)
    {
        session.BeginUpdate(false, true);
        DatabaseLocation bootLocation = session.DatabaseLocations.LocationForDb(0);
        DatabaseLocation locationNew = new DatabaseLocation(updatedHostName, newPath, bootLocation.StartDatabaseNumber, bootLocation.EndDatabaseNumber, session,
            bootLocation.CompressPages, bootLocation.PageEncryption, bootLocation.IsBackupLocation, bootLocation.BackupOfOrForLocation);
        bootLocation = session.NewLocation(locationNew);
        session.Commit(false);
    }

    [Test]
    //[Repeat(3)]
    public void moveDatabaseLocations()
    {
      DirectoryInfo info = new DirectoryInfo(Path.Combine(SessionBase.BaseDatabasePath,  systemDir));
      if (info.Exists)
        info.Delete(true);
      createDatabaseLocations(new SessionNoServer(systemDir));
      string newPath = Path.Combine(SessionBase.BaseDatabasePath,  systemDir + "MovedTo");
      info.MoveTo(newPath);
      moveDatabaseLocations(new SessionNoServer(newPath, 2000, false, false), systemHost, newPath);
      verifyDatabaseLocations(new SessionNoServer(newPath));
      info.Delete(true);

      info = new DirectoryInfo(Path.Combine(SessionBase.BaseDatabasePath, systemDir));
      if (info.Exists)
        info.Delete(true);
      createDatabaseLocations(new ServerClientSession(systemDir));
      newPath = Path.Combine(SessionBase.BaseDatabasePath, systemDir + "MovedTo");
      info.MoveTo(newPath);
      moveDatabaseLocations(new ServerClientSession(newPath, systemHost, 2000, false, false), systemHost, newPath);
      verifyDatabaseLocations(new ServerClientSession(newPath));
      info.Delete(true);

      info = new DirectoryInfo(Path.Combine(SessionBase.BaseDatabasePath, systemDir));
      if (info.Exists)
        info.Delete(true);
      string d = SessionBase.BaseDatabasePath;
      try
      {
        SessionBase.BaseDatabasePath = "\\\\" + systemHost2 + "\\Shared";
        newPath = Path.Combine(SessionBase.BaseDatabasePath, systemDir);
        info = new DirectoryInfo(newPath);
        createDatabaseLocations(new ServerClientSession(newPath, systemHost2));
        SessionBase.BaseDatabasePath = d;
        newPath = Path.Combine(SessionBase.BaseDatabasePath, systemDir + "MovedTo");
        string[] files = Directory.GetFiles(info.FullName);
        Directory.CreateDirectory(newPath);
        foreach (string file in files)
        {
          string name = Path.GetFileName(file);
          string dest = Path.Combine(newPath, name);
          File.Copy(file, dest);
        }
        info.Delete(true);
        info = new DirectoryInfo(newPath);
        moveDatabaseLocations(new ServerClientSession(newPath, systemHost, 2000, false, false), systemHost, newPath);
        verifyDatabaseLocations(new ServerClientSession(newPath));
        info.Delete(true);
      }
      finally
      {
        SessionBase.BaseDatabasePath = d;
      }
    }
  }
}
