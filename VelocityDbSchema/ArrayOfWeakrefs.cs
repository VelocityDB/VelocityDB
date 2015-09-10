using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class ArrayOfWeakrefs : OptimizedPersistable  
  {
    WeakIOptimizedPersistableReference<Person>[] anArrayOfWeakRefs;

    public ArrayOfWeakrefs(SessionBase session)
    {
      anArrayOfWeakRefs = new WeakIOptimizedPersistableReference<Person>[10];
      Person p = new Person();
      p.Persist(session, p);
      anArrayOfWeakRefs[0] = new  WeakIOptimizedPersistableReference<Person>(p);
      p = new Person();
      p.Persist(session, p);
      anArrayOfWeakRefs[1] = new  WeakIOptimizedPersistableReference<Person>(p);
      p = new Person();
      p.Persist(session, p);
      anArrayOfWeakRefs[2] = new  WeakIOptimizedPersistableReference<Person>(p);
      p = new Person();
      p.Persist(session, p);
      anArrayOfWeakRefs[3] = new  WeakIOptimizedPersistableReference<Person>(p);
    }
  }
}
