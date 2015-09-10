using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class AwardsShareManagers : OptimizedPersistable
  {
    string awardID;
    UInt16 yearID;
    string lgID;
    string managerID;
    UInt16 pointsWon;
    UInt16 pointsMax;
    UInt16 votesFirst;

    public AwardsShareManagers(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      awardID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);   
      lgID = fields[i++].Trim('\"'); 
      managerID = fields[i++].Trim('\"');
      pointsWon = UInt16.Parse(fields[i++]); 
      pointsMax = UInt16.Parse(fields[i++]); 
      votesFirst = UInt16.Parse(fields[i++]); 
    }
  }
}
