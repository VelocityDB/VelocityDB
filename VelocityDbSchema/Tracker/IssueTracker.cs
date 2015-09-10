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
    public const UInt32 PlaceInDatabase = 11;
    SortedSetAny<Issue> issueSetByDescription; 
    SortedSetAny<Issue> issueSetById;
    SortedSetAny<Issue> issueSetByPriority;
    SortedSetAny<Issue> issueSetByDateTimeCreated;
    SortedSetAny<Issue> issueSetByDateTimeUpdated;
    SortedSetAny<Issue> issueSetByStatus;
    SortedSetAny<Issue> issueSetBySummary;
    SortedSetAny<Issue> issueSetByProject;
    SortedSetAny<Issue> issueSetByCategory;
    SortedSetAny<Issue> issueSetByReportedBy;
    SortedSetAny<Issue> issueSetByLastUpdatedBy;
    SortedSetAny<Issue> issueSetByAssignedTo;
    SortedSetAny<Issue> issueSetByDueDate;
    SortedSetAny<Issue> issueSetByVersion;
    SortedSetAny<Component> componentSet;
    SortedSetAny<User> userSet;
    SortedSetAny<Project> projectSet;
    SortedSetAny<Organization> organizationSet;
#pragma warning disable 0169
    SortedSetAny<ProductLabel> labelSet;
