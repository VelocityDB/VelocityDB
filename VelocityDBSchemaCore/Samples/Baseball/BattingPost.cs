using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class BattingPost : OptimizedPersistable
  {
    UInt16 yearID;
    string round;
    string playerID;    
    string teamID;
    string lgID;
    UInt16 G;
    //UInt16 G_batting;
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

    public BattingPost(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      yearID = UInt16.Parse(fields[i++]); 
      round = fields[i++].Trim('\"');
      playerID = fields[i++].Trim('\"');  
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');  
      G = UInt16.Parse(fields[i++]); 
      AB = UInt16.Parse(fields[i++]);
      R = UInt16.Parse(fields[i++]);
      H = UInt16.Parse(fields[i++]);
      _2B = UInt16.Parse(fields[i++]);
      _3B = UInt16.Parse(fields[i++]);
      HR = UInt16.Parse(fields[i++]);
      RBI = UInt16.Parse(fields[i++]);
      SB = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out CS);
      UInt16.TryParse(fields[i++], out BB);
      UInt16.TryParse(fields[i++], out SO);
      UInt16.TryParse(fields[i++], out IBB);
      UInt16.TryParse(fields[i++], out HBP);
      UInt16.TryParse(fields[i++], out SH);
      UInt16.TryParse(fields[i++], out SF);
      UInt16.TryParse(fields[i++], out GIDP);
    }
  }
}

