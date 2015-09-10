using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Salaries : OptimizedPersistable
  {
    UInt16 yearID;
    string teamID;        
    string lgID;
    string playerID;
    double salary;

    public Salaries(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      yearID = UInt16.Parse(fields[i++]);
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      playerID = fields[i++].Trim('\"');
      salary = double.Parse(fields[i++]);
    }
  }
}
