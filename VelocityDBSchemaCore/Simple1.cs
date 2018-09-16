using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class Simple1 : OptimizedPersistable 
  {
    public Int32 createNumber;
    public Simple1(Int32 number) 
    {
      createNumber = number;
    }
  }
}
