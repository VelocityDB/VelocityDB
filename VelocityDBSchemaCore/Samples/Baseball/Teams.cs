using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Teams : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 93;

    UInt16 yearID;
    string lgID;
    string teamID;
    string franchID;
    string divID;
    UInt16 Rank;
    UInt16 G;
    UInt16 Ghome;
    UInt16 W;
    UInt16 L;
    UInt16 DivWin;
    UInt16 WCWin;
    string LgWin;
    UInt16 WSWin;
    UInt16 R;
    UInt16 AB;
    UInt16 H;
    UInt16 _2B;
    UInt16 _3B;
    UInt16 HR;
    UInt16 BB;
    UInt16 SO;
    UInt16 SB;
    UInt16 CS;
    UInt16 HBP;
    UInt16 SF;
    UInt16 RA;
    UInt16 ER;
    float ERA;
    UInt16 CG;
    UInt16 SHO;
    UInt16 SV;
    UInt16 IPouts;
    UInt16 HA;
    UInt16 HRA;
    UInt16 BBA;
    UInt16 SOA;
    UInt16 E;
    UInt16 DP;
    float FP;
    string name;
    string park;
    UInt32 attendance;
    UInt16 BPF;
    UInt16 PPF;
    string teamIDBR;
    string teamIDlahman45;
    string teamIDretro;

    public Teams(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      yearID = UInt16.Parse(fields[i++]);
      lgID = fields[i++].Trim('\"');
      teamID = fields[i++].Trim('\"');
      franchID = fields[i++].Trim('\"');
      divID = fields[i++].Trim('\"');
      Rank = UInt16.Parse(fields[i++]);
      G = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out Ghome);
      W = UInt16.Parse(fields[i++]);
      L = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out DivWin);
      UInt16.TryParse(fields[i++], out WCWin);
      LgWin = fields[i++].Trim('\"');
      UInt16.TryParse(fields[i++], out WSWin);
      R = UInt16.Parse(fields[i++]);
      AB = UInt16.Parse(fields[i++]);
      H = UInt16.Parse(fields[i++]);
      _2B = UInt16.Parse(fields[i++]);
      _3B = UInt16.Parse(fields[i++]);
      HR = UInt16.Parse(fields[i++]);
      BB = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out SO);
      UInt16.TryParse(fields[i++], out SB);
      UInt16.TryParse(fields[i++], out CS);
      UInt16.TryParse(fields[i++], out HBP);
      UInt16.TryParse(fields[i++], out SF);
      RA = UInt16.Parse(fields[i++]);
      ER = UInt16.Parse(fields[i++]);
      ERA = float.Parse(fields[i++]);
      CG = UInt16.Parse(fields[i++]);
      SHO = UInt16.Parse(fields[i++]);
      SV = UInt16.Parse(fields[i++]);
      IPouts = UInt16.Parse(fields[i++]);
      HA = UInt16.Parse(fields[i++]);
      HRA = UInt16.Parse(fields[i++]);
      BBA = UInt16.Parse(fields[i++]);
      SOA = UInt16.Parse(fields[i++]);
      E = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out DP);
      FP = float.Parse(fields[i++]);
      name = fields[i++].Trim('\"');
      park = fields[i++].Trim('\"');
      UInt32.TryParse(fields[i++], out attendance);
      BPF = UInt16.Parse(fields[i++]);
      PPF = UInt16.Parse(fields[i++]);
      teamIDBR = fields[i++].Trim('\"');
      teamIDlahman45 = fields[i++].Trim('\"');
      teamIDretro = fields[i++].Trim('\"');
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }
  }
}