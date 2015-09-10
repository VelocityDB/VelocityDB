using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class AwardsManagers : OptimizedPersistable
  {
    string managerID;
    string awardID;
    UInt16 yearID;
    string lgID;
    bool tie;
    string notes;

    public AwardsManagers(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      managerID = fields[i++].Trim('\"');
      awardID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);     
      lgID = fields[i++].Trim('\"');
      tie = fields[i++] == "Y";
      notes = fields[i++].Trim('\"');     
    }
  }
}
