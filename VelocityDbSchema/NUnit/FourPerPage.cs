using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class FourPerPage : OptimizedPersistable
  {
    UInt64 ct;
    public FourPerPage(UInt64 ct)
    {
      this.ct = ct;
    }

    public override UInt16 ObjectsPerPage
    {
      get
      {
        return 4;
      }
    }
  }
}
