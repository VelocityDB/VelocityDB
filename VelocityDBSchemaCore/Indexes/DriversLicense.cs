using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;
using Index = VelocityDb.Indexing.Index;

namespace VelocityDbSchema.Indexes
{
  [Index("stateOrCountry,licenseNumber")]
  public class DriversLicense : OptimizedPersistable 
  {
    string stateOrCountry;
    string licenseNumber;
    DateTime dateIssued;

    [Index]
    DateTime validUntil;

    public DriversLicense(string stateOrCountry, string licenseNumber, DateTime validUntil)
    {
      this.stateOrCountry = stateOrCountry;
      this.licenseNumber = licenseNumber;
      this.dateIssued = DateTime.Now;
      this.validUntil = validUntil;
    }

    [FieldAccessor("validUntil")]
    public DateTime ValidUntil
    {
      get
      {
        return validUntil;
      }
    }
  }
}
