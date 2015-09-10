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
      this.fileInfo = fileInfo;
      fileName = fileInfo.Name;
      directoryName = fileInfo.DirectoryName;
      fileBinaryData = null;
    }
    [NonSerialized]
    FileInfo fileInfo;
    public void SetFileBytes()
    {
      FileStream file = fileInfo.OpenRead();
      fileBinaryData = new byte[file.Length];
      int offset = 0;
      int remaining = (int) file.Length; // how to handle larger files ???
      while (remaining > 0)
      {
        int read = file.Read(fileBinaryData, offset, remaining);
        if (read <= 0)
          throw new EndOfStreamException
              (String.Format("End of stream reached with {0} bytes left to read", remaining));
        remaining -= read;
        offset += read;
      }
    }

    public void SetFileInfo(string fileName)
    {
      fileInfo = new FileInfo(fileName);
    }
    string directoryName;
    string fileName;
#pragma warning disable 0169
    int length;
#pragma warning restore 0169
    byte[] fileBinaryData;
  }
}
