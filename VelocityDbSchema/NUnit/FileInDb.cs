using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.NUnit
{
  public class FileInDb : FileOrFolder, IRelation<FileInDb, Folder>
  {
    Relation<FileInDb, Folder> m_folderRelation;
    WeakIOptimizedPersistableReference<FileContent> m_fileContent;

    public FileInDb(string name, Folder parentFolder):base(name)
    {
      m_folderRelation = new Relation<FileInDb, Folder>(this, parentFolder);
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
    public void AddRelation(Relation<Folder, FileInDb> reverseRelation)
    {
      if (m_folderRelation != null)
        m_folderRelation.RelationTo = reverseRelation.RelationFrom;
      else
      {
        Update();
        m_folderRelation = new Relation<FileInDb, Folder>(this, reverseRelation.RelationFrom);
      }
    }

    public void RemoveRelation(Relation<Folder, FileInDb> reverseRelation)
    {
      Unpersist(Session);
    }

    public bool RelationExist(Relation<Folder, FileInDb> reverseRelation)
    {
      return m_folderRelation != null;
    }

    public static byte[] ReadAllBytes(string fullName)
    {
      throw new NotImplementedException();
    }

    public override Folder ParentFolder
    {
      get
      {
        return m_folderRelation.RelationTo;
      }
    }

    public override void Unpersist(SessionBase session, bool disableFlush = true)
    {
      m_folderRelation.Unpersist(session);
      if (m_fileContent != null)
        Content.Unpersist(session);
      base.Unpersist(session, disableFlush);
    }
  }
}
