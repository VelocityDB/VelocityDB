using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.Samples.AllSupportedSample;

namespace VelocityDbSchema
{
  public class StoreCat : OptimizedPersistable
  {
    public static UInt32 PlaceInDatabase = 88;
    public Cat cat;
    public StoreCat()
    {
      cat = new Cat("Boze", 8);
    }
  }
}
