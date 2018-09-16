using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Schools : OptimizedPersistable
  {
    string schoolID;
    string schoolName;
    string schoolCity;
    string schoolState;
    string schoolNick;

    public Schools(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      schoolID = fields[i++].Trim('\"');
      schoolName = fields[i++].Trim('\"');
      schoolCity = fields[i++].Trim('\"');
      schoolState = fields[i++].Trim('\"');
      schoolNick = fields[i++].Trim('\"');
    }
  }
}

