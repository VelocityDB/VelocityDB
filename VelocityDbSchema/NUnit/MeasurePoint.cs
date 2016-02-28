using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.NUnit
{
  public class MeasurePoint : OptimizedPersistable
  {
    public MeasurePoint()
    { }

    [Index]
    [UniqueConstraint]
    [OnePerDatabase]
    int key;

    public MeasurePoint(int key) { this.key = key; }

    [FieldAccessor("key")]
    public int Key { get { return key; } }

    public List<float> Data { get; set; }

  }
}
