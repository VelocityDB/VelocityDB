using System;

/*
 * Ported to C# from Java by Mats Persson, VelocityDB, Inc.
 *
 * original Java code:
 * Copyright 2010, Silvio Heuberger @ IFS www.ifs.hsr.ch
 *
 * This code is release under the LGPL license.
 * You should have received a copy of the license
 * in the LICENSE file. If you have not, see
 * http://www.gnu.org/licenses/lgpl-3.0.txt
 */
namespace VelocityDB.geohash.util
{

  /// <summary>
  /// Ecapsulates Vincety's geodesy algorithm .
  /// </summary>
  public class VincentyGeodesy
  {
    internal const double EquatorRadius = 6378137, PoleRadius = 6356752.3142, f = 1 / 298.257223563;
    public const double DegToRad = 0.0174532925199433;
    internal static readonly double EquatorRadiusSquared = EquatorRadius * EquatorRadius, PoleRadiusSquared = PoleRadius * PoleRadius;
    public const double EPSILON = 1e-12;

    /// <summary>
    /// returns the <seealso cref="WGS84Point"/> that is in the given direction at the
    /// following distance of the given point.<br>
    /// Uses Vincenty's formula and the WGS84 ellipsoid.
    /// </summary>
    /// <param name="bearingInDegrees">
    ///            : must be within 0 and 360 </param>
    /// <param name="point"> : where to start </param>
    /// <param name="distanceInMeters">: How far to move in the given direction </param>
    public static WGS84Point MoveInDirection(WGS84Point point, double bearingInDegrees, double distanceInMeters)
    {

      if (bearingInDegrees < 0 || bearingInDegrees > 360)
      {
        throw new System.ArgumentException("direction must be in (0,360)");
      }

      double a = 6378137, b = 6356752.3142, f = 1 / 298.257223563; // WGS-84
      // ellipsiod
      double alpha1 = bearingInDegrees * DegToRad;
      double sinAlpha1 = Math.Sin(alpha1), cosAlpha1 = Math.Cos(alpha1);

      double tanU1 = (1 - f) * Math.Tan(point.Latitude * DegToRad);
      double cosU1 = 1 / Math.Sqrt((1 + tanU1 * tanU1)), sinU1 = tanU1 * cosU1;
      double sigma1 = Math.Atan2(tanU1, cosAlpha1);
      double sinAlpha = cosU1 * sinAlpha1;
      double cosSqAlpha = 1 - sinAlpha * sinAlpha;
      double uSq = cosSqAlpha * (a * a - b * b) / (b * b);
      double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
      double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));

      double sinSigma = 0, cosSigma = 0, cos2SigmaM = 0;
      double sigma = distanceInMeters / (b * A), sigmaP = 2 * Math.PI;
      while (Math.Abs(sigma - sigmaP) > 1e-12)
      {
        cos2SigmaM = Math.Cos(2 * sigma1 + sigma);
        sinSigma = Math.Sin(sigma);
        cosSigma = Math.Cos(sigma);
        double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) - B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
        sigmaP = sigma;
        sigma = distanceInMeters / (b * A) + deltaSigma;
      }

      double tmp = sinU1 * sinSigma - cosU1 * cosSigma * cosAlpha1;
      double lat2 = Math.Atan2(sinU1 * cosSigma + cosU1 * sinSigma * cosAlpha1, (1 - f) * Math.Sqrt(sinAlpha * sinAlpha + tmp * tmp));
      double lambda = Math.Atan2(sinSigma * sinAlpha1, cosU1 * cosSigma - sinU1 * sinSigma * cosAlpha1);
      double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
      double L = lambda - (1 - C) * f * sinAlpha * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));

      double newLat = lat2 / DegToRad;
      double newLon = point.Longitude + L / DegToRad;

      newLon = (newLon > 180.0 ? 360.0 - newLon : newLon);
      newLon = (newLon < -180.0 ? 360.0 + newLon : newLon);

      return new WGS84Point(newLat, newLon);
    }

    public static double DistanceInMeters(WGS84Point foo, WGS84Point bar)
    {
      double a = 6378137, b = 6356752.3142, f = 1 / 298.257223563; // WGS-84
      // ellipsiod
      double L = (bar.Longitude - foo.Longitude) * DegToRad;
      double U1 = Math.Atan((1 - f) * Math.Tan(foo.Latitude * DegToRad));
      double U2 = Math.Atan((1 - f) * Math.Tan(bar.Latitude * DegToRad));
      double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1);
      double sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);

      double cosSqAlpha, sinSigma, cos2SigmaM, cosSigma, sigma;

      double lambda = L, lambdaP , iterLimit = 20;
      do
      {
        double sinLambda = Math.Sin(lambda), cosLambda = Math.Cos(lambda);
        sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) + (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
        if (sinSigma == 0)
        {
          return 0; // co-incident points
        }
        cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
        sigma = Math.Atan2(sinSigma, cosSigma);
        double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
        cosSqAlpha = 1 - sinAlpha * sinAlpha;
        cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;
        if (double.IsNaN(cos2SigmaM))
        {
          cos2SigmaM = 0; // equatorial line: cosSqAlpha=0
        }
        double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
        lambdaP = lambda;
        lambda = L + (1 - C) * f * sinAlpha * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
      } while (Math.Abs(lambda - lambdaP) > EPSILON && --iterLimit > 0);

      if (iterLimit == 0)
      {
        return double.NaN;
      }
      double uSquared = cosSqAlpha * (a * a - b * b) / (b * b);
      double A = 1 + uSquared / 16384 * (4096 + uSquared * (-768 + uSquared * (320 - 175 * uSquared)));
      double B = uSquared / 1024 * (256 + uSquared * (-128 + uSquared * (74 - 47 * uSquared)));
      double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) - B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
      double s = b * A * (sigma - deltaSigma);

      return s;
    }

  }

}