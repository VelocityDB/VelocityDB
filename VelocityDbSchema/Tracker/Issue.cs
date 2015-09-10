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
    SortedSetAny<Attachment> attachments;
#pragma warning disable 0414
    SortedSetAny<User> subscribers;
#pragma warning restore 0414
    User assignedTo;
    User lastUpdatedBy;
    User reportedBy;
    DateTime dateTimeLastUpdated;
    DateTime dateTimeCreated;
    DateTime dueDate;
    StatusEnum status;
#pragma warning disable 0169
    TimeSpan estimatedTimeToFix;
#pragma warning restore 0169
#pragma warning disable 0414
    TestCase testCase;
#pragma warning restore 0414
    SortedSetAny<Component> affectedComponentSet;
    SortedSetAny<ProductVersion> affectedVersionSet;
    SortedSetAny<Vote> voteSet;
    SortedSetAny<Issue> relatedIssueSet;
    SortedSetAny<ProductVersion> fixedInVersionSet;
    SortedSetAny<SubTask> subTaskSet;
    SortedSetAny<ProductLabel> labelSet;
    string summary; // a brief one line summary of the issue.
    string description; // longer text about the issue
    string environment; // like "Windows 7 32 bit" or ie8
    string fixmessage;
    Resolution fixResolution;
    PriorityEnum priority;
    CategoryEnum category;
    ProductVersion version;
    Project project;
    Component component;
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
        this.attachments = attachments;
      else
        this.attachments = new SortedSetAny<Attachment>();
      this.reportedBy = reportedBy;
      this.project = project;
      this.component = component;
      affectedComponentSet = new SortedSetAny<Component>(1);
      if (component != null)
        affectedComponentSet.Add(component);
      affectedVersionSet = new SortedSetAny<ProductVersion>(1);
      if (version != null)
        affectedVersionSet.Add(version);
      voteSet = new SortedSetAny<Vote>(1);
      relatedIssueSet = new SortedSetAny<Issue>(1);
      fixedInVersionSet = new SortedSetAny<ProductVersion>(1);
      subTaskSet = new SortedSetAny<SubTask>(1);
      labelSet = new SortedSetAny<ProductLabel>(1);
      this.version = version;
      this.summary = summary;
      this.description = description;
      this.environment = environment;
      this.category = category;
      this.priority = priority;
      fixResolution = resolution;
      dateTimeCreated = DateTime.Now;
      dateTimeLastUpdated = dateTimeCreated;
      fixmessage = null;
      lastUpdatedBy = reportedBy;
      this.dueDate = dueDate;
      this.status = status;
      this.AssignedTo = assignedTo;
      subscribers = null; // to do
      testCase = null;// to do
    }

    public SortedSetAny<Attachment> Attachments
    {
      get
      {
        return attachments;
      }
    }

    public Attachment[] AttachmentArray
    {
      get
      {
        if (attachments == null)
          return null;
        return attachments.Keys.ToArray();
      }
    }
    
    public User AssignedTo
    {
      get
      {
        return assignedTo;
      }
      set
      {
        Update();
        assignedTo = value;
      }
    }

    public CategoryEnum Category
    {
      get
      {
        return category;
      }
      set
      {
        Update();
        category = value;
      }
    }
    
    public Component Component
    {
      get
      {
        return component;
      }
      set
      {
        Update();
        component = value;
      }
    }
    
    public DateTime DateTimeCreated
    {
      get
      {
        return dateTimeCreated;
      }
    }

    public DateTime DateTimeLastUpdated
    {
      get
      {
        return dateTimeLastUpdated;
      }
      set
      {
        Update();
        dateTimeLastUpdated = value;
      }
    }

    public string Description
    {
      get
      {
        return description;
      }
      set
      {
        Update();
        description = value;
      }
    }
    
    public DateTime DueDate
    {
      get
      {
        return dueDate;
      }
      set
      {
        Update();
        dueDate = value;
      }
    }

    public string Environment
    {
      get
      {
        return environment;
      }
      set
      {
        Update();
        environment = value;
      }
    }

    public string FixMessage
    {
      get
      {
        return fixmessage;
      }
      set
      {
        Update();
        fixmessage = value;
      }
    }

    public Resolution FixResolution
    {
      get
      {
        return fixResolution;
      }
      set
      {
        Update();
        fixResolution = value;
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
        return lastUpdatedBy;
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
        return priority;
      }
      set
      {
        Update();
        priority = value;
      }
    }
    public Project Project
    {
      get
      {
        return project;
      }
      set
      {
        Update();
        project = value;
      }
    }

    public User ReportedBy
    {
      get
      {
        return reportedBy;
      }
      set
      {
        Update();
        reportedBy = value;
      }
    }

    public StatusEnum Status
    {
      get
      {
        return status;
      }
      set
      {
        status = value;
      }
    }

    public string Summary
    {
      get
      {
        return summary;
      }
      set
      {
        Update();
        summary = value;
      }
    }

    public ProductVersion Version
    {
      get
      {
        return version;
      }
      set
      {
        Update();
        version = value;
      }
    }
  }
}
