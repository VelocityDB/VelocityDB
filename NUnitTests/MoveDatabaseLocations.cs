using NUnit.Framework;
using System;
using System.IO;
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
    public const string otherDbDir = "MoveDatabaseLocationTestOther";
    public const string otherDbDir2 = "MoveDatabaseLocationTestOther2";
    public string systemHost = SessionBase.LocalHost;
    public string systemHost2 = "FindPriceBuy";
    public const UInt32 otherStartdbId = 100;

    public void verifyDatabaseLocations(SessionBase session)
    {
      session.BeginRead();
      foreach (Person person in session.AllObjects<Person>())
      {
        Console.WriteLine(person.ToString());
        Assert.AreEqual(person.FirstName, "Mats");
      }
      Database db = session.OpenDatabase(otherStartdbId);
      foreach (Person person in db.AllObjects<Person>())
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
      var otherLocation = new DatabaseLocation(session.SystemHostName, otherDbDir, otherStartdbId, session.DefaultDatabaseLocation().EndDatabaseNumber, session);
      Placement place = new Placement(otherStartdbId);
      Person person2 = new Person("Mats", "Persson", 27);
      session.Persist(place, person2);
      session.Commit();
      verifyDatabaseLocations(session);
    }

    public void moveDatabaseLocations(SessionBase session)
    {
      session.RelocateDefaultDatabaseLocation();
      session.RelocateDatabaseLocationFor(otherStartdbId, session.SystemHostName, otherDbDir2);
    }

    [Test]
    //[Repeat(3)]
    public void moveDatabaseLocations()
    {
      SessionBase.BaseDatabasePath = "c:/Databases"; // use same as VelocityDbServer.exe.config 
      DirectoryInfo info = new DirectoryInfo(Path.Combine(SessionBase.BaseDatabasePath,  systemDir));
     if (info.Exists)
        info.Delete(true);
      DirectoryInfo newInfo = new DirectoryInfo(Path.Combine(SessionBase.BaseDatabasePath,  systemDir + "MovedTo"));
      string newPath = newInfo.FullName;
      if (newInfo.Exists)
        newInfo.Delete(true);
      createDatabaseLocations(new SessionNoServer(systemDir));
      info.MoveTo(newPath);
      moveDatabaseLocations(new SessionNoServer(newPath, 2000, false, false));
      verifyDatabaseLocations(new SessionNoServer(newPath));
      info.Delete(true);

      info = new DirectoryInfo(Path.Combine(SessionBase.BaseDatabasePath, systemDir));
      if (info.Exists)
        info.Delete(true);
      createDatabaseLocations(new ServerClientSession(systemDir));
      newPath = Path.Combine(SessionBase.BaseDatabasePath, systemDir + "MovedTo");
      info.MoveTo(newPath);
      moveDatabaseLocations(new ServerClientSession(newPath, systemHost, 2000, false, false));
      verifyDatabaseLocations(new ServerClientSession(newPath, systemHost));
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
        moveDatabaseLocations(new ServerClientSession(newPath, systemHost, 2000, false, false));
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
