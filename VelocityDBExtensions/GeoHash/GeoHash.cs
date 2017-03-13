using System;
using System.Collections.Generic;
using System.Text;
using VelocityDb;
using VelocityDb.Exceptions;

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
namespace VelocityDBExtensions.geohash
{
  /// <summary>
  /// See https://en.wikipedia.org/wiki/Geohash
  /// </summary>
  [Serializable]
  public sealed class GeoHash : IComparable<GeoHash>
  {
    private const int MAX_BIT_PRECISION = 64;
    private const int MAX_CHARACTER_PRECISION = 12;

    private const long SerialVersionUID = -8553214249630252175L;
    private static readonly int[] s_bits = new int[] {16, 8, 4, 2, 1};
    private const int BASE32_BITS = 5;
    const Int64 FIRST_BIT_FLAGGED = unchecked((long) 0x8000000000000000L);
    private static readonly char[] s_base32 = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};

    private static readonly IDictionary<char?, int?> DecodeMap = new Dictionary<char?, int?>();

    static GeoHash()
    {
      int sz = s_base32.Length;
      for (int i = 0; i < sz; i++)
      {
        DecodeMap[s_base32[i]] = i;
      }
    }

    internal Int64 m_bits = 0;
    private WGS84Point m_point;

    private BoundingBox m_boundingBox;

    internal sbyte m_significantBits = 0;

    internal GeoHash()
    {
    }

    /// <summary>
    /// This method uses the given number of characters as the desired precision
    /// value. The hash can only be 64bits long, thus a maximum precision of 12
    /// characters can be achieved.
    /// </summary>
    public static GeoHash WithCharacterPrecision(double latitude, double longitude, int numberOfCharacters)
    {
      if (numberOfCharacters > MAX_CHARACTER_PRECISION)
      {
        throw new System.ArgumentException($"A geohash can only be {MAX_CHARACTER_PRECISION} character long.");
      }
      int desiredPrecision = (numberOfCharacters * 5 <= 60) ? numberOfCharacters * 5 : 60;
      return new GeoHash(latitude, longitude, desiredPrecision);
    }

    /// <summary>
    /// create a new <seealso cref="GeoHash"/> with the given number of bits accuracy. This
    /// at the same time defines this hash's bounding box.
    /// </summary>
    public static GeoHash WithBitPrecision(double latitude, double longitude, int numberOfBits = MAX_BIT_PRECISION)
    {
      if (numberOfBits > MAX_BIT_PRECISION)
      {
        throw new System.ArgumentException($"A Geohash can only be {MAX_BIT_PRECISION} bits long!");
      }
      if (Math.Abs(latitude) > 90.0 || Math.Abs(longitude) > 180.0)
      {
        throw new System.ArgumentException("Can't have lat/lon values out of (-90,90)/(-180/180)");
      }
      return new GeoHash(latitude, longitude, numberOfBits);
    }

    /// <summary>
    /// create a new <seealso cref="GeoHash"/> with the given number of bits accuracy. This
    /// at the same time defines this hash's bounding box.
    /// </summary>
    /// <param name="point">Location to create <seealso cref="GeoHash"/> for</param>
    /// <param name="numberOfBits">How may bits precision to use (64 is recommended)</param>
    /// <returns></returns>
    public static GeoHash withBitPrecision(WGS84Point point, int numberOfBits)
    {
      return WithBitPrecision(point.Latitude, point.Longitude, numberOfBits);
    }

    /// <summary>
    /// Recreates a <see cref="GeoHash"/> from a string of 0's and 1's
    /// </summary>
    /// <param name="binaryString"><see cref="string"/> of 0's and 1's</param>
    /// <returns>A <see cref="GeoHash"/></returns>
    public static GeoHash FromBinaryString(string binaryString)
    {
      GeoHash geohash = new GeoHash();
      for (int i = 0; i < binaryString.Length; i++)
      {
        if (binaryString[i] == '1')
        {
          geohash.AddOnBitToEnd();
        }
        else if (binaryString[i] == '0')
        {
          geohash.AddOffBitToEnd();
        }
        else
        {
          throw new System.ArgumentException(binaryString + " is not a valid geohash as a binary string");
        }
      }
      geohash.m_bits <<= (MAX_BIT_PRECISION - geohash.m_significantBits);
      Int64[] latitudeBits = geohash.RightAlignedLatitudeBits;
      Int64[] longitudeBits = geohash.RightAlignedLongitudeBits;
      return geohash.RecombineLatLonBitsToHash(latitudeBits, longitudeBits);
    }

