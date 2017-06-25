using System;
using System.Collections.Generic;

namespace VelocityDBExtensions.Geo.util
{
  /// <summary>
  /// Select random samples of geohashes within a bounding box, without replacement
  /// </summary>
  public class BoundingBoxSampler
  {
    private TwoGeoHashBoundingBox m_boundingBox;
    private HashSet<int?> m_alreadyUsed = new HashSet<int?>();
    private int m_maxSamples;
    private Random m_rand = new Random();

    /// <param name="bbox"> </param>
    /// <exception cref="IllegalArgumentException">
    ///             if the number of geohashes contained in the bounding box
    ///             exceeds Integer.MAX_VALUE </exception>
    public BoundingBoxSampler(TwoGeoHashBoundingBox bbox)
    {
      m_boundingBox = bbox;
      Int64 maxSamplesLong = GeoHash.StepsBetween(bbox.BottomLeft, bbox.TopRight);
      if (maxSamplesLong > int.MaxValue)
      {
        throw new System.ArgumentException("This bounding box is too big too sample using this algorithm");
      }
      m_maxSamples = (int) maxSamplesLong;
    }

    public BoundingBoxSampler(TwoGeoHashBoundingBox bbox, int seed) : this(bbox)
    {
      m_rand = new Random(seed);
    }

    public virtual TwoGeoHashBoundingBox BoundingBox
    {
      get
      {
        return m_boundingBox;
      }
    }

    /// <returns> next sample, or NULL if all samples have been returned </returns>
    public virtual GeoHash Next()
    {
      if (m_alreadyUsed.Count == m_maxSamples)
      {
        return null;
      }
      int idx = m_rand.Next(m_maxSamples + 1);
      while (m_alreadyUsed.Contains(idx))
      {
        idx = m_rand.Next(m_maxSamples + 1);
      }
      m_alreadyUsed.Add(idx);
      GeoHash gh = m_boundingBox.BottomLeft.Next(idx);
      if (!m_boundingBox.BoundingBox.Contains(gh.Point))
      {
        return Next();
      }
      return gh;
    }
  }
}