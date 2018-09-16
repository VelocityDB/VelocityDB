using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class RecordDefinition : OptimizedPersistable
  {
    bool m_something;
    public bool Something
    {
      get
      {
        return m_something;
      }
      set
      {
        Update();
        m_something = value;
      }
    }
  }
}
