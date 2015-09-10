using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class NullByteArray : OptimizedPersistable
  {
    public NullByteArray()
    {
      m_comparisonData = null;
      m_nodeList = null;
    }
    List<WeakIOptimizedPersistableReference<OptimizedPersistable>> m_nodeList;
    List<Person> m_personList;
    byte[] m_comparisonData;

    public byte[] ComparisonData
    {
      get
      {
        return m_comparisonData;
      }
    }

    public List<Person> PersonList
    {
      get
      {
        return m_personList;
      }
      set
      {
        m_personList = value;
      }
    }

    public List<WeakIOptimizedPersistableReference<OptimizedPersistable>> NodeList
    {
      get
      {
        return m_nodeList;
      }
      set
      {
        m_nodeList = value;
      }
    }
  }
}
