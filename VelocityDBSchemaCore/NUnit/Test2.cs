using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class Test2 : Common_Base_Class
  {
    private string m_Field1;

    private string m_field1;

    private Dictionary<Test, int> m_Dict_Field;

    public string Field1
    {
      get { return m_Field1; }
      set
      {
        Update();
        m_Field1 = value;
      }
    }

    public Dictionary<Test, int> Dict_Field
    {
      get { return m_Dict_Field; }
      set
      {
        Update();
        m_Dict_Field = value;
      }
    }

    public Test2(string str)
    {
      m_Field1 = str;
      //    m_Field3 = field3;

      Dict_Field = new Dictionary<Test, int>();



    }
  }

  public class Test : Common_Base_Class
  {
    private string m_Field1;

    private int m_Field3;

    public string Field1
    {
      get { return m_Field1; }
      set
      {
        Update();
        m_Field1 = value;
      }
    }

    public int Field3
    {
      get { return m_Field3; }
      set
      {
        Update();
        m_Field3 = value;
      }
    }

    public Test(string field1, int field3)
    {
      m_Field1 = field1;
      m_Field3 = field3;

    }
  }

  public class Common_Base_Class : OptimizedPersistable
  {
    public virtual string My_Short_Name()
    {
      return "Short Name";

    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }
  }
}
