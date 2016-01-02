using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.Comparer;
using VelocityDb.Session;

namespace VelocityDbSchema.Tracker
{
  public class IssueTracker : OptimizedPersistable
  {
    SortedSetAny<Issue> m_issueSetByDescription; 
    SortedSetAny<Issue> m_issueSetById;
    SortedSetAny<Issue> m_issueSetByPriority;
    SortedSetAny<Issue> m_issueSetByDateTimeCreated;
    SortedSetAny<Issue> m_issueSetByDateTimeUpdated;
    SortedSetAny<Issue> m_issueSetByStatus;
    SortedSetAny<Issue> m_issueSetBySummary;
    SortedSetAny<Issue> m_issueSetByProject;
    SortedSetAny<Issue> m_issueSetByCategory;
    SortedSetAny<Issue> m_issueSetByReportedBy;
    SortedSetAny<Issue> m_issueSetByLastUpdatedBy;
    SortedSetAny<Issue> m_issueSetByAssignedTo;
    SortedSetAny<Issue> m_issueSetByDueDate;
    SortedSetAny<Issue> m_issueSetByVersion;
    SortedSetAny<Component> m_componentSet;
    SortedSetAny<User> m_userSet;
    SortedSetAny<Project> m_projectSet;
    SortedSetAny<Organization> m_organizationSet;
#pragma warning disable 0169
    SortedSetAny<ProductLabel> m_labelSet;
#pragma warning restore 0169
    SortedSetAny<ProductVersion> m_versionSet;
    PermissionScheme m_permissions;

    public IssueTracker(int capacity, SessionBase session)
    {
      m_issueSetById = new SortedSetAny<Issue>(capacity);
      CompareByField<Issue> descriptionCompare = new CompareByField<Issue>("m_description", session, true);
      m_issueSetByDescription = new SortedSetAny<Issue>(descriptionCompare); 
      ComparePriority priorityCompare = new ComparePriority();
      m_issueSetByPriority = new SortedSetAny<Issue>(priorityCompare);
      CompareByField<Issue> dateTimeCreatedCompare = new CompareByField<Issue>("m_dateTimeCreated", session, true);
      m_issueSetByDateTimeCreated = new SortedSetAny<Issue>(dateTimeCreatedCompare);
      CompareByField<Issue> dateTimeUpdatedCompare = new CompareByField<Issue>("m_dateTimeLastUpdated", session, true);
      m_issueSetByDateTimeUpdated = new SortedSetAny<Issue>(dateTimeCreatedCompare);
      CompareByField<Issue> compareStatus = new CompareByField<Issue>("m_status", session, true);
      m_issueSetByStatus = new SortedSetAny<Issue>(compareStatus);
      CompareSummary compareSummary = new CompareSummary();
      m_issueSetBySummary = new SortedSetAny<Issue>(compareSummary);
      CompareByField<Issue> compareProject = new CompareByField<Issue>("m_project", session, true);
      m_issueSetByProject = new SortedSetAny<Issue>(compareProject);
      CompareCategory compareCategory = new CompareCategory();
      m_issueSetByCategory = new SortedSetAny<Issue>(compareCategory);
      CompareReportedBy compareReportedBy = new CompareReportedBy();
      m_issueSetByReportedBy = new SortedSetAny<Issue>(compareReportedBy);
      CompareLastUpdatedBy compareLastUpdatedBy = new CompareLastUpdatedBy();
      m_issueSetByLastUpdatedBy = new SortedSetAny<Issue>(compareLastUpdatedBy);
      CompareAssignedTo compareAssignedTo = new CompareAssignedTo();
      m_issueSetByAssignedTo = new SortedSetAny<Issue>(compareAssignedTo);
      CompareByField<Issue> compareByDueDate = new CompareByField<Issue>("m_dueDate", session, true);
      m_issueSetByDueDate = new SortedSetAny<Issue>(compareByDueDate);
      CompareByVersion compareByVersion = new CompareByVersion();
      m_issueSetByVersion = new SortedSetAny<Issue>(compareByVersion);
      m_componentSet = new SortedSetAny<Component>(capacity);
      m_userSet = new SortedSetAny<User>(capacity);
      m_projectSet = new SortedSetAny<Project>(capacity);
      m_versionSet = new SortedSetAny<ProductVersion>(capacity);
      m_organizationSet = new SortedSetAny<Organization>(capacity);
      m_permissions = null;
    }

