using System;

/*
 * Ported to C# from Java by Mats Persson, VelocityDB, Inc.
 *
 * original Java code:
 * Copyright 2010, Silvio Heuberger @ IFS www.ifs.hsr.ch
 *
 * This code is release under the Apache License 2.0.
 * You should have received a copy of the license
 * in the LICENSE file. If you have not, see
 * http://www.apache.org/licenses/LICENSE-2.0
 */
namespace VelocityDBExtensions.Geo.util
{

  public class GeoHashSizeTable
  {
    static readonly int s_numBits = 64;
    private static readonly double[] s_dLat = new double[s_numBits];
    private static readonly double[] s_dLon = new double[s_numBits];

    static GeoHashSizeTable()
    {
      for (int i = 0; i < s_numBits; i++)
      {
        s_dLat[i] = DLat(i);
        s_dLon[i] = DLon(i);
      }
    }

    protected internal static double DLat(int bits)
    {
      return 180d / Math.Pow(2, bits / 2);
    }

    protected internal static double DLon(int bits)
    {
      return 360d / Math.Pow(2, (bits + 1) / 2);
    }

    public static int NumberOfBitsForOverlappingGeoHash(BoundingBox boundingBox)
    {
      int bits = 63;
      double height = boundingBox.LatitudeSize;
      double width = boundingBox.LongitudeSize;
      while ((s_dLat[bits] < height || s_dLon[bits] < width) && bits > 0)
      {
        bits--;
      }
      return bits;
    }
  }

}