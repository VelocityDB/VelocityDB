using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class PitchingPost : OptimizedPersistable
  {
    string playerID;
    UInt16 yearID;
    string round;
    string teamID;
    string lgID;
    UInt16 W;
    UInt16 L;
    UInt16 G;
    UInt16 GS;
    UInt16 CG;
    UInt16 SHO;
    UInt16 SV;
    UInt16 IPouts;
    UInt16 H;
    UInt16 ER;
    UInt16 HR;
    UInt16 BB;	
    UInt16 SO;
    float BAOpp;	
    float ERA;	
    UInt16 IBB;	
    UInt16 WP;	
    UInt16 HBP;	
    UInt16 BK;	
    UInt16 BFP;	
    UInt16 GF;	
    UInt16 R;	
    UInt16 SH;
    UInt16 SF;
    UInt16 GIDP;

    public PitchingPost(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"');
      yearID = UInt16.Parse(fields[i++]);
      round = fields[i++].Trim('\"');
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      W = UInt16.Parse(fields[i++]);
      L = UInt16.Parse(fields[i++]);
      G = UInt16.Parse(fields[i++]);
      GS = UInt16.Parse(fields[i++]);
      CG = UInt16.Parse(fields[i++]);
      SHO = UInt16.Parse(fields[i++]);
      SV = UInt16.Parse(fields[i++]);
      IPouts = UInt16.Parse(fields[i++]);
      H = UInt16.Parse(fields[i++]);
      ER = UInt16.Parse(fields[i++]);
      HR = UInt16.Parse(fields[i++]);
      BB = UInt16.Parse(fields[i++]);
      SO = UInt16.Parse(fields[i++]);
      float.TryParse(fields[i++], out BAOpp);
      float.TryParse(fields[i++], out ERA);
      UInt16.TryParse(fields[i++], out IBB);
      UInt16.TryParse(fields[i++], out WP);
      UInt16.TryParse(fields[i++], out HBP);
      UInt16.TryParse(fields[i++], out BK);
      UInt16.TryParse(fields[i++], out BFP);
      UInt16.TryParse(fields[i++], out GF);
      UInt16.TryParse(fields[i++], out R);
      UInt16.TryParse(fields[i++], out SH);
      UInt16.TryParse(fields[i++], out SF);
      UInt16.TryParse(fields[i++], out GIDP);
    }
  }
}