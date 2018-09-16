using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema
{  
  public class NotSharingPage : OptimizedPersistable
  {
    static long ct = 0;
    long someData;
    public NotSharingPage()
    {
      someData = ++ct;
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }
  }
}
