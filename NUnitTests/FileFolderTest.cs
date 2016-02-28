using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class FileFolderTest
  {
    static readonly string s_windrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = System.IO.Path.Combine(s_windrive, "NUnitTestDbs");
    static readonly string s_sampleFolder = System.IO.Path.Combine(s_windrive, "VelocityDbSetup");
    void NotifyBeforeCommit(SessionBase session)
    {
      var newlyPersisted = session.NewOids;
      Console.Out.WriteLine("Number of newly persisted: " + newlyPersisted.Count);
      foreach (var id in newlyPersisted)
      {
        var pObj = session.Open(id);
      }

      var newlyUnpersisted = session.DeletedOids;
      Console.Out.WriteLine("Number of newly unpersisted: " + newlyUnpersisted.Count);

      var updated = session.UpdatedOids;
      Console.Out.WriteLine("Number of updated: " + updated.Count);
      foreach (var id in updated)
      {
        var pObj = session.Open(id);
      }
    }

    void CreateDirectoriesAndFiles(DirectoryInfo dirInfo, Folder folder, SessionBase session)
    {
      foreach (DirectoryInfo dir in dirInfo.GetDirectories())
      {
        Folder subFolder = new Folder(dir.Name, folder, session);
        CreateDirectoriesAndFiles(dir, subFolder, session);
      }
      foreach (FileInfo fileInfo in dirInfo.GetFiles())
      {
        FileInDb file = new FileInDb(fileInfo.Name, folder);
        byte[] bytes = File.ReadAllBytes(fileInfo.FullName);
        FileContent fileContent = new FileContent(bytes);
        session.Persist(fileContent);
        file.Content = fileContent;
      }
    }

    [Test]
    public void DoFileFolderTest()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.NotifyBeforeCommit = NotifyBeforeCommit;
        session.BeginUpdate();
        DirectoryInfo dirInfo = new DirectoryInfo(s_sampleFolder);
        Folder folder = new Folder(dirInfo.Name, null, session);
        CreateDirectoriesAndFiles(dirInfo, folder, session);
        session.Commit();
      }
    }

    [Test]
    public void UnpersistFileFoldersTest()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.NotifyBeforeCommit = NotifyBeforeCommit;
        session.BeginUpdate();
        Assert.Less(10, session.AllObjects<Folder>().Count);
        Assert.Less(10, session.AllObjects<FileInDb>().Count);
        DirectoryInfo dirInfo = new DirectoryInfo(s_sampleFolder);
        Folder folder = (from f in session.AllObjects<Folder>(false) where f.Name == dirInfo.Name && f.ParentFolder == null select f).FirstOrDefault();
        folder.Unpersist(session);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        Assert.AreEqual(0, session.AllObjects<Folder>().Count);
        Assert.AreEqual(0, session.AllObjects<FileInDb>().Count);
        session.Commit();
      }
    }
  }
}
