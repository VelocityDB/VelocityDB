using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Exceptions;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public abstract class TokenBase : OptimizedPersistable
  {
    public virtual UInt32 GlobalCount
    {
      get
      {
        throw new UnexpectedException("GlobalCount - Should not be used at this level (Word)");
      }
      set
      {
        throw new UnexpectedException("GlobalCount - Should not be used at this level (Word)");
      }
    }
  }
}
