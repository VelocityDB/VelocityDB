using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class AwardsSharePlayers : OptimizedPersistable
  {
    string awardID;
    UInt16 yearID;
    string lgID;
    string playerID;
    float pointsWon;
    UInt16 pointsMax;
    float votesFirst;

    public AwardsSharePlayers(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      awardID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);   
      lgID = fields[i++].Trim('\"');
      playerID = fields[i++].Trim('\"');
      pointsWon = float.Parse(fields[i++]);
      pointsMax = UInt16.Parse(fields[i++]); 
      float.TryParse(fields[i++], out votesFirst); 
    }
  }
}
