using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class NullListWeakIOptimizedPersistableReference : OptimizedPersistable
  {
    List<WeakIOptimizedPersistableReference<OptimizedPersistable>> m_nodeList;

    public List<WeakIOptimizedPersistableReference<OptimizedPersistable>> NodeList
    {
      get
      {
        return m_nodeList;
      }
      set
      {
        Update();
        m_nodeList = value;
      }
    }
  }
}
