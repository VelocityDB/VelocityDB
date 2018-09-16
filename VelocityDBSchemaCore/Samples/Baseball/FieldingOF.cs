using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class FieldingOF : OptimizedPersistable
  {
    string playerID;
    UInt16 yearID;
    UInt16 stint;   
    UInt16 Glf;
    UInt16 Gcf;
    UInt16 Grf;

    public FieldingOF(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"'); 
      yearID = UInt16.Parse(fields[i++]); 
      stint = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out Glf);
      UInt16.TryParse(fields[i++], out Gcf);
      UInt16.TryParse(fields[i++], out Grf);
    }
  }
}

