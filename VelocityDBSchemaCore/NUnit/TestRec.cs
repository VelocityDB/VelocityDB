using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class TestRec : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 8718;

    object myObj;

    public object Stuff
    {
      get
      {
        return myObj;
      }
      private set
      {
        myObj = value;
      }
    }

    public TestRec(int stuff)
    {
      Stuff = stuff;
    }

    public TestRec(string stuff)
    {
      Stuff = stuff;
    }

    public TestRec(int[] stuff)
    {
      Stuff = stuff;
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }
  }
}
