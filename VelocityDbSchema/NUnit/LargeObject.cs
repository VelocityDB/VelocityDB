using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.NUnit
{
  public class LargeObject
  {
    Int64[] m_int64array;

    public LargeObject(int int64arrayASize)
    {
      m_int64array = new Int64[int64arrayASize];
      for (int i = 0; i < int64arrayASize; i++)
        m_int64array[i] = i;
    }

    public bool IsOK()
    {
      for (int i = 0; i < m_int64array.Length; i++)
        if (m_int64array[i] != i)
          return false;
      return true;
    }
  }
}
