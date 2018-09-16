using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Batting : OptimizedPersistable
  {
    string playerID;
    UInt16 yearID;
    UInt16 stint;
    string teamID;
    string lgID;
    UInt16 G;
    UInt16 G_batting;
    UInt16 AB;
    UInt16 R;
    UInt16 H;
    UInt16 _2B;
    UInt16 _3B;
    UInt16 HR;
    UInt16 RBI;
    UInt16 SB;
    UInt16 CS;
    UInt16 BB;
    UInt16 SO;
    UInt16 IBB;
    UInt16 HBP;
    UInt16 SH;
    UInt16 SF;
    UInt16 GIDP;
    UInt16 G_old;

    public Batting(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);   
      stint = UInt16.Parse(fields[i++]);  
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      UInt16.TryParse(fields[i++], out G);
      UInt16.TryParse(fields[i++], out G_batting);
      UInt16.TryParse(fields[i++], out AB);
      UInt16.TryParse(fields[i++], out R);
      UInt16.TryParse(fields[i++], out H);
      UInt16.TryParse(fields[i++], out _2B);
      UInt16.TryParse(fields[i++], out _3B);
      UInt16.TryParse(fields[i++], out HR);
      UInt16.TryParse(fields[i++], out RBI);
      UInt16.TryParse(fields[i++], out SB);
      UInt16.TryParse(fields[i++], out CS);
      UInt16.TryParse(fields[i++], out BB);
      UInt16.TryParse(fields[i++], out SO);
      UInt16.TryParse(fields[i++], out IBB);
      UInt16.TryParse(fields[i++], out HBP);
      UInt16.TryParse(fields[i++], out SH);
      UInt16.TryParse(fields[i++], out SF);
      UInt16.TryParse(fields[i++], out GIDP);
      UInt16.TryParse(fields[i++], out G_old);
    }

    public string PlayerID
    {
      get
      {
        return playerID;
      }
    }
  }
}
