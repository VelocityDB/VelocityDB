using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Indexes
{
  [Index]
  public class Person : OptimizedPersistable 
  {
    string name;
    DriversLicense license;

    public Person(string name, DriversLicense license)
    {
      this.name = name;
      this.license = license;
    }
  }
}