    /// <summary>
    /// build a new <seealso cref="GeoHash"/> from a base32-encoded <seealso cref="String"/>.
    /// This will also set up the hashes bounding box and other values, so it can
    /// also be used with functions like within().
    /// </summary>
    /// <param name="geohash">base32-encoded <see cref="String"/></param>
    /// <returns>A <see cref="GeoHash"/></returns>
    public static GeoHash FromGeohashString(string geohash)
    {
      double[] latitudeRange = new double[] {-90.0, 90.0};
      double[] longitudeRange = new double[] {-180.0, 180.0};

      bool isEvenBit = true;
      GeoHash hash = new GeoHash();

      for (int i = 0; i < geohash.Length; i++)
      {
        int cd = DecodeMap[geohash[i]].Value;
        for (int j = 0; j < BASE32_BITS; j++)
        {
          int mask = s_bits[j];
          if (isEvenBit)
          {
            DivideRangeDecode(hash, longitudeRange, (cd & mask) != 0);
          }
          else
          {
            DivideRangeDecode(hash, latitudeRange, (cd & mask) != 0);
          }
          isEvenBit = !isEvenBit;
        }
      }

      double latitude = (latitudeRange[0] + latitudeRange[1]) / 2;
      double longitude = (longitudeRange[0] + longitudeRange[1]) / 2;

      hash.m_point = new WGS84Point(latitude, longitude);
      SetBoundingBox(hash, latitudeRange, longitudeRange);
      hash.m_bits <<= (MAX_BIT_PRECISION - hash.m_significantBits);
      return hash;
    }

    /// <summary>
    /// Creates a <see cref="GeoHash"/> from a long value
    /// </summary>
    /// <param name="hashVal">the <see cref="GeoHash"/> as a <see cref="long"/></param>
    /// <param name="significantBits">How many bits to use from the long value (64 recommended and is default)</param>
    /// <returns>A <see cref="GeoHash"/></returns>
    public static GeoHash FromLongValue(Int64 hashVal, int significantBits = MAX_BIT_PRECISION)
    {
      double[] latitudeRange = new double[] {-90.0, 90.0};
      double[] longitudeRange = new double[] {-180.0, 180.0};

      bool isEvenBit = true;
      GeoHash hash = new GeoHash();

      string binaryString = Convert.ToString((long)hashVal, 2);
      while (binaryString.Length < MAX_BIT_PRECISION)
      {
        binaryString = "0" + binaryString;
      }
      for (int j = 0; j < significantBits; j++)
      {
        if (isEvenBit)
        {
          DivideRangeDecode(hash, longitudeRange, binaryString[j] != '0');
        }
        else
        {
          DivideRangeDecode(hash, latitudeRange, binaryString[j] != '0');
        }
        isEvenBit = !isEvenBit;
      }

      double latitude = (latitudeRange[0] + latitudeRange[1]) / 2;
      double longitude = (longitudeRange[0] + longitudeRange[1]) / 2;

      hash.m_point = new WGS84Point(latitude, longitude);
      SetBoundingBox(hash, latitudeRange, longitudeRange);
      hash.m_bits <<= (MAX_BIT_PRECISION - hash.m_significantBits);
      return hash;
    }

    /// <summary>
    /// This method uses the given number of characters as the desired precision
    /// value. The hash can only be 64bits long, thus a maximum precision of 12
    /// characters can be achieved.
    /// </summary>
    public static string GeoHashStringWithCharacterPrecision(double latitude, double longitude, int numberOfCharacters)
    {
      GeoHash hash = WithCharacterPrecision(latitude, longitude, numberOfCharacters);
      return hash.ToBase32();
    }

