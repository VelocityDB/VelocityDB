using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.OnlineStoreFinder
{
  [Serializable]
  public class TokenComparer<T> : VelocityDbComparer<T> where T: IComparable
  {
    public TokenComparer() { }

    public override int Compare(T a, T b)
    {
      UInt32 aHash = (UInt32) a.GetHashCode();
      UInt32 bHash = (UInt32) b.GetHashCode();
      int value = aHash.CompareTo(bHash);
      if (value != 0)
        return value;
      return a.CompareTo(b);
    }

    public override void SetComparisonArrayFromObject(T key, byte[] comparisonArray, bool oidShort)
    {
      Int32 hashCode = key.GetHashCode();
      Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(hashCode)), 0, comparisonArray, 0, comparisonArray.Length);
    }
  }
}
