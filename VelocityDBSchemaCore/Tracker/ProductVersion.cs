using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Tracker
{
  public class ProductVersion : OptimizedPersistable
  {
    DateTime m_releaseDate;
    User m_createdBy;
    string m_name;
    string m_description;
#pragma warning disable 0414
    State m_state;
#pragma warning restore 0414
    public enum State : byte { Released, Unreleased, Archived };

    public ProductVersion() { }
    public ProductVersion(User user, string version, string description, DateTime? releaseDate)
    {
      m_createdBy = user;
      m_name = version;
      m_description = description;
      m_releaseDate = releaseDate ?? DateTime.MaxValue;
      if (releaseDate != null)
        m_state = State.Released;
      else
        m_state = State.Unreleased;
    }

    public User CreatedBy
    {
      get
      {
        return m_createdBy;
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
    
    public DateTime ReleaseDate
    {
      get
      {
        return m_releaseDate;
      }
      set
      {
        Update();
        m_releaseDate = value;
      }
    }

    public override string ToString()
    {
      return m_name;
    }
  }
}
