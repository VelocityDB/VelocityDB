using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class AllStarFull : OptimizedPersistable
  {
    string playerID;
    UInt16 yearID;
    UInt16 gameNum;
    string gameID;
    string teamID;
    string lgID;
    UInt16 GP;
    UInt16 startingPos;
    public AllStarFull(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);
      gameNum = UInt16.Parse(fields[i++]);
      gameID = fields[i++].Trim('\"');
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      GP = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out startingPos);
    }
  }
}
