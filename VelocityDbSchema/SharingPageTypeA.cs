using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema
{
  public class SharingPageTypeA : OptimizedPersistable
  {
    static long ct = 0;
    long someData;
    public SharingPageTypeA()
    {
      someData = ++ct;
    }
  }
}
