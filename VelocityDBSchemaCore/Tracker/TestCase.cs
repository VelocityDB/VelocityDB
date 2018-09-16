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
    SortedSetAny<FileData> m_testCaseFiles;
    string m_description;
    public TestCase(string description)
    {
      this.m_description = description;
    }
    public void AddFile(string fileName)
    {
      FileInfo fileInfo = new FileInfo(fileName);
      FileData fileData = new FileData(fileInfo);
      if (m_testCaseFiles == null)
        m_testCaseFiles = new SortedSetAny<FileData>();
      m_testCaseFiles.Add(fileData);
      fileData.SetFileBytes();
    }
  }
}
