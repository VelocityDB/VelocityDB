using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Tracker
{
  public class User : OptimizedPersistable
  {
#pragma warning disable 0169
    Organization m_company;
#pragma warning restore 0169
    DateTime m_dateTimeCreated;
    User m_createdBy;
    string m_email;
    string m_firstName;
    string m_lastName;
    string m_userName;

    public User() 
    {     
      m_createdBy = this;
    }

    public User(string email)
    {
      this.m_email = email;
    }

    public User(User createdBy, string email, string firstName, string lastName, string userName)
    {
      if (createdBy == null)
        m_createdBy = this;
      else
        m_createdBy = createdBy;
      m_email = email;
      m_firstName = firstName;
      m_lastName = lastName;
      m_userName = userName;
      m_dateTimeCreated = DateTime.Now;
    }

    public override int CompareTo(object obj)
    {
      if (obj is User)
      {
        User otherUser = (User)obj;
        return this.m_email.CompareTo(otherUser.m_email);
      }
      else
      {
        throw new ArgumentException("object is not a User");
      }
    }

    public string CreatedBy
    {
      get
      {
        return m_createdBy.ToString();
      }
    }
    
    public DateTime DateTimeCreated
    {
      get
      {
        return m_dateTimeCreated;
      } 
    }
    
    public string Email
    {
      get
      {
        return m_email;
      }
      set
      {
        Update();
        m_email = value;
      }
    }
    
    public string FirstName
    {
      get
      {
        return m_firstName;
      }
      set
      {
        Update();
        m_firstName = value;
      }
    }

    public string LastName
    {
      get
      {
        return m_lastName;
      }
      set
      {
        Update();
        m_lastName = value;
      }    
    }
    
    public string UserName
    {
      get
      {
        return m_userName;
      }
      set
      {
        Update();
        m_userName = value;
      }
    }

    public override string ToString()
    {
      return m_userName;
    }
  }
}
