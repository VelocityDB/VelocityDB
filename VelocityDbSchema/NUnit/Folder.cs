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
    BTreeSet<FileInDb> m_files;
    BTreeSet<Folder> m_folders;

    public Folder(string name, Folder parentFolder, SessionBase session) : base(name, parentFolder)
    {
      CompareByField<FileInDb> comparerByFileName = new CompareByField<FileInDb>("m_name", session, false, true);
      m_files = new BTreeSet<FileInDb>(comparerByFileName, session, 10000, CommonTypes.s_sizeOfUInt32);
      CompareByField<Folder> comparerByFolderName = new CompareByField<Folder>("m_name", session, false, true);
      m_folders = new BTreeSet<Folder>(comparerByFolderName, session, 10000, CommonTypes.s_sizeOfUInt32);
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
        return m_folders;
      }
    }

    public override void Unpersist(SessionBase session, bool disableFlush = true)
    {
      if (IsPersistent)
      {
        foreach (FileInDb file in m_files)
          file.Unpersist(session);
        m_files.Unpersist(session);
        foreach (Folder folder in m_folders)
          folder.Unpersist(session);
        m_folders.Unpersist(session);
        base.Unpersist(session);
      }
    }
  }
}
