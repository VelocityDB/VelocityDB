using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class FileInDb : FileOrFolder
  {
    WeakIOptimizedPersistableReference<FileContent> m_fileContent;

    public FileInDb(string name, Folder parentFolder):base(name, parentFolder)
    {
    }

    public FileContent Content
    {
      get
      {
        if (m_fileContent == null)
          return null;
        return m_fileContent.GetTarget(false, Session);
      }
      set
      {
        if (value == null)
          m_fileContent = null;
        else if (m_fileContent == null)
          m_fileContent = new WeakIOptimizedPersistableReference<FileContent>(value);
        else
          m_fileContent.Id = value.Id;
      }
    }

    public static byte[] ReadAllBytes(string fullName)
    {
      throw new NotImplementedException();
    }
  }
}
