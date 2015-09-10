using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.Tracker
{
  public class TestCase : OptimizedPersistable
  {
    SortedSetAny<FileData> testCaseFiles;
    string description;
    public TestCase(string description)
    {
      this.description = description;
    }
    public void AddFile(string fileName)
    {
      FileInfo fileInfo = new FileInfo(fileName);
      FileData fileData = new FileData(fileInfo);
      if (testCaseFiles == null)
        testCaseFiles = new SortedSetAny<FileData>();
      testCaseFiles.Add(fileData);
      fileData.SetFileBytes();
    }
  }
}
