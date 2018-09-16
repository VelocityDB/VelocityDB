using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Xref_Stats : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 96;

    string playerID;
    UInt32 statsID;
    string bisID;

    public Xref_Stats(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"');
      statsID = UInt32.Parse(fields[i++]);     
      bisID = fields[i++].Trim('\"');
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
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

