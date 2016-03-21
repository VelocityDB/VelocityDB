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
  public class Folder : FileOrFolder, IRelation<Folder, FileInDb>, IRelation<Folder, Folder>
  {
    Relation<Folder, Folder> m_folderRelation;
    BTreeSet<FileInDb> m_files;
    BTreeSet<Folder> m_folders;

    public Folder(string name, Folder parentFolder, SessionBase session) : base(name)
    {
      CompareByField<FileInDb> comparerByFileName = new CompareByField<FileInDb>("m_name", session, false, true);
      m_files = new BTreeSet<FileInDb>(comparerByFileName, session, 10000, CommonTypes.s_sizeOfUInt32);
      CompareByField<Folder> comparerByFolderName = new CompareByField<Folder>("m_name", session, false, true);
      m_folders = new BTreeSet<Folder>(comparerByFolderName, session, 10000, CommonTypes.s_sizeOfUInt32);
      session.Persist(this);
      if (parentFolder != null)
      {
        Update();
        m_folderRelation = new Relation<Folder, Folder>(this, parentFolder);
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
        return m_folders;
      }
    }

    public override void Unpersist(SessionBase session)
    {
      if (IsPersistent)
      {
        if (m_folderRelation != null)
          m_folderRelation.Unpersist(session);
        foreach (FileInDb file in m_files.ToArray()) // ToArray because file.Unpersist modifies m_files.
          file.Unpersist(session);
        m_files.Unpersist(session);
        foreach (Folder folder in m_folders.ToArray())
          folder.Unpersist(session);
        m_folders.Unpersist(session);
        base.Unpersist(session);
      }
    }

    public void AddRelation(Relation<FileInDb, Folder> relation)
    {
      m_files.AddFast(relation.RelationFrom);
    }

    public void AddRelation(Relation<Folder, Folder> relation)
    {
      m_folders.AddFast(relation.RelationFrom);
    }
    public bool RelationExist(Relation<FileInDb, Folder> relation)
    {
      return m_files.Contains(relation.RelationFrom);
    }

    public bool RelationExist(Relation<Folder, Folder> relation)
    {
      return m_folders.Contains(relation.RelationFrom);
    }

    public void RemoveRelation(Relation<FileInDb, Folder> relation)
    {
      m_files.Remove(relation.RelationFrom);
    }

    public void RemoveRelation(Relation<Folder, Folder> relation)
    {
      m_folders.Remove(relation.RelationFrom);
    }

    public override Folder ParentFolder
    {
      get
      {
        if (m_folderRelation != null)
          return m_folderRelation.RelationTo;
        return null;
      }
    }
  }
}
