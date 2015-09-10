using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.Samples.DatabaseLocking
{
  public class Number : OptimizedPersistable
  {
    int myInt;

    public Number(int anInt = 1)
    {
      myInt = anInt;
    }

    public int MyInt
    {
      get
      {
        return myInt;
      }
      set
      {
        Update();
        myInt = value;
      }
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override string ToString()
    {
      return Oid.ToString() + " myInt: " + myInt;
    }
  }
}
