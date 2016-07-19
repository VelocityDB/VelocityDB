using NMoneys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class TestISerializable : ISerializable
  {
    static readonly TestISerializable s_instance = new TestISerializable();
    public int m_intOne;
    public string m_stringOne;
    public string m_notSerialized;

    public TestISerializable()
    {
      m_stringOne = "one";
      m_intOne = 1;
      m_notSerialized = "not";
    }

    private TestISerializable(SerializationInfo info, StreamingContext context)
    {
      m_intOne = info.GetInt32("m_intOne");
      m_stringOne = info.GetString("m_stringOne");
      m_notSerialized = "transient";
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_intOne", m_intOne);
      info.AddValue("m_stringOne", m_stringOne);
    }
  }

  public struct TestISerializableStruct : ISerializable
  {
    static readonly TestISerializable s_instance = new TestISerializable();
    public int m_intOne;
    public string m_stringOne;
    public string m_notSerialized;

    public TestISerializableStruct(string notSerialized)
    {
      m_stringOne = "one";
      m_intOne = 1;
      m_notSerialized = notSerialized;
    }

    private TestISerializableStruct(SerializationInfo info, StreamingContext context)
    {
      m_intOne = info.GetInt32("m_intOne");
      m_stringOne = info.GetString("m_stringOne");
      m_notSerialized = "transient";
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_intOne", m_intOne);
      info.AddValue("m_stringOne", m_stringOne);
    }
  }

    public class MoneyTest : ABaseClass
    {
      private string m_Field1;

      private Money m_Field3;

    public MoneyTest(string field1, Money field3)
    {
      m_Field1 = field1;
      m_Field3 = field3;
    }

    public string Field1
      {
        get { return m_Field1; }
        set
        {
          Update();
          m_Field1 = value;
        }
      }

      public Money Field3
      {
        get { return m_Field3; }
        set
        {
          Update();
          m_Field3 = value;
        }
      }
    }

  public class ABaseClass : OptimizedPersistable
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
