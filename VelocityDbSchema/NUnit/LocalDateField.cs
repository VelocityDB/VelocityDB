using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class LocalDateField : OptimizedPersistable
  {
    private string m_Field1;

    private LocalDate m_Field2;

    public string Field1
    {
      get { return m_Field1; }
      set
      {
        Update();
        m_Field1 = value;
      }
    }

    public LocalDate Field2
    {
      get { return m_Field2; }
      set
      {
        Update();
        m_Field2 = value;
      }
    }

    public LocalDateField(string field1, LocalDate field2)
    {
      m_Field1 = field1;
      m_Field2 = field2;
    }
  }
}
