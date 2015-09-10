using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Tracker
{
  public class Component : OptimizedPersistable
  {
    string name;
    string description;
    Project project;
    User createdBy;

    public Component() { }
    public Component(User user, string componentName, string compopnentDescription, Project componentProject)
    {
      name = componentName;
      description = compopnentDescription;
      project = componentProject;
      createdBy = user;
    }

    public User CreatedBy
    {
      get
      {
        return createdBy;
      }
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

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 16;
      }
    }

    public override string ToString()
    {
      if (project == null)
        return name;
      return project.Name + " - " + name;
    }
  }
}
