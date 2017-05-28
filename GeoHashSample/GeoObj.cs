using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.Comparer;
using VelocityDb.TypeInfo;
using VelocityDBExtensions.geohash;

namespace GeoHashSample
{
  public class GeoObj : OptimizedPersistable
  {
    public static Random s_randGen = new Random(5);
    double m_longitude;
    double m_latitude;
    long m_geoHash;

    public GeoObj()
    {
      m_longitude = (s_randGen.Next(360) - 180) * s_randGen.NextDouble();
      m_latitude = (s_randGen.Next(180) - 90) * s_randGen.NextDouble();
      m_geoHash = GeoHash.WithBitPrecision(m_latitude, m_longitude).LongValue;
    }

    public GeoObj(long geoHash)
    {
      m_geoHash = geoHash;
    }

    public double Latitude
    {
      get
      {
        return m_latitude;
      }
      set
      {
        Update();
        m_latitude = value;
      }
    }

    public double Longitude
    {
      get
      {
        return m_longitude;
      }
      set
      {
        Update();
        m_longitude = value;
      }
    }

    public long GeoHashAsLong
    {
      get
      {
        return m_geoHash;
      }
      set
      {
        Update();
        m_geoHash = value;
      }
    }
  }

  public class CompareGeoObj : VelocityDbComparer<GeoObj>
  {
    public override int Compare(GeoObj a, GeoObj b)
    {
      return a.GeoHashAsLong.CompareTo(b.GeoHashAsLong);
    }

    public override void SetComparisonArrayFromObject(GeoObj key, byte[] comparisonArray, bool oidShort)
    {
      Int64 val = key.GeoHashAsLong;
      UInt64 uval = (UInt64)val;
      if (val < 0L)
        uval &= 0x7FFFFFFFFFFFFFFF;
      else
        uval |= 0x8000000000000000;
      byte [] fieldBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int64)uval));
      Buffer.BlockCopy(fieldBytes, 0, comparisonArray, 0, CommonTypes.s_sizeOfInt64);
    }
  }
}
