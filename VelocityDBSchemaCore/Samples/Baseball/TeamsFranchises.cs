using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class TeamsFranchises : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 94;

    string franchID;
    string franchName;
    string active;
    string NAassoc;

    public TeamsFranchises(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      franchID = fields[i++].Trim('\"');
      franchName = fields[i++].Trim('\"');
      active = fields[i++].Trim('\"');
      NAassoc = fields[i++].Trim('\"');
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
