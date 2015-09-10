using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema
{
  public class SharingPageTypeB : OptimizedPersistable
  {
    static long ct = 0;
    long someData;
    public SharingPageTypeB()
    {
      someData = ++ct;
    }
  }
}
