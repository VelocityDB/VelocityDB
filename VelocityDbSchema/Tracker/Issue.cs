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
  public class Issue : OptimizedPersistable
  {
    SortedSetAny<Attachment> m_attachments;
#pragma warning disable 0414
    SortedSetAny<User> m_subscribers;
#pragma warning restore 0414
    User m_assignedTo;
    User m_lastUpdatedBy;
    User m_reportedBy;
    DateTime m_dateTimeLastUpdated;
    DateTime m_dateTimeCreated;
    DateTime m_dueDate;
    StatusEnum m_status;
#pragma warning disable 0169
    TimeSpan m_estimatedTimeToFix;
#pragma warning restore 0169
#pragma warning disable 0414
    TestCase m_testCase;
#pragma warning restore 0414
    SortedSetAny<Component> m_affectedComponentSet;
    SortedSetAny<ProductVersion> m_affectedVersionSet;
    SortedSetAny<Vote> m_voteSet;
    SortedSetAny<Issue> m_relatedIssueSet;
    SortedSetAny<ProductVersion> m_fixedInVersionSet;
    SortedSetAny<SubTask> m_subTaskSet;
    SortedSetAny<ProductLabel> m_labelSet;
    string m_summary; // a brief one line summary of the issue.
    string m_description; // longer text about the issue
    string m_environment; // like "Windows 7 32 bit" or ie8
    string m_fixmessage;
    Resolution m_fixResolution;
    PriorityEnum m_priority;
    CategoryEnum m_category;
    ProductVersion m_version;
    Project m_project;
    Component m_component;
    public enum Resolution : byte { Incomplete, Fixed, WontFix, Dublicate, CannotReproduce };
    public enum SecurityLevel : byte { All, Reporters, Developers};
    public enum PriorityEnum : byte { Blocker, Critical, Major, Minor, Trivial };
    public enum CategoryEnum : byte { Bug, Improvement, NewFeature, Task, CustomIssue };
    public enum StatusEnum : byte { Open, InProgress, Resolved, Reopened, Closed };

    public Issue() { }
    public Issue(User reportedBy, PriorityEnum priority, Project project, CategoryEnum category, Component component, ProductVersion version, Resolution resolution,
      string summary, string description, string environment, User assignedTo, DateTime dueDate, SortedSetAny<Attachment> attachments, StatusEnum status = StatusEnum.Open)
    {
      if (attachments != null)
        m_attachments = attachments;
      else
        m_attachments = new SortedSetAny<Attachment>();
      m_reportedBy = reportedBy;
      m_project = project;
      m_component = component;
      m_affectedComponentSet = new SortedSetAny<Component>(1);
      if (component != null)
        m_affectedComponentSet.Add(component);
      m_affectedVersionSet = new SortedSetAny<ProductVersion>(1);
      if (version != null)
        m_affectedVersionSet.Add(version);
      m_voteSet = new SortedSetAny<Vote>(1);
      m_relatedIssueSet = new SortedSetAny<Issue>(1);
      m_fixedInVersionSet = new SortedSetAny<ProductVersion>(1);
      m_subTaskSet = new SortedSetAny<SubTask>(1);
      m_labelSet = new SortedSetAny<ProductLabel>(1);
      m_version = version;
      m_summary = summary;
      m_description = description;
      m_environment = environment;
      m_category = category;
      m_priority = priority;
      m_fixResolution = resolution;
      m_dateTimeCreated = DateTime.Now;
      m_dateTimeLastUpdated = m_dateTimeCreated;
      m_fixmessage = null;
      m_lastUpdatedBy = reportedBy;
      m_dueDate = dueDate;
      m_status = status;
      AssignedTo = assignedTo;
      m_subscribers = null; // to do
      m_testCase = null;// to do
    }

    public SortedSetAny<Attachment> Attachments
    {
      get
      {
        return m_attachments;
      }
    }

    public Attachment[] AttachmentArray
    {
      get
      {
        if (m_attachments == null)
          return null;
        return m_attachments.Keys.ToArray();
      }
    }
    
    public User AssignedTo
    {
      get
      {
        return m_assignedTo;
      }
      set
      {
        Update();
        m_assignedTo = value;
      }
    }

    public CategoryEnum Category
    {
      get
      {
        return m_category;
      }
      set
      {
        Update();
        m_category = value;
      }
    }
    
    public Component Component
    {
      get
      {
        return m_component;
      }
      set
      {
        Update();
        m_component = value;
      }
    }
    
    public DateTime DateTimeCreated
    {
      get
      {
        return m_dateTimeCreated;
      }
    }

    public DateTime DateTimeLastUpdated
    {
      get
      {
        return m_dateTimeLastUpdated;
      }
      set
      {
        Update();
        m_dateTimeLastUpdated = value;
      }
    }

    public string Description
    {
      get
      {
        return m_description;
      }
      set
      {
        Update();
        m_description = value;
      }
    }
    
    public DateTime DueDate
    {
      get
      {
        return m_dueDate;
      }
      set
      {
        Update();
        m_dueDate = value;
      }
    }

    public string Environment
    {
      get
      {
        return m_environment;
      }
      set
      {
        Update();
        m_environment = value;
      }
    }

    public string FixMessage
    {
      get
      {
        return m_fixmessage;
      }
      set
      {
        Update();
        m_fixmessage = value;
      }
    }

    public Resolution FixResolution
    {
      get
      {
        return m_fixResolution;
      }
      set
      {
        Update();
        m_fixResolution = value;
      }
    } 
    
    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 20;
      }
    }

    public User LastUpdatedBy
    {
      get
      {
        return m_lastUpdatedBy;
      }
    }


    /// <summary>
    /// A default for number of objects per database page used when persiting objects without an explicit <see cref="Placement"/> object. This happens when objects are persited by reachability from a persistent object.
    /// All objects reachable from a persistent object are automaticly made persistent.
    /// </summary>
    public override UInt16 ObjectsPerPage
    {
      get
      {
        return 99;
      }
    }
  
    public PriorityEnum Priority
    {
      get
      {
        return m_priority;
      }
      set
      {
        Update();
        m_priority = value;
      }
    }
    public Project Project
    {
      get
      {
        return m_project;
      }
      set
      {
        Update();
        m_project = value;
      }
    }

    public User ReportedBy
    {
      get
      {
        return m_reportedBy;
      }
      set
      {
        Update();
        m_reportedBy = value;
      }
    }

    public StatusEnum Status
    {
      get
      {
        return m_status;
      }
      set
      {
        m_status = value;
      }
    }

    public string Summary
    {
      get
      {
        return m_summary;
      }
      set
      {
        Update();
        m_summary = value;
      }
    }

    public ProductVersion Version
    {
      get
      {
        return m_version;
      }
      set
      {
        Update();
        m_version = value;
      }
    }
  }
}
