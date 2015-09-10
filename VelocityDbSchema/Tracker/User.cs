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
    Organization company;
#pragma warning restore 0169
    DateTime dateTimeCreated;
    User createdBy;
    string email;
    string firstName;
    string lastName;
    string userName;

    public User() 
    {     
      createdBy = this;
    }

    public User(string email)
    {
      this.email = email;
    }

    public User(User createdBy, string email, string firstName, string lastName, string userName)
    {
      if (createdBy == null)
        this.createdBy = this;
      else
        this.createdBy = createdBy;
      this.email = email;
      this.firstName = firstName;
      this.lastName = lastName;
      this.userName = userName;
      dateTimeCreated = DateTime.Now;
    }

    public override int CompareTo(object obj)
    {
      if (obj is User)
      {
        User otherUser = (User)obj;
        return this.email.CompareTo(otherUser.email);
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
        return createdBy.ToString();
      }
    }
    
    public DateTime DateTimeCreated
    {
      get
      {
        return dateTimeCreated;
      } 
    }
    
    public string Email
    {
      get
      {
        return email;
      }
      set
      {
        Update();
        email = value;
      }
    }
    
    public string FirstName
    {
      get
      {
        return firstName;
      }
      set
      {
        Update();
        firstName = value;
      }
    }

    public string LastName
    {
      get
      {
        return lastName;
      }
      set
      {
        Update();
        lastName = value;
      }    
    }
    
    public string UserName
    {
      get
      {
        return userName;
      }
      set
      {
        Update();
        userName = value;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 14;
      }
    }

    public override string ToString()
    {
      return userName;
    }
  }
}
