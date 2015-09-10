using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class TeamsHalf : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 95;

    UInt16 yearID;
    string lgID;
    string teamID;
    UInt16 Half;
    string divID;
    string DivWin;
    UInt16 Rank;
    UInt16 G;
    UInt16 W;
    UInt16 L;

    public TeamsHalf(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      yearID = UInt16.Parse(fields[i++]);
      lgID = fields[i++].Trim('\"');
      teamID = fields[i++].Trim('\"');
      Half = UInt16.Parse(fields[i++].Trim('\"'));
      divID = fields[i++].Trim('\"');
      DivWin = fields[i++].Trim('\"');
      Rank = UInt16.Parse(fields[i++]);
      G = UInt16.Parse(fields[i++]);
      W = UInt16.Parse(fields[i++]);
      L = UInt16.Parse(fields[i++]);
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
