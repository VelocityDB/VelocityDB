using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class SchoolsPlayers : OptimizedPersistable
  {
    string playerID;
    string schoolID;
    UInt16 yearMin;
    UInt16 yearMax;

    public SchoolsPlayers(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"');
      schoolID = fields[i++].Trim('\"');
      yearMin = UInt16.Parse(fields[i++]);
      yearMax = UInt16.Parse(fields[i++]);
    }
  }
}
