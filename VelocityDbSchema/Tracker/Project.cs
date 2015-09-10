using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Tracker
{
  public class Project : OptimizedPersistable
  {
    string name; // used in combination with issue id
    string description;
    User createdBy;

    public Project() { }
    public Project(User user, string projectName, string projectDescription) 
    {
      createdBy = user;
      name = projectName;
      description = projectDescription;
    }

    public string Name
    {
      get
      {
        return name;
      }
      set
      {
        Update();
        name = value;
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

    public User CreatedBy
    {
      get
      {
        return createdBy;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 18;
      }
    }

    public override string ToString()
    {
      return name;
    }
  }
}
