using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.NUnit
{
  public struct ListWrapper<T>
  {
    List<T> m_list;

    public void Add(T item)
    {
      if (m_list == null)
        m_list = new List<T>();
      m_list.Add(item);
    }

    public T First()
    {
      if (m_list == null)
        throw new IndexOutOfRangeException();
      return m_list.First();
    }
  }
}
