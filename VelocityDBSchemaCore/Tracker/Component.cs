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
    string m_name;
    string m_description;
    Project m_project;
    User m_createdBy;

    public Component() { }
    public Component(User user, string componentName, string compopnentDescription, Project componentProject)
    {
      m_name = componentName;
      m_description = compopnentDescription;
      m_project = componentProject;
      m_createdBy = user;
    }

    public User CreatedBy
    {
      get
      {
        return m_createdBy;
      }
    }
    
    public string Name
    {
      get
      {
        return m_name;
      }
      set
      {
        Update();
        m_name = value;
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

    public override string ToString()
    {
      if (m_project == null)
        return m_name;
      return m_project.Name + " - " + m_name;
    }
  }
}
