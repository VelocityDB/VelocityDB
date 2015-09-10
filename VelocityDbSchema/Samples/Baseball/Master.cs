using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Baseball
{
  public class Master : OptimizedPersistable
  {
    enum LeftOrRight : byte { Unknown, Left, Right };
    UInt16 lahmanID;
    string playerID;
    string managerID;
    string hofID;
    UInt16 birthYear;
    UInt16 birthMonth;
    UInt16 birthDay;
    string birthCountry;
    string birthState;
    string birthCity;
    UInt16 deathYear;
    UInt16 deathMonth;
    UInt16 deathDay;
    string deathCountry;
    string deathState;
    string deathCity;
    string nameFirst;
    string nameLast;
    string nameNote;
    string nameGiven;
    string[] nameNick;
    UInt16 weight;
    float height;
    LeftOrRight bats;
    LeftOrRight throws;
    DateTime debut; 
    DateTime finalGame;
    string college;
    string lahman40ID;
    string lahman45ID;
    string retroID;
    string holtzID;
    string bbrefID;

    public Master(string line)
    {
      string[] fields = line.Split(',');
      int i = 0;
      lahmanID = UInt16.Parse(fields[i++]);
      playerID = fields[i++].Trim('\"');
      managerID = fields[i++].Trim('\"');
      hofID = fields[i++].Trim('\"');
      UInt16.TryParse(fields[i++], out birthYear);
      UInt16.TryParse(fields[i++], out birthMonth);
      UInt16.TryParse(fields[i++], out birthDay);
      birthCountry = fields[i++].Trim('\"');
      birthState = fields[i++].Trim('\"');
      birthCity = fields[i++].Trim('\"');
      UInt16.TryParse(fields[i++], out deathYear);
      UInt16.TryParse(fields[i++], out deathMonth);
      UInt16.TryParse(fields[i++], out deathDay);
      deathCountry = fields[i++].Trim('\"');
      deathState = fields[i++].Trim('\"');
      deathCity = fields[i++].Trim('\"');
      nameFirst = fields[i++].Trim('\"');
      nameLast = fields[i++].Trim('\"');
      nameNote = fields[i++].Trim('\"');
      nameGiven = fields[i++].Trim('\"');
      nameNick = new string[1];
      nameNick[0] = fields[i++].Trim('\"');
      while (fields[i].Length > 0 && UInt16.TryParse(fields[i].Trim('\"'), out weight) == false)
      {
        Array.Resize(ref nameNick, nameNick.Length + 1);
        nameNick[nameNick.Length - 1] = fields[i++].Trim('\"');
      }
      i++;
      float.TryParse(fields[i++], out height);
      if (fields[i].Length > 0)
        bats = fields[i++] == "L" ? LeftOrRight.Left : LeftOrRight.Right;
      else
      {
        i++;
        bats = LeftOrRight.Unknown;
      }
      if (fields[i].Length > 0)
        throws = fields[i++] == "L" ? LeftOrRight.Left : LeftOrRight.Right;
      else
      {
        i++;
        throws = LeftOrRight.Unknown;
      }
      DateTime.TryParse(fields[i++], out debut);
      DateTime.TryParse(fields[i++], out finalGame);
      college = fields[i++].Trim('\"');
      lahman40ID = fields[i++].Trim('\"');
      lahman45ID = fields[i++].Trim('\"');
      retroID = fields[i++].Trim('\"');
      holtzID = fields[i++].Trim('\"');
      bbrefID = fields[i++].Trim('\"');
    }
  }
}


