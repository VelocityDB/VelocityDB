using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.NUnit
{
  public class Folder : FileOrFolder
  {
    BTreeSet<Folder> m_subFolders;
    BTreeSet<FileInDb> m_files;
    Folder m_parentFolder;

    public Folder(string name, Folder parentFolder, SessionBase session) : base(name)
    {
      CompareByField<FileInDb> comparerByFileName = new CompareByField<FileInDb>("m_name", session, false, true);
      m_files = new BTreeSet<FileInDb>(comparerByFileName, session, 10000, CommonTypes.s_sizeOfUInt32);
      CompareByField<Folder> comparerByFolderName = new CompareByField<Folder>("m_name", session, false, true);
      m_subFolders = new BTreeSet<Folder>(comparerByFolderName, session, 10000, CommonTypes.s_sizeOfUInt32);
      if (parentFolder != null)
      {
        m_parentFolder = parentFolder;
        m_parentFolder.Folders.AddFast(this);
      }
    }

    public BTreeSet<FileInDb> Files
    {
      get
      {
        return m_files;
      }
    }

    public BTreeSet<Folder> Folders
    {
      get
      {
        return m_subFolders;
      }
    }

    public override void Unpersist(SessionBase session)
    {
      if (IsPersistent)
      {
        foreach (var file in m_files.ToArray())
          file.Unpersist(session);
        m_files.Unpersist(session);
        foreach (var folder in m_subFolders.ToArray())
          folder.Unpersist(session);
        m_subFolders.Unpersist(session);
        base.Unpersist(session);
      }
    }

    public override Folder ParentFolder
    {
      get
      {
        return m_parentFolder;
      }
    }
  }
}
