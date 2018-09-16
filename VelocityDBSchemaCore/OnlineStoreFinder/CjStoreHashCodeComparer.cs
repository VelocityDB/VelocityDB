using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.OnlineStoreFinder
{
  [Serializable]
  public class CjStoreHashCodeComparer : VelocityDbComparer<StoreBase>
  {
      public override int Compare(StoreBase a, StoreBase b)
    {
      UInt32 aHash = (UInt32) a.StoreName.GetHashCode();
      UInt32 bHash = (UInt32) b.StoreName.GetHashCode();
      int value = aHash.CompareTo(bHash);
      if (value != 0)
        return value;
      return a.StoreName.CompareTo(b.StoreName);
    }

      public override void SetComparisonArrayFromObject(StoreBase key, byte[] comparisonArray, bool oidShort)
    {
      Int32 hashCode = key.StoreName.GetHashCode();
      Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(hashCode)), 0, comparisonArray, 0, comparisonArray.Length);
    }
  }
}

