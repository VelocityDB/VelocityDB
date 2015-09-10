using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDbSchema.NUnit
{
  [Index("name,name2")]
  [UniqueConstraint]
  [OnePerDatabase]
  public class RDerby1 : OptimizedPersistable
  {
    [Index]
    [UniqueConstraint]
    string name;

    [Index]
    [UniqueConstraint]
    string name2;
    string added;


    [FieldAccessor("name")]
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

    [FieldAccessor("name2")]
    public string Name2
    {
      get
      {
        return name2;
      }
      set
      {
        Update();
        name2 = value;
      }
    }

    public string Added
    {
      get
      {
        return added;
      }
      set
      {
        Update();
        name2 = added;
      }
    }

    public RDerby1(string name, string name2, string added)
    {
      Name = name;
      this.name2 = name2;
      this.added = added;
    }
  }
}
