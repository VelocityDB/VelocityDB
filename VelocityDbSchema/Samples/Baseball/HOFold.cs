using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class HOFold : OptimizedPersistable
  {
    string hofID;
    UInt16 yearID;
    string votedBy;
    UInt16 ballots;
    UInt16 votes;
    bool inducted;
    string category;

    public HOFold(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      hofID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);
      votedBy = fields[i++].Trim('\"');
      UInt16.TryParse(fields[i++], out ballots);
      UInt16.TryParse(fields[i++], out votes);
      inducted = fields[i++] == "Y";
      category = fields[i++].Trim('\"');
    }
  }
}

