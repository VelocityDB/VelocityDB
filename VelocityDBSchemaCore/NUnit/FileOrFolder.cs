using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.NUnit
{
  public abstract class FileOrFolder : OptimizedPersistable
  {
    string m_name;

    public FileOrFolder(string name)
    {
      m_name = name;
    }

    public string Name
    {
      get
      {
        return m_name;
      }
    }

    public abstract Folder ParentFolder { get;  }
  }
}
