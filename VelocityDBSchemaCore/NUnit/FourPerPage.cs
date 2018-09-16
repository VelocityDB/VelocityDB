using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class FourPerPage : OptimizedPersistable
  {
    UInt64 m_ct;
    ListWrapper<UInt64> m_listWrapper;
    public FourPerPage(UInt64 ct)
    {
      m_ct = ct;
      m_listWrapper.Add(ct);
    }

    public override UInt16 ObjectsPerPage
    {
      get
      {
        return 4;
      }
    }

    public bool IsOK()
    {
      return m_ct == m_listWrapper.First();
    }
  }
}
