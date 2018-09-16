using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema
{
  [Index("_id")]
  public class UserR : OptimizedPersistable
  {
    [AutoIncrement(5)]
#pragma warning disable 0649
    private ulong _id;
#pragma warning restore 0649
    [FieldAccessor("_id")]
    public new ulong Id
    {
      get { return _id; }
    }
  }
}
