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
    string m_name; // used in combination with issue id
    string m_description;
    User m_createdBy;

    public Project() { }
    public Project(User user, string projectName, string projectDescription) 
    {
      m_createdBy = user;
      m_name = projectName;
      m_description = projectDescription;
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

    public User CreatedBy
    {
      get
      {
        return m_createdBy;
      }
    }

    public override string ToString()
    {
      return m_name;
    }
  }
}
