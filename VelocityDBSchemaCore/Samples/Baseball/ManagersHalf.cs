using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class ManagersHalf : OptimizedPersistable
  {
    string managerID;
    UInt16 yearID;
    string teamID;
    string lgID;
    UInt16 inseason;
    UInt16 half;
    UInt16 G;
    UInt16 W;
    UInt16 L;
    UInt16 rank;

    public ManagersHalf(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      managerID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      inseason = UInt16.Parse(fields[i++]);
      half = UInt16.Parse(fields[i++]);
      G = UInt16.Parse(fields[i++]); 
      W = UInt16.Parse(fields[i++]);
      L = UInt16.Parse(fields[i++]);
      rank = UInt16.Parse(fields[i++]);
    }
  }
}

