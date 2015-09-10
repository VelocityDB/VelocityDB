using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Fielding : OptimizedPersistable
  {	
    string playerID;
    UInt16 yearID;
    UInt16 stint;   
    string teamID;
    string lgID;
    string POS;
    UInt16 G;
    UInt16 GS;
    UInt16 InnOuts;
    UInt16 PO;
    UInt16 A;
    UInt16 E;
    UInt16 DP;
    UInt16 PB;
    UInt16 WP;
    UInt16 SB;
    UInt16 CS;
    UInt16 ZR;

    public Fielding(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"'); 
      yearID = UInt16.Parse(fields[i++]); 
      stint = UInt16.Parse(fields[i++]);       
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      POS = fields[i++].Trim('\"');
      UInt16.TryParse(fields[i++], out G);
      UInt16.TryParse(fields[i++], out GS);
      UInt16.TryParse(fields[i++], out InnOuts);
      UInt16.TryParse(fields[i++], out PO);
      UInt16.TryParse(fields[i++], out A);
      UInt16.TryParse(fields[i++], out E);
      UInt16.TryParse(fields[i++], out DP);
      UInt16.TryParse(fields[i++], out PB);
      UInt16.TryParse(fields[i++], out WP);
      UInt16.TryParse(fields[i++], out SB);
      UInt16.TryParse(fields[i++], out CS);
      UInt16.TryParse(fields[i++], out ZR);
    }
  }
}
