using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class AllStar : OptimizedPersistable
  {
    string playerID;
    UInt16 yearId;
    string lgID;

    public AllStar(string line)
    {
      string[] fields = line.Split(',');
      playerID = fields[0].Trim('\"');
      yearId = UInt16.Parse(fields[1]);
      lgID = fields[1].Trim('\"');
    }
  }
}
