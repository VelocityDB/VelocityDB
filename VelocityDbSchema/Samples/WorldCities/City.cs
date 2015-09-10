using System;
using System.Collections.Generic;
#if !NET35
using System.ComponentModel.DataAnnotations;
#endif
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using System.Globalization;

namespace VelocityDbSchema.Samples.WorldCities
{
  public class City : OptimizedPersistable
  {
#if !NET35
    [StringLength(2, ErrorMessage = "The country code must not exceed 2 caharacters. ")]
#endif
    string countryCode;
    string cityNameAscii;
    string cityNameIso;
#if !NET35
    [StringLength(2, ErrorMessage = "The state code must not exceed 2 caharacters. ")]
#endif 
    UInt32 population;
    float latitude;
    float longitude;

    public City(string countryCode, string cityNameAscii, string cityNameIso, string stateCode, string population, string latitude, string longitude)
    {
      //Encoding iso = Encoding.GetEncoding("ISO-8859-1");
      //Encoding utf8 = Encoding.UTF8; 
      this.countryCode = countryCode;
      this.cityNameAscii = cityNameAscii;
      //byte[] utfBytes = utf8.GetBytes(cityNameIso); 
      //byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
      //this.cityNameIso = iso.GetString(isoBytes); 
      this.cityNameIso = cityNameIso;
      if (population.Length > 0)
        this.population = UInt32.Parse(population);
      this.latitude = float.Parse(latitude);
      this.longitude = float.Parse(longitude);
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public string CityName
    {
      get
      {
        return cityNameAscii;
      }
    }

    public string Country
    {
      get
      {
        return new RegionInfo(countryCode).EnglishName;
      }
    }

    public string CountryCode
    {
      get
      {
        return countryCode;
      }
    }

    public UInt32 Population
    {
      get
      {
        return population;
      }
    }

    public float Longitude
    {
      get
      {
        return longitude;
      }
    }

    public float Latitude
    {
      get
      {
        return latitude;
      }
    }
  }
}
