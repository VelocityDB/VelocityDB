using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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
}