    private GeoHash(double latitude, double longitude, int desiredPrecision)
    {
      m_point = new WGS84Point(latitude, longitude);
      desiredPrecision = Math.Min(desiredPrecision, MAX_BIT_PRECISION);

      bool isEvenBit = true;
      double[] latitudeRange = new double[] {-90, 90};
      double[] longitudeRange = new double[] {-180, 180};

      while (m_significantBits < desiredPrecision)
      {
        if (isEvenBit)
        {
          DivideRangeEncode(longitude, longitudeRange);
        }
        else
        {
          DivideRangeEncode(latitude, latitudeRange);
        }
        isEvenBit = !isEvenBit;
      }

      SetBoundingBox(this, latitudeRange, longitudeRange);
      m_bits <<= (MAX_BIT_PRECISION - desiredPrecision);
    }

    private static void SetBoundingBox(GeoHash hash, double[] latitudeRange, double[] longitudeRange)
    {
      hash.m_boundingBox = new BoundingBox(new WGS84Point(latitudeRange[0], longitudeRange[0]), new WGS84Point(latitudeRange[1], longitudeRange[1]));
    }

    /// <summary>
    /// ?
    /// </summary>
    /// <param name="step">?</param>
    /// <returns></returns>
    public GeoHash Next(int step)
    {
      return FromOrd(Ord() + (long) step, m_significantBits);
    }

    /// <summary>
    /// ?
    /// </summary>
    /// <returns>The next <see cref="GeoHash"/></returns>
    public GeoHash Next()
    {
      return Next(1);
    }

    /// <summary>
    /// ?
    /// </summary>
    /// <returns>The previous <see cref="GeoHash"/></returns>
    public GeoHash Prev()
    {
      return Next(-1);
    }

    /// <summary>
    /// ?
    /// </summary>
    /// <returns>?</returns>
    public Int64 Ord()
    {
      int insignificantBits = MAX_BIT_PRECISION - m_significantBits;
      long bitMask = (1L << m_significantBits) - 1;
      return (m_bits >> insignificantBits) & bitMask;
    }

