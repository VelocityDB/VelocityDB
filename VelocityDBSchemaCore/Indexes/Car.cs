using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;
using Index = VelocityDb.Indexing.Index;

namespace VelocityDbSchema.Indexes
{
  [UniqueConstraint]
  [Index("registrationState,registrationPlate")]
  public class Car : Vehicle
  {
    string registrationState;
    string registrationPlate;
    [Index]
    InsuranceCompany insuranceCompany;
    string insurancePolicy;

    public Car(string color, int maxPassengers, int fuelCapacity, double litresPer100Kilometers, DateTime modelYear,
      string brandName, string modelName, int maxSpeed, int odometer, string registrationState, string registrationPlate,
      InsuranceCompany insuranceCompany, string insurancePolicy):base(modelYear,color, maxPassengers, fuelCapacity, litresPer100Kilometers,  brandName, modelName, maxSpeed, odometer)
    {
      this.registrationState = registrationState;
      this.registrationPlate = registrationPlate;
      this.insuranceCompany = insuranceCompany;
      this.insurancePolicy = insurancePolicy;
    }
  }
}
