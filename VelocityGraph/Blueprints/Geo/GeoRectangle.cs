using System;

namespace Frontenac.Blueprints.Geo
{
    [Serializable]
    public class GeoRectangle : IGeoShape
    {
        public GeoRectangle(GeoPoint topLeft, GeoPoint bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public GeoRectangle(double minX, double maxX, double minY, double maxY)
        {
            TopLeft = new GeoPoint(minX, maxY);
            BottomRight = new GeoPoint(maxX, minY);
        }

        public GeoPoint TopLeft { get; set; }
        public GeoPoint BottomRight { get; set; }
    }
}
