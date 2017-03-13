using System.Collections.Generic;

namespace VelocityDBExtensions.geohash.util
{
  /// <summary>
  /// Iterate over all of the values within a bounding box at a particular
  /// resolution
  /// </summary>
  public class BoundingBoxGeoHashIterator : IEnumerable<GeoHash>
  {
    private TwoGeoHashBoundingBox m_boundingBoxRenamed;
    private GeoHash m_current;

    public BoundingBoxGeoHashIterator(TwoGeoHashBoundingBox bbox)
    {
      m_boundingBoxRenamed = bbox;
      m_current = bbox.BottomLeft;
    }

    public virtual TwoGeoHashBoundingBox BoundingBox
    {
      get
      {
        return m_boundingBoxRenamed;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<GeoHash> GetEnumerator()
    {
      while (!m_boundingBoxRenamed.BoundingBox.Contains(m_current.Point))
      {
        m_current = m_current.Next();
        yield return m_current;
      }
    }
  }
}