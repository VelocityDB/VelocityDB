using System;

namespace VelocityDBExtensions.Geo.util
{

  public class LongUtil
  {
    public const Int64 FIRST_BIT = unchecked((Int64) 0x8000000000000000);

    public static int CommonPrefixLength(Int64 a, Int64 b)
    {
      int result = 0;
      while (result < 64 && (a & FIRST_BIT) == (b & FIRST_BIT))
      {
        result++;
        a <<= 1;
        b <<= 1;
      }
      return result;
    }
  }
}