using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.NUnit
{
  public class CastRecord : OptimizedPersistable
  {
    public readonly SortedMap<string, object> Fields = new SortedMap<string, object>();
  }
}
