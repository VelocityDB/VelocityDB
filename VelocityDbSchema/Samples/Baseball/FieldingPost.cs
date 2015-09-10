using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class FieldingPost : OptimizedPersistable
  {
    string playerID;
    UInt16 yearID;
    string teamID;
    string lgID;
    string round;
    string POS;
    UInt16 G;   
    UInt16 GS;
    UInt16 InnOuts;
    UInt16 PO;
    UInt16 A;
    UInt16 E;
    UInt16 DP;
    UInt16 TP;
    UInt16 PB;
    UInt16 SB;
    UInt16 CS;

    public FieldingPost(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      playerID = fields[i++].Trim('\"'); 
      yearID = UInt16.Parse(fields[i++]);
      teamID = fields[i++].Trim('\"');
      lgID = fields[i++].Trim('\"');
      round = fields[i++].Trim('\"');
      POS = fields[i++].Trim('\"');
      G = UInt16.Parse(fields[i++]);
      GS = UInt16.Parse(fields[i++]);
      InnOuts = UInt16.Parse(fields[i++]);
      PO = UInt16.Parse(fields[i++]);
      A = UInt16.Parse(fields[i++]);
      E = UInt16.Parse(fields[i++]);
      DP = UInt16.Parse(fields[i++]);
      TP = UInt16.Parse(fields[i++]);
      UInt16.TryParse(fields[i++], out PB);
      UInt16.TryParse(fields[i++], out SB);
      UInt16.TryParse(fields[i++], out CS);
    }
  }
}
