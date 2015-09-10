using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema
{
  public class PersonComparer : VelocityDbComparer<Person>
  {
    public override int Compare(Person aP, Person bP)
    {
      int i;
      byte[] strBytes = SessionBase.TextEncoding.GetBytes(aP.lastName);
      byte[] strBytes2 = SessionBase.TextEncoding.GetBytes(bP.lastName);
      for (i = 0; i < strBytes.Length; i++)
      {
        if (i == strBytes2.Length)
          return 1;
        int value = strBytes[i] - strBytes2[i];
        if (value < 0)
          return -1;
        if (value > 0)
          return 1;
      }
      if (strBytes2.Length > strBytes.Length)
        return -1;
      strBytes = SessionBase.TextEncoding.GetBytes(aP.firstName);
      strBytes2 = SessionBase.TextEncoding.GetBytes(bP.firstName);
      for (i = 0; i < strBytes.Length; i++)
      {
        if (i == strBytes2.Length)
          return 1;
        int value = strBytes[i] - strBytes2[i];
        if (value < 0)
          return -1;
        if (value > 0)
          return 1;
      }
      if (strBytes2.Length > strBytes.Length)
        return -1;
      return (int) (aP.Count - bP.Count);
    }

    public override void SetComparisonArrayFromObject(Person aP, byte[] comparisonArray, bool oidShort)
    {
      byte[] strBytes = SessionBase.TextEncoding.GetBytes(aP.lastName);
      Array.Clear(comparisonArray, 0, comparisonArray.Length);
      if (strBytes.Length <= comparisonArray.Length)
        Buffer.BlockCopy(strBytes, 0, comparisonArray, 0, strBytes.Length);
      else
        Buffer.BlockCopy(strBytes, 0, comparisonArray, 0, comparisonArray.Length);
    }
  }
}
