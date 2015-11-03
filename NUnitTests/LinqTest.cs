using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDbSchema;
using VelocityDbSchema.NUnit;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class LinqTest
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    [Test]
    public void OneMillionCreate()
    {
      Int32 recordsTocreate = 1000000;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        ComputerFileData velrecord;
        for (int i = 0; i < recordsTocreate; i++)
        {
          velrecord = (new ComputerFileData()
                 {
                   FullPath = @"c:\test\file" + i,
                   ItemDeleted = true,
                   PackagingErrors = "",
                   PackagedSuccessfully = false,
                   FileSelected = true,
                   WDSSelected = false,
                   FPPSearchSelected = false,
                   RegexSelected = false,
                   SharepointSelected = false,
                   SharepointIndexedSearchSelected = false,
                   WebSelected = false,
                   DateFilterExcluded = false,
                   IncludeFilterExcluded = false,
                   ExcludeFilterExcluded = false,
                   ContainerID = 1,
                   FolderID = 1,
                   FileID = i,
                   ParentFileID = null,
                   //ParentFileID = 0,
                   Category = "cat",
                   ResourceImageType = "",
                   FolderPath = "",
                   LastAccessTime = DateTime.UtcNow,
                   LastWriteTime = DateTime.UtcNow,
                   CreationTime = DateTime.UtcNow,
                   Size = 40000,
                 });
          session.Persist(velrecord);
        }
        session.Commit();
      }
    }

    [Test]
    public void OneMillionFindSingleRecordInTheMiddle()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var result = (from ComputerFileData computerFileData in session.AllObjects<ComputerFileData>()
                      where computerFileData.FileID == 500000
                      select computerFileData).First();
        session.Commit();
      }
    }

    [Test]
    public void OneMillionFindSingleRecordInTheMiddleServer()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginRead();
        var result = (from ComputerFileData computerFileData in session.AllObjects<ComputerFileData>()
                      where computerFileData.FileID == 500000
                      select computerFileData).First();
        session.Commit();
      }
    }

    [Test]
    public void OneMillionFindSingleRecordInTheMiddleNoLinq()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var computerFileDataEnum = session.AllObjects<ComputerFileData>();
        foreach (ComputerFileData computerFileData in computerFileDataEnum)
        {
          if (computerFileData.FileID == 500000)
            break; // found it
        }
        session.Commit();
      }
    }
  }
}
