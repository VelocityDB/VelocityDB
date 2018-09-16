using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.NUnit
{
  public class FileInDb : FileOrFolder
  {
    Folder m_folder;
    WeakIOptimizedPersistableReference<FileContent> m_fileContent;

    public FileInDb(string name, Folder parentFolder):base(name)
    {
      m_folder = parentFolder;
      m_folder.Files.AddFast(this);
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

    public override Folder ParentFolder
    {
      get
      {
        return m_folder;
      }
    }

    public override void Unpersist(SessionBase session)
    {
      m_folder.Files.Remove(this);
      if (m_fileContent != null)
        Content.Unpersist(session);
      base.Unpersist(session);
    }
  }
}
