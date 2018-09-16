using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Indexes
{
  public class Truck : Vehicle
  {
    string registrationState;
    string registrationPlate;
    InsuranceCompany insuranceCompany;
    string insurancePolicy;
    int cargoCapacity; // in Kg

    public Truck(string color, int maxPassengers, int fuelCapacity, double litresPer100Kilometers, DateTime modelYear,
      string brandName, string modelName, int maxSpeed, int odometer, string registrationState, string registrationPlate,
      InsuranceCompany insuranceCompany, string insurancePolicy, int cargoCapacity):base(modelYear,color, maxPassengers, fuelCapacity, litresPer100Kilometers,  brandName, modelName, maxSpeed, odometer)
    {
      this.registrationState = registrationState;
      this.registrationPlate = registrationPlate;
      this.insuranceCompany = insuranceCompany;
      this.insurancePolicy = insurancePolicy;
      this.cargoCapacity = cargoCapacity;
    }
  }
}
