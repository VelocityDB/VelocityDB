using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class Counter : OptimizedPersistable
  {
    ulong count = 0;

    public ulong Count
    {
      get
      {
        return count;
      }
      set
      {
        Update();
        count = value;
      }
    }

    public ulong Next
    {
      get
      {
        Update();
        return ++count;
      }
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override CacheEnum Cache => CacheEnum.Yes;
  }
}