    public void Add(Issue issue)
    {
      m_issueSetById.Add(issue); 
      m_issueSetByDescription.Add(issue);
      m_issueSetByPriority.Add(issue);
      m_issueSetByDateTimeCreated.Add(issue);
      m_issueSetByDateTimeUpdated.Add(issue);
      m_issueSetByStatus.Add(issue);
      m_issueSetBySummary.Add(issue);
      m_issueSetByProject.Add(issue);
      m_issueSetByCategory.Add(issue);
      m_issueSetByReportedBy.Add(issue);
      m_issueSetByLastUpdatedBy.Add(issue);
      m_issueSetByAssignedTo.Add(issue);
      m_issueSetByDueDate.Add(issue);
      m_issueSetByVersion.Add(issue);
    }

   public bool Remove(Issue issue)
    {
      return m_issueSetById.Remove(issue) &&
             m_issueSetByDescription.Remove(issue) &&
             m_issueSetByPriority.Remove(issue) &&
             m_issueSetByDateTimeCreated.Remove(issue) &&
             m_issueSetByDateTimeUpdated.Remove(issue) &&
             m_issueSetByStatus.Remove(issue) &&
             m_issueSetBySummary.Remove(issue) &&
             m_issueSetByProject.Remove(issue) &&
             m_issueSetByCategory.Remove(issue) &&
             m_issueSetByReportedBy.Remove(issue) &&
             m_issueSetByLastUpdatedBy.Remove(issue) &&
             m_issueSetByAssignedTo.Remove(issue) &&
             m_issueSetByDueDate.Remove(issue) &&
             m_issueSetByVersion.Remove(issue);
    }

   public SortedSetAny<Issue> IssueSetById
    {
      get
      {
        return m_issueSetById;
      }
    }

    public SortedSetAny<Issue> IssueSetByAssignedTo
    {
      get
      {
        return m_issueSetByAssignedTo;
      }
    }

    public SortedSetAny<Issue> IssueSetByCategory
    {
      get
      {
        return m_issueSetByCategory;
      }
    }

    public SortedSetAny<Issue> IssueSetByDueDate
    {
      get
      {
        return m_issueSetByDueDate;
      }
    }

    public SortedSetAny<Issue> IssueSetByLastUpdatedBy
    {
      get
      {
        return m_issueSetByLastUpdatedBy;
      }
    }

    public SortedSetAny<Issue> IssueSetByProject
    {
      get
      {
        return m_issueSetByProject;
      }
    }

    public SortedSetAny<Issue> IssueSetByDateTimeCreated
    {
      get
      {
        return m_issueSetByDateTimeCreated;
      }
    }


    public SortedSetAny<Issue> IssueSetByDateTimeUpdated
    {
      get
      {
        return m_issueSetByDateTimeUpdated;
      }
    }

    public SortedSetAny<Issue> IssueSetByReportedBy
    {
      get
      {
        return m_issueSetByReportedBy;
      }
    }

    public SortedSetAny<Issue> IssueSetByStatus
    {
      get
      {
        return m_issueSetByStatus;
      }
    }

    public SortedSetAny<Issue> IssueSetBySummary
    {
      get
      {
        return m_issueSetBySummary;
      }
    }

    public SortedSetAny<Issue> IssueSetByPriority
    {
      get
      {
        return m_issueSetByPriority;
      }
    }

    public SortedSetAny<Issue> IssueSetByVersion
    {
      get
      {
        return m_issueSetByVersion;
      }
    }

    public SortedSetAny<Component> ComponentSet
    {
      get
      {
        return m_componentSet;
      }
    }

    public PermissionScheme Permissions
    {
      get
      {
        return m_permissions;
      }
      set
      {
        Update();
        m_permissions = value;
      }
    }

    public SortedSetAny<Project> ProjectSet
    {
      get
      {
        return m_projectSet;
      }
    }
    
    public SortedSetAny<User> UserSet
    {
      get
      {
        return m_userSet;
      }
    }

    public SortedSetAny<ProductVersion> VersionSet
    {
      get
      {
        return m_versionSet;
      }
    }
  }
}
