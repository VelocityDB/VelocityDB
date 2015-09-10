using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Appearances : OptimizedPersistable
  {
    UInt16 yearID;
    string teamID;
    string lgID;
    string playerID;
    UInt16 G_all;
    UInt16 G_batting;
    UInt16 G_defense;
    UInt16 G_p;
    UInt16 G_c;
    UInt16 G_1b;
    UInt16 G_2b;
    UInt16 G_3b;
    UInt16 G_ss;
    UInt16 G_lf;
    UInt16 G_cf;
    UInt16 G_rf;
    UInt16 G_of;
    UInt16 G_dh;
    UInt16 G_ph;
    UInt16 G_pr;

    public Appearances(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;    
      yearID = UInt16.Parse(fields[i++]);
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      playerID = fields[i++].Trim('\"');
      G_all = UInt16.Parse(fields[i++]);
      G_batting = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out G_defense);
      G_p = UInt16.Parse(fields[i++]);
      G_c = UInt16.Parse(fields[i++]);
      G_1b = UInt16.Parse(fields[i++]);
      G_2b = UInt16.Parse(fields[i++]);
      G_3b = UInt16.Parse(fields[i++]);
      G_ss = UInt16.Parse(fields[i++]);
      G_lf = UInt16.Parse(fields[i++]);
      G_cf = UInt16.Parse(fields[i++]);
      G_rf = UInt16.Parse(fields[i++]);
      G_of = UInt16.Parse(fields[i++]);
      G_dh = UInt16.Parse(fields[i++]);
      G_ph = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out G_pr);
    }
  }
}

