using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class SeriesPost : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 92;

    UInt16 yearID;
    string round;
    string teamIDwinner;
    string lgIDwinner;
    string teamIDloser;
    string lgIDloser;
    UInt16 wins;
    UInt16 losses;
    UInt16 ties;

    public SeriesPost(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      yearID = UInt16.Parse(fields[i++]);
      round = fields[i++].Trim('\"');
      teamIDwinner = fields[i++].Trim('\"');
      lgIDwinner = fields[i++].Trim('\"');
      teamIDloser = fields[i++].Trim('\"');
      lgIDloser = fields[i++].Trim('\"');
      wins = UInt16.Parse(fields[i++]);
      losses = UInt16.Parse(fields[i++]);
      ties = UInt16.Parse(fields[i++]);
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

