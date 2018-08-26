using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  public class WeakReferencedConnection<T> : OptimizedPersistable where T : OptimizedPersistable
  {
    UInt64 _objId;
    static WeakReferencedConnection()
    {
      var list = new List<Type> { typeof(T) };
      Schema.WeakReferencedTypes[typeof(WeakReferencedConnection<T>)] = list; // register this weak reference with schema so that DatabaseManager can regognice this as being a weak referenced object
    }

    public WeakReferencedConnection(T t)
    {
      if (!t.IsPersistent)
        throw new PersistedObjectExcpectedException("Persist first");
       t.GetSession().Persist(this);
      _objId = t.Id;
    }

    public T MyWeakReferencedObject
    {
      get
      {
        return Session.Open<T>(_objId);
      }
    }
  }
}
