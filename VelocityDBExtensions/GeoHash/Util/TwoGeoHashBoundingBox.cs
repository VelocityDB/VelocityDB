namespace VelocityDBExtensions.geohash.util
{
  /// <summary>
  /// Ported to C# from Java by Mats Persson, VelocityDB, Inc.
  ///
  /// original Java code:
  /// Created by IntelliJ IDEA. User: kevin Date: Jan 17, 2011 Time: 12:03:47 PM
  /// </summary>
  public class TwoGeoHashBoundingBox
  {
    private BoundingBox m_boundingBox;
    private GeoHash m_bottomLeft;
    private GeoHash m_topRight;

    public static TwoGeoHashBoundingBox WithCharacterPrecision(BoundingBox bbox, int numberOfCharacters)
    {
      GeoHash bottomLeft = GeoHash.WithCharacterPrecision(bbox.MinLat, bbox.MinLon, numberOfCharacters);
      GeoHash topRight = GeoHash.WithCharacterPrecision(bbox.MaxLat, bbox.MaxLon, numberOfCharacters);
      return new TwoGeoHashBoundingBox(bottomLeft, topRight);
    }

    public static TwoGeoHashBoundingBox WithBitPrecision(BoundingBox bbox, int numberOfBits)
    {
      GeoHash bottomLeft = GeoHash.WithBitPrecision(bbox.MinLat, bbox.MinLon, numberOfBits);
      GeoHash topRight = GeoHash.WithBitPrecision(bbox.MaxLat, bbox.MaxLon, numberOfBits);
      return new TwoGeoHashBoundingBox(bottomLeft, topRight);
    }

    public static TwoGeoHashBoundingBox FromBase32(string base32)
    {
      string bottomLeft = base32.Substring(0, 7);
      string topRight = base32.Substring(7);
      return new TwoGeoHashBoundingBox(GeoHash.FromGeohashString(bottomLeft), GeoHash.FromGeohashString(topRight));
    }

    public TwoGeoHashBoundingBox(GeoHash bottomLeft, GeoHash topRight)
    {
      if (bottomLeft.SignificantBits != topRight.SignificantBits)
      {
        throw new System.ArgumentException("Does it make sense to iterate between hashes that have different precisions?");
      }
      m_bottomLeft = GeoHash.FromLongValue(bottomLeft.LongValue, bottomLeft.SignificantBits);
      m_topRight = GeoHash.FromLongValue(topRight.LongValue, topRight.SignificantBits);
      m_boundingBox = m_bottomLeft.BoundingBox;
      m_boundingBox.ExpandToInclude(m_topRight.BoundingBox);
    }

    public virtual BoundingBox BoundingBox
    {
      get
      {
        return m_boundingBox;
      }
    }

    public virtual GeoHash BottomLeft
    {
      get
      {
        return m_bottomLeft;
      }
    }

    public virtual GeoHash TopRight
    {
      get
      {
        return m_topRight;
      }
    }

    public virtual string ToBase32()
    {
      return m_bottomLeft.ToBase32() + m_topRight.ToBase32();
    }
  }
}
