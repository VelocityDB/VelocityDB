using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Indexes
{ [Index("modelYear,brandName,modelName,color")]
  public abstract class Vehicle : OptimizedPersistable
  {
    [Index]
    string color;
    int maxPassengers;
    int fuelCapacity; // fuel capacity in liters   
    [Index]
    double litresPer100Kilometers; // fuel cunsumption 
    [Index]
    [UniqueConstraint]
    Guid guid = Guid.NewGuid();
    DateTime modelYear;
    [Index]
    [IndexStringByHashCode]
    string brandName;
    string modelName;
    List<VelocityDbSchema.Person> owners;
    int maxSpeed; // km/h
    int odometer; // km

    protected Vehicle(DateTime modelYear, string color = "Blue", int maxPassengers = 1, int fuelCapacity = 25, double litresPer100Kilometers = 0.66,  string brandName = "Nissan", string modelName = "Altima", int maxSpeed = 99, int odometer = 1500)
    {
      this.color = color;
      this.maxPassengers = maxPassengers;
      this.fuelCapacity = fuelCapacity;
      this.litresPer100Kilometers = litresPer100Kilometers;
      this.modelYear = modelYear;
      this.brandName = brandName;
      this.modelName = modelName;
      this.maxSpeed = maxSpeed;
      this.odometer = odometer;
      owners = new List<VelocityDbSchema.Person>();
      for (int i = 0; i < 5; i++)
        owners.Add(new Man());
    }

    [FieldAccessor("color")]
    public string Color
    {
      get
      {
        return color;
      }
      set
      {
        Update();
        color = value;
      }
    }

    [FieldAccessor("litresPer100Kilometers")]
    public double LitresPer100Kilometers
    {
      get
      {
        return litresPer100Kilometers;
      }
    }
  }
}
