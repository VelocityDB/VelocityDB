using System;

namespace Frontenac.Blueprints.Geo
{
    [Serializable]
    public class GeoCircle : IGeoShape
    {
        public GeoCircle(double latitude, double longitude, double radius)
        {
            Center = new GeoPoint(latitude, longitude);
            Radius = radius;
        }

        public GeoCircle(GeoPoint center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public GeoPoint Center { get; set; }
        public double Radius { get; set; }
    }
}
