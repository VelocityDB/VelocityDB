using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.NUnit
{
  public class StringRecord2 : OptimizedPersistable
  {
    public StringRecord2()
    {
      Fields = new SortedMap<string, object>();
    }

    public SortedMap<string, object> Fields { get; private set; }

    public StringRecord2 Copy()
    {
      var result = new StringRecord2();
      foreach (var field in Fields)
      {
        result.Fields[field.Key] = field.Value;
      }
      return result;
    }
  }
}
