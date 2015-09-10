using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class LazyLoadPropertyClass : OptimizedPersistable
  {
    LazyLoadPropertyClass myRef;
    UInt32 ct;

    public LazyLoadPropertyClass(UInt32 ct, LazyLoadPropertyClass aRef)
    {
      myRef = aRef;
      this.ct = ct;
    }
    
    public override bool LazyLoadFields
    {
      get
      {
        return true;
      }
    }

    public LazyLoadPropertyClass MyRef
    {
      get
      {
        LoadFields();
        return myRef;
      }
      set
      {
        LoadFields();
        Update();
        myRef = value;
      }
    }

    public UInt32 MyCt
    {
      get
      {
        return ct;
      }
    }
    
    public LazyLoadPropertyClass MyRefPeek
    {
      get
      {
        return myRef;
      }
    }
  }
}
