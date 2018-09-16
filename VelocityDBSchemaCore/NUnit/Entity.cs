using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class Entity : OptimizedPersistable, IEquatable<Entity>, IEqualityComparer<Entity>
  {
    public int idNumber;

    public int UserID
    {
      get;
      set;
    }

    public string FirstName
    {
      get;
      set;
    }

    public string LastName
    {
      get;
      set;
    }

    public string MiddleName
    {
      get;
      set;
    }


    public bool Equals(Entity other)
    {
      if (idNumber == other.idNumber) return true;
      else return false;
    }

    public bool Equals(Entity x, Entity y)
    {
      if (x.idNumber == y.idNumber) return true;
      else return false;
    }

    public int GetHashCode(Entity obj)
    {
      int hCode = obj.idNumber;
      return hCode.GetHashCode();
    }
  }
}
