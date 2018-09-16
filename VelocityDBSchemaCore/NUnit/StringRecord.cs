using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.NUnit
{
  public class StringRecord : OptimizedPersistable
  {
    public StringRecord()
    {
      Fields = new SortedMap<string, object>();
    }
    public SortedMap<string, object> Fields { get; private set; }
  }
}
