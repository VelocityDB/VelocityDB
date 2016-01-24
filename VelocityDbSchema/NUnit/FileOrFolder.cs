using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class FileOrFolder : OptimizedPersistable
  {
    string m_name;
    Folder m_parentFolder;

    public FileOrFolder(string name, Folder parentFolder)
    {
      m_parentFolder = parentFolder;
      m_name = name;
    }

    public string Name
    {
      get
      {
        return m_name;
      }
    }

    public Folder ParentFolder
    {
      get
      {
        return m_parentFolder;
      }
    }
  }
}
