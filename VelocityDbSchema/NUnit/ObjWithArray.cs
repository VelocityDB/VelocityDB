using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.NUnit
{
  public class ObjWithArray : OptimizedPersistable
  {
    UInt64[] myArray;
    public ObjWithArray(int arraySize)
    {
      myArray = new UInt64[arraySize];
    }
  }
}
