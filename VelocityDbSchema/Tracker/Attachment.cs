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
    string comment;
    string fileName;
    string filePath;
    string contentType;
    byte[] fileContent;
    WeakIOptimizedPersistableReference<Issue>  issueAttachedTo;

    public Attachment(string filePath, string fileName, string note, byte[] content, string contentType, Issue issue)
    {
      this.fileName = fileName;
      this.filePath = filePath;
      this.comment = note;
      this.contentType = contentType;
      fileContent = content;
      if (issue != null)
        issueAttachedTo = new WeakIOptimizedPersistableReference<Issue>(issue);
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
        return comment;
      }
      set
      {
        Update();
        comment = value;
      }
    }

    public string ContentType
    {
      get
      {
        return contentType;
      }
    }

    public string FileName
    {
      get
      {
        return fileName;
      }
      set
      {
        Update();
        fileName = value;
      }
    }
    public byte[] FileContent
    {
      get
      {
        return fileContent;
      }
    }

    public WeakIOptimizedPersistableReference<Issue> IssueAttachedTo
    {
      get
      {
        return issueAttachedTo;
      }
      set
      {
        Update();
        issueAttachedTo = value;
      }
    }
    public override string ToString()
    {
      return FileName + " " + fileContent.Length.ToString() + " bytes";
    }
  }
}