#pragma warning restore 0169
    SortedSetAny<ProductVersion> versionSet;
    PermissionScheme permissions;

    public IssueTracker(int capacity, SessionBase session)
    {
      issueSetById = new SortedSetAny<Issue>(capacity);
      CompareByField<Issue> descriptionCompare = new CompareByField<Issue>("description", session, true);
      issueSetByDescription = new SortedSetAny<Issue>(descriptionCompare); 
      ComparePriority priorityCompare = new ComparePriority();
      issueSetByPriority = new SortedSetAny<Issue>(priorityCompare);
      CompareByField<Issue> dateTimeCreatedCompare = new CompareByField<Issue>("dateTimeCreated", session, true);
      issueSetByDateTimeCreated = new SortedSetAny<Issue>(dateTimeCreatedCompare);
      CompareByField<Issue> dateTimeUpdatedCompare = new CompareByField<Issue>("dateTimeLastUpdated", session, true);
      issueSetByDateTimeUpdated = new SortedSetAny<Issue>(dateTimeCreatedCompare);
      CompareByField<Issue> compareStatus = new CompareByField<Issue>("status", session, true);
      issueSetByStatus = new SortedSetAny<Issue>(compareStatus);
      CompareSummary compareSummary = new CompareSummary();
      issueSetBySummary = new SortedSetAny<Issue>(compareSummary);
      CompareByField<Issue> compareProject = new CompareByField<Issue>("project", session, true);
      issueSetByProject = new SortedSetAny<Issue>(compareProject);
      CompareCategory compareCategory = new CompareCategory();
      issueSetByCategory = new SortedSetAny<Issue>(compareCategory);
      CompareReportedBy compareReportedBy = new CompareReportedBy();
      issueSetByReportedBy = new SortedSetAny<Issue>(compareReportedBy);
      CompareLastUpdatedBy compareLastUpdatedBy = new CompareLastUpdatedBy();
      issueSetByLastUpdatedBy = new SortedSetAny<Issue>(compareLastUpdatedBy);
      CompareAssignedTo compareAssignedTo = new CompareAssignedTo();
      issueSetByAssignedTo = new SortedSetAny<Issue>(compareAssignedTo);
      CompareByField<Issue> compareByDueDate = new CompareByField<Issue>("dueDate", session, true);
      issueSetByDueDate = new SortedSetAny<Issue>(compareByDueDate);
      CompareByVersion compareByVersion = new CompareByVersion();
      issueSetByVersion = new SortedSetAny<Issue>(compareByVersion);
      componentSet = new SortedSetAny<Component>(capacity);
      userSet = new SortedSetAny<User>(capacity);
      projectSet = new SortedSetAny<Project>(capacity);
      versionSet = new SortedSetAny<ProductVersion>(capacity);
      organizationSet = new SortedSetAny<Organization>(capacity);
      permissions = null;
    }

    public void Add(Issue issue)
    {
      issueSetById.Add(issue); 
      issueSetByDescription.Add(issue);
      issueSetByPriority.Add(issue);
      issueSetByDateTimeCreated.Add(issue);
      issueSetByDateTimeUpdated.Add(issue);
      issueSetByStatus.Add(issue);
      issueSetBySummary.Add(issue);
      issueSetByProject.Add(issue);
      issueSetByCategory.Add(issue);
      issueSetByReportedBy.Add(issue);
      issueSetByLastUpdatedBy.Add(issue);
      issueSetByAssignedTo.Add(issue);
      issueSetByDueDate.Add(issue);
      issueSetByVersion.Add(issue);
    }

   public bool Remove(Issue issue)
    {
      return issueSetById.Remove(issue) &&
             issueSetByDescription.Remove(issue) &&
             issueSetByPriority.Remove(issue) &&
             issueSetByDateTimeCreated.Remove(issue) &&
             issueSetByDateTimeUpdated.Remove(issue) &&
             issueSetByStatus.Remove(issue) &&
             issueSetBySummary.Remove(issue) &&
             issueSetByProject.Remove(issue) &&
             issueSetByCategory.Remove(issue) &&
             issueSetByReportedBy.Remove(issue) &&
             issueSetByLastUpdatedBy.Remove(issue) &&
             issueSetByAssignedTo.Remove(issue) &&
             issueSetByDueDate.Remove(issue) &&
             issueSetByVersion.Remove(issue);
    }

   public SortedSetAny<Issue> IssueSetById
    {
      get
      {
        return issueSetById;
      }
    }

    public SortedSetAny<Issue> IssueSetByAssignedTo
    {
      get
      {
        return issueSetByAssignedTo;
      }
    }

    public SortedSetAny<Issue> IssueSetByCategory
    {
      get
      {
        return issueSetByCategory;
      }
    }

    public SortedSetAny<Issue> IssueSetByDueDate
    {
      get
      {
        return issueSetByDueDate;
      }
    }

    public SortedSetAny<Issue> IssueSetByLastUpdatedBy
    {
      get
      {
        return issueSetByLastUpdatedBy;
      }
    }

    public SortedSetAny<Issue> IssueSetByProject
    {
      get
      {
        return issueSetByProject;
      }
    }

    public SortedSetAny<Issue> IssueSetByDateTimeCreated
    {
      get
      {
        return issueSetByDateTimeCreated;
      }
    }


    public SortedSetAny<Issue> IssueSetByDateTimeUpdated
    {
      get
      {
        return issueSetByDateTimeUpdated;
      }
    }

    public SortedSetAny<Issue> IssueSetByReportedBy
    {
      get
      {
        return issueSetByReportedBy;
      }
    }

    public SortedSetAny<Issue> IssueSetByStatus
    {
      get
      {
        return issueSetByStatus;
      }
    }

    public SortedSetAny<Issue> IssueSetBySummary
    {
      get
      {
        return issueSetBySummary;
      }
    }

    public SortedSetAny<Issue> IssueSetByPriority
    {
      get
      {
        return issueSetByPriority;
      }
    }

    public SortedSetAny<Issue> IssueSetByVersion
    {
      get
      {
        return issueSetByVersion;
      }
    }

    public SortedSetAny<Component> ComponentSet
    {
      get
      {
        return componentSet;
      }
    }

    public PermissionScheme Permissions
    {
      get
      {
        return permissions;
      }
      set
      {
        Update();
        permissions = value;
      }
    }

    public SortedSetAny<Project> ProjectSet
    {
      get
      {
        return projectSet;
      }
    }
    
    public SortedSetAny<User> UserSet
    {
      get
      {
        return userSet;
      }
    }

    public SortedSetAny<ProductVersion> VersionSet
    {
      get
      {
        return versionSet;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }
  }
}
