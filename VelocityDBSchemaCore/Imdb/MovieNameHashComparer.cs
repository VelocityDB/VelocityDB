using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb.Collection.Comparer;
using System.Reflection;

namespace VelocityDbSchema.Imdb
{
  [Obfuscation(Feature = "renaming", Exclude = true)]
  [Serializable]
  public class MovieNameHashComparer : VelocityDbComparer<Movie>
  {
    public override int Compare(Movie a, Movie b)
    {
      UInt32 aHash = (UInt32)a.Title.GetHashCode();
      UInt32 bHash = (UInt32)b.Title.GetHashCode();
      int value = aHash.CompareTo(bHash);
      if (value != 0)
        return value;
      return a.Title.CompareTo(b.Title);
    }

    public override void SetComparisonArrayFromObject(Movie key, byte[] comparisonArray, bool oidShort)
    {
      Int32 hashCode = key.Title.GetHashCode();
      Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(hashCode)), 0, comparisonArray, 0, comparisonArray.Length);
    }
  }
}
