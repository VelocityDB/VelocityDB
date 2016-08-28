using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class LazyLoadByDepth : OptimizedPersistable
  {
    LazyLoadByDepth myRef;
    UInt32 ct;

    public LazyLoadByDepth(UInt32 ct, LazyLoadByDepth aRef)
    {
      myRef = aRef;
      this.ct = ct;
    }

    public LazyLoadByDepth MyRef
    {
      get
      {
        Session?.LoadFields(this, 0);
        return myRef;
      }
      set
      {
        Session?.LoadFields(this, 0);
        Update();
        myRef = value;
      }
    }

    public LazyLoadByDepth MyRefPeek
    {
      get
      {
        return myRef;
      }
    }    
    
    public UInt32 MyCt    
    {
      get
      {
        return ct;
      }
    }
  }
}