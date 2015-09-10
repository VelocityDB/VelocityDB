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
    DateTime releaseDate;
    User createdBy;
    string name;
    string description;
#pragma warning disable 0414
    State state;
#pragma warning restore 0414
    public enum State : byte { Released, Unreleased, Archived };

    public ProductVersion() { }
    public ProductVersion(User user, string version, string description, DateTime? releaseDate)
    {
      createdBy = user;
      name = version;
      this.description = description;
      this.releaseDate = releaseDate ?? DateTime.MaxValue;
      if (releaseDate != null)
        state = State.Released;
      else
        state = State.Unreleased;
    }

    public User CreatedBy
    {
      get
      {
        return createdBy;
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
    
    public DateTime ReleaseDate
    {
      get
      {
        return releaseDate;
      }
      set
      {
        Update();
        releaseDate = value;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 17;
      }
    }

    public override string ToString()
    {
      return name;
    }
  }
}