    /// <summary>
    /// Returns the number of characters that represent this hash.
    /// </summary>
    /// <exception cref="IllegalStateException">
    ///             when the hash cannot be encoded in base32, i.e. when the
    ///             precision is not a multiple of 5. </exception>
    public int CharacterPrecision
    {
      get
      {
        if (m_significantBits % 5 != 0)
        {
          throw new UnexpectedException("precision of GeoHash is not divisble by 5: " + this);
        }
        return m_significantBits / 5;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ord"></param>
    /// <param name="significantBits"></param>
    /// <returns></returns>
    public static GeoHash FromOrd(Int64 ord, int significantBits)
    {
      int insignificantBits = MAX_BIT_PRECISION - significantBits;
      return FromLongValue(ord << insignificantBits, significantBits);
    }

    /// <summary>
    /// Counts the number of geohashes contained between the two (ie how many
    /// times next() is called to increment from one to two) This value depends
    /// on the number of significant bits.
    /// </summary>
    /// <param name="one"> </param>
    /// <param name="two"> </param>
    /// <returns> number of steps </returns>
    public static Int64 StepsBetween(GeoHash one, GeoHash two)
    {
      if (one.SignificantBits != two.SignificantBits)
      {
        throw new System.ArgumentException("It is only valid to compare the number of steps between two hashes if they have the same number of significant bits");
      }
      return two.Ord() - one.Ord();
    }

    private void DivideRangeEncode(double value, double[] range)
    {
      double mid = (range[0] + range[1]) / 2;
      if (value >= mid)
      {
        AddOnBitToEnd();
        range[0] = mid;
      }
      else
      {
        AddOffBitToEnd();
        range[1] = mid;
      }
    }

    private static void DivideRangeDecode(GeoHash hash, double[] range, bool b)
    {
      double mid = (range[0] + range[1]) / 2;
      if (b)
      {
        hash.AddOnBitToEnd();
        range[0] = mid;
      }
      else
      {
        hash.AddOffBitToEnd();
        range[1] = mid;
      }
    }

    /// <summary>
    /// returns the 8 adjacent hashes for this one. They are in the following
    /// order:
    /// N, NE, E, SE, S, SW, W, NW
    /// </summary>
    public GeoHash[] Adjacent
    {
      get
      {
        GeoHash northern = NorthernNeighbour;
        GeoHash eastern = EasternNeighbour;
        GeoHash southern = SouthernNeighbour;
        GeoHash western = WesternNeighbour;
        return new GeoHash[] {northern, northern.EasternNeighbour, eastern, southern.EasternNeighbour, southern, southern.WesternNeighbour, western, northern.WesternNeighbour};
      }
    }

    /// <summary>
    /// how many significant bits are there in this <seealso cref="GeoHash"/>?
    /// </summary>
    public int SignificantBits
    {
      get
      {
        return m_significantBits;
      }
    }

    /// <summary>
    /// <see cref="long"/> representation of this <see cref="GeoHash"/> 
    /// </summary>
    public Int64 LongValue
    {
      get
      {
        return m_bits;
      }
    }

    /// <summary>
    /// get the base32 string for this <seealso cref="GeoHash"/>.
    /// this method only makes sense, if this hash has a multiple of 5
    /// significant bits.
    /// </summary>
    /// <exception cref="IllegalStateException">
    ///             when the number of significant bits is not a multiple of 5. </exception>
    public string ToBase32()
    {
      if (m_significantBits % 5 != 0)
      {
        throw new UnexpectedException("Cannot convert a geohash to base32 if the precision is not a multiple of 5.");
      }
      StringBuilder buf = new StringBuilder();

      Int64 firstFiveBitsMask = unchecked((long) 0xf800000000000000);
      Int64 bitsCopy = m_bits;
      int partialChunks = (int) Math.Ceiling(((double) m_significantBits / 5));

      for (int i = 0; i < partialChunks; i++)
      {
        int pointer = (int)((int)((uint)(bitsCopy & firstFiveBitsMask) >> 59));
        buf.Append(s_base32[pointer]);
        bitsCopy <<= 5;
      }
      return buf.ToString();
    }

    /// <summary>
    /// returns true if this is within the given geohash bounding box.
    /// </summary>
    public bool Within(GeoHash boundingBox)
    {
      return (m_bits & boundingBox.Mask()) == boundingBox.m_bits;
    }

    /// <summary>
    /// find out if the given point lies within this hashes bounding box.
    /// <i>Note: this operation checks the bounding boxes coordinates, i.e. does
    /// not use the <seealso cref="GeoHash"/>s special abilities.s</i>
    /// </summary>
    public bool Contains(WGS84Point point)
    {
      return m_boundingBox.Contains(point);
    }

    /// <summary>
    /// returns the <seealso cref="WGS84Point"/> that was originally used to set up this.
    /// If it was built from a base32-<seealso cref="String"/>, this is the center point of
    /// the bounding box.
    /// </summary>
    public WGS84Point Point
    {
      get
      {
        return m_point;
      }
    }

    /// <summary>
    /// return the center of this <seealso cref="GeoHash"/>s bounding box. this is rarely
    /// the same point that was used to build the hash.
    /// </summary>
    // TODO: make sure this method works as intented for corner cases!
    public WGS84Point BoundingBoxCenterPoint
    {
      get
      {
        return m_boundingBox.CenterPoint;
      }
    }

    /// <summary>
    /// Get <see cref="BoundingBox"/> for this <see cref="GeoHash"/> 
    /// </summary>
    public BoundingBox BoundingBox
    {
      get
      {
        return m_boundingBox;
      }
    }

    /// <summary>
    /// ?
    /// </summary>
    /// <param name="point">?</param>
    /// <param name="radius">?</param>
    /// <returns>?</returns>
    public bool EnclosesCircleAroundPoint(WGS84Point point, double radius)
    {
      return false;
    }

    internal GeoHash RecombineLatLonBitsToHash(Int64[] latBits, Int64[] lonBits)
    {
      GeoHash hash = new GeoHash();
      bool isEvenBit = false;
      int latShiftBits = MAX_BIT_PRECISION - (int) latBits[1];
      latBits[0] <<= latShiftBits;
      int lonShiftBits = MAX_BIT_PRECISION - (int) lonBits[1];
      lonBits[0] <<= lonShiftBits;
      double[] latitudeRange = new double[] {-90.0, 90.0};
      double[] longitudeRange = new double[] {-180.0, 180.0};

      for (int i = 0; i < (int) (latBits[1] + lonBits[1]); i++)
      {
        if (isEvenBit)
        {
          DivideRangeDecode(hash, latitudeRange, (latBits[0] & FIRST_BIT_FLAGGED) == FIRST_BIT_FLAGGED);
          latBits[0] <<= 1;
        }
        else
        {
          DivideRangeDecode(hash, longitudeRange, (lonBits[0] & FIRST_BIT_FLAGGED) == FIRST_BIT_FLAGGED);
          lonBits[0] <<= 1;
        }
        isEvenBit = !isEvenBit;
      }
      hash.m_bits <<= (MAX_BIT_PRECISION - hash.m_significantBits);
      SetBoundingBox(hash, latitudeRange, longitudeRange);
      hash.m_point = hash.m_boundingBox.CenterPoint;
      return hash;
    }

    /// <summary>
    /// Nearest <see cref="GeoHash"/> Neighbor to the North
    /// </summary>
    public GeoHash NorthernNeighbour
    {
      get
      {
        Int64[] latitudeBits = RightAlignedLatitudeBits;
        Int64[] longitudeBits = RightAlignedLongitudeBits;
        latitudeBits[0] += 1;
        latitudeBits[0] = (Int64) MaskLastNBits(latitudeBits[0], latitudeBits[1]);
        return RecombineLatLonBitsToHash(latitudeBits, longitudeBits);
      }
    }

    /// <summary>
    /// Nearest <see cref="GeoHash"/> Neighbor to the South
    /// </summary>
    public GeoHash SouthernNeighbour
    {
      get
      {
        Int64[] latitudeBits = RightAlignedLatitudeBits;
        Int64[] longitudeBits = RightAlignedLongitudeBits;
        latitudeBits[0] -= 1;
        latitudeBits[0] =  (Int64) MaskLastNBits(latitudeBits[0], latitudeBits[1]);
        return RecombineLatLonBitsToHash(latitudeBits, longitudeBits);
      }
    }

    /// <summary>
    /// Nearest <see cref="GeoHash"/> Neighbor to the East
    /// </summary>
    public GeoHash EasternNeighbour
    {
      get
      {
        Int64[] latitudeBits = RightAlignedLatitudeBits;
        Int64[] longitudeBits = RightAlignedLongitudeBits;
        longitudeBits[0] += 1;
        longitudeBits[0] = (Int64)MaskLastNBits(longitudeBits[0], longitudeBits[1]);
        return RecombineLatLonBitsToHash(latitudeBits, longitudeBits);
      }
    }

    /// <summary>
    /// Nearest <see cref="GeoHash"/> Neighbor to the West
    /// </summary>
    public GeoHash WesternNeighbour
    {
      get
      {
        Int64[] latitudeBits = RightAlignedLatitudeBits;
        Int64[] longitudeBits = RightAlignedLongitudeBits;
        longitudeBits[0] -= 1;
        longitudeBits[0] = (Int64)MaskLastNBits(longitudeBits[0], longitudeBits[1]);
        return RecombineLatLonBitsToHash(latitudeBits, longitudeBits);
      }
    }

    internal Int64[] RightAlignedLatitudeBits
    {
      get
      {
        Int64 copyOfBits = m_bits << 1;
        Int64 value = ExtractEverySecondBit(copyOfBits, NumberOfLatLonBits[0]);
        return new Int64[] { value, (Int64)NumberOfLatLonBits[0] };
      }
    }

    internal Int64[] RightAlignedLongitudeBits
    {
      get
      {
        Int64 copyOfBits = m_bits;
        Int64 value = ExtractEverySecondBit(copyOfBits, NumberOfLatLonBits[1]);
        return new Int64[] { value, (Int64)NumberOfLatLonBits[1] };
      }
    }

    private Int64 ExtractEverySecondBit(Int64 copyOfBits, int numberOfBits)
    {
      Int64 value = 0;
      for (int i = 0; i < numberOfBits; i++)
      {
        if ((copyOfBits & FIRST_BIT_FLAGGED) == FIRST_BIT_FLAGGED)
        {
          value |= 0x1;
        }
        value <<= 1;
        copyOfBits <<= 2;
      }
      value = (value >> 1);
      return value;
    }

    internal int[] NumberOfLatLonBits
    {
      get
      {
        if (m_significantBits % 2 == 0)
        {
          return new int[] {m_significantBits / 2, m_significantBits / 2};
        }
        else
        {
          return new int[] {m_significantBits / 2, m_significantBits / 2 + 1};
        }
      }
    }

    internal void AddOnBitToEnd()
    {
      m_significantBits++;
      m_bits <<= 1;
      m_bits = m_bits | 0x1;
    }

    internal void AddOffBitToEnd()
    {
      m_significantBits++;
      m_bits <<= 1;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      if (m_significantBits % 5 == 0)
      {
        return string.Format("{0} -> {1} -> {2}", Convert.ToString((long)m_bits, 2), m_boundingBox, ToBase32());
      }
      else
      {
        return string.Format("{0} -> {1}, bits: {2:D}", Convert.ToString((long)m_bits, 2), m_boundingBox, m_significantBits);
      }
    }

    /// <summary>
    /// Get binary <see cref="string"/> representation of this <see cref="GeoHash"/> 
    /// </summary>
    /// <returns><see cref="string"/> representation</returns>
    public string ToBinaryString()
    {
      StringBuilder bui = new StringBuilder();
      Int64 bitsCopy = m_bits;
      for (int i = 0; i < m_significantBits; i++)
      {
        if ((bitsCopy & FIRST_BIT_FLAGGED) == FIRST_BIT_FLAGGED)
        {
          bui.Append('1');
        }
        else
        {
          bui.Append('0');
        }
        bitsCopy <<= 1;
      }
      return bui.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (obj == this)
      {
        return true;
      }
      if (obj is GeoHash)
      {
        GeoHash other = (GeoHash) obj;
        if (other.m_significantBits == m_significantBits && other.m_bits == m_bits)
        {
          return true;
        }
      }
      return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      int f = 17;
      f = 31 * f + (int)(m_bits ^ (m_bits >> 32));
      f = 31 * f + m_significantBits;
      return f;
    }

    /// <summary>
    /// return a long mask for this hashes significant bits.
    /// </summary>
    private Int64 Mask()
    {
      if (m_significantBits == 0)
      {
        return 0;
      }
      else
      {
        Int64 value = FIRST_BIT_FLAGGED;
        value >>= (m_significantBits - 1);
        return value;
      }
    }

    private Int64 MaskLastNBits(Int64 value, Int64 n)
    {
      Int64 mask = unchecked((Int64) 0xffffffffffffffff);
      int shift = (int) (MAX_BIT_PRECISION - n);
      mask = mask >> shift;
      return value & mask;
    }

    public int CompareTo(GeoHash o)
    {
      int bitsCmp = (m_bits ^ FIRST_BIT_FLAGGED).CompareTo(o.m_bits ^ FIRST_BIT_FLAGGED);
      if (bitsCmp != 0)
      {
        return bitsCmp;
      }
      else
      {
        return m_significantBits.CompareTo(o.m_significantBits);
      }
    }
  }
}