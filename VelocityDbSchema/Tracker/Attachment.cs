using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
namespace VelocityDbSchema.Tracker
{
   [Serializable]
  public class Attachment : OptimizedPersistable
  {
    string m_comment;
    string m_fileName;
    string m_filePath;
    string m_contentType;
    byte[] m_fileContent;
    WeakIOptimizedPersistableReference<Issue>  m_issueAttachedTo;

    public Attachment(string filePath, string fileName, string note, byte[] content, string contentType, Issue issue)
    {
      m_fileName = fileName;
      m_filePath = filePath;
      m_comment = note;
      m_contentType = contentType;
      m_fileContent = content;
      if (issue != null)
        m_issueAttachedTo = new WeakIOptimizedPersistableReference<Issue>(issue);
    }

    public override int CompareTo(object obj)
    {
      if (obj is Attachment)
      {
        Attachment otherAttachment = (Attachment)obj;
        return this.FileName.CompareTo(otherAttachment.FileName);
      }
      else
      {
        throw new ArgumentException("object is not an Attachment");
      }
    }

    public string Comment
    {
      get
      {
        return m_comment;
      }
      set
      {
        Update();
        m_comment = value;
      }
    }

    public string ContentType
    {
      get
      {
        return m_contentType;
      }
    }

    public string FileName
    {
      get
      {
        return m_fileName;
      }
      set
      {
        Update();
        m_fileName = value;
      }
    }
    public byte[] FileContent
    {
      get
      {
        LoadFields();
        return m_fileContent;
      }
    }

    public WeakIOptimizedPersistableReference<Issue> IssueAttachedTo
    {
      get
      {
        return m_issueAttachedTo;
      }
      set
      {
        Update();
        m_issueAttachedTo = value;
      }
    }

    public override bool LazyLoadFields
    {
      get
      {
        return true;
      }
    }

    public override string ToString()
    {
      if (FieldsLoaded)
        return FileName + " " + m_fileContent.Length.ToString() + " bytes";
      return FileName;
    }
  }
}
