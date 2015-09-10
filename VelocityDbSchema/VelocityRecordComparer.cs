using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema
{
  public class VelocityRecordComparer : VelocityDbComparer<VelocityRecord>
  {
  }

  public class VelocityKeyValueComparer : VelocityDbComparer<VelocityKeyValue>
  {
    /*public override int Compare(VelocityRecord aRecord, VelocityRecord bRecord)
    {
      return aRecord.Key.CompareTo(bRecord.Key);
    }*/

    public override void SetComparisonArrayFromObject(VelocityKeyValue record, byte[] comparisonArray, bool oidShort)
    {
      Buffer.BlockCopy(record.Key, 0, comparisonArray, 0, record.Key.Length);
    }
  }
}
