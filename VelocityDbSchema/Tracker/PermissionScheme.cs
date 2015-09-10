using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Tracker
{
  public class PermissionScheme : OptimizedPersistable
  {
    SortedSetAny<User> adminSet;
    SortedSetAny<User> developerSet;
    UInt32 adminPermissions;
    UInt32 developerPermissions;
    UInt32 userPermissions;
    User superUser;
    public enum Permission
    {
      CreateProject = 1,
      DeleteProject = 4,
      EditProject = 8,   
      CreateComponent = 32,                  
      DeleteComponent = 64,            
      EditComponent = 128,                
      CreateIssue = 256,
      DeleteIssue = 512,
      EditIssue = 1024,
      EditOwnIssue = 2048,
      DeleteOwnIssue = 4096,
      CreateVersion = 8192,
      DeleteVersion = 16384,
      EditVersion = 32768,
      AddLargeAttachment = 65536,
      AddSmallAttachment = 131072,
      DeleteAttachment = 262144,
      EditOwnWorklogs = 0x80000,
      LinkIssues = 0x100000,
      ManageWatcherList = 0x200000,
      ModifyReporter = 0x400000,
      MoveIssues = 0x800000,
      EditUser = 0x1000000,
      ScheduleIssues = 0x2000000,
      DeleteUser = 0x4000000,
      ViewVotersAndWatchers = 0x8000000,
      WorkOnIssues = 0x10000000
    };

    public PermissionScheme(User superUser)
    {
      this.superUser = superUser;
      adminSet = new SortedSetAny<User>();
      adminSet.Add(superUser);
      developerSet = new SortedSetAny<User>();
      adminPermissions = (uint) (Permission.CreateProject | Permission.DeleteProject | Permission.EditProject | Permission.CreateComponent | Permission.DeleteComponent | 
        Permission.EditComponent | Permission.CreateIssue | Permission.DeleteIssue | Permission.EditIssue | Permission.EditOwnIssue | Permission.DeleteOwnIssue | 
        Permission.CreateVersion | Permission.DeleteVersion | Permission.EditVersion | Permission.AddLargeAttachment | Permission.AddSmallAttachment |
        Permission.DeleteAttachment | Permission.LinkIssues | Permission.EditUser | Permission.ScheduleIssues | Permission.DeleteUser | Permission.WorkOnIssues);
      developerPermissions = (uint)(Permission.CreateIssue | Permission.DeleteIssue | Permission.EditIssue | Permission.EditOwnIssue | Permission.DeleteOwnIssue | 
        Permission.EditVersion | Permission.AddLargeAttachment | Permission.AddSmallAttachment |
        Permission.DeleteAttachment | Permission.LinkIssues | Permission.ScheduleIssues | Permission.WorkOnIssues);
      userPermissions = (uint)(Permission.CreateIssue | Permission.EditOwnIssue | Permission.DeleteOwnIssue | Permission.AddSmallAttachment);
    }

    public SortedSetAny<User> AdminSet
    {
      get
      {
        return adminSet;
      }
    }

    public SortedSetAny<User> DeveloperSet
    {
      get
      {
        return developerSet;
      }
    }


    public UInt32 AdminPermissions
    {
      get
      {
        return adminPermissions;
      }
      set
      {
        Update();
        adminPermissions = value;
      }
    }

    public UInt32 DeveloperPermissions
    {
      get
      {
        return developerPermissions;
      }
      set
      {
        Update();
        developerPermissions = value;
      }
    }

    public UInt32 UserPermissions
    {
      get
      {
        return userPermissions;
      }
      set
      {
        Update();
        userPermissions = value;
      }
    }
  }
}

