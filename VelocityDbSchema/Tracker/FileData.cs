using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.Tracker
{
  public class FileData : OptimizedPersistable
  {

  public FileData(FileInfo fileInfo)
    {
      m_fileInfo = fileInfo;
      m_fileName = fileInfo.Name;
      m_directoryName = fileInfo.DirectoryName;
      m_fileBinaryData = null;
    }
    [NonSerialized]
    FileInfo m_fileInfo;
    public void SetFileBytes()
    {
      FileStream file = m_fileInfo.OpenRead();
      m_fileBinaryData = new byte[file.Length];
      int offset = 0;
      int remaining = (int) file.Length; // how to handle larger files ???
      while (remaining > 0)
      {
        int read = file.Read(m_fileBinaryData, offset, remaining);
        if (read <= 0)
          throw new EndOfStreamException
              (String.Format("End of stream reached with {0} bytes left to read", remaining));
        remaining -= read;
        offset += read;
      }
    }

    public void SetFileInfo(string fileName)
    {
      m_fileInfo = new FileInfo(fileName);
    }
    string m_directoryName;
    string m_fileName;
#pragma warning disable 0169
    int m_length;
#pragma warning restore 0169
    byte[] m_fileBinaryData;
  }
}
