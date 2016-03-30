using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.NUnit
{
  public class TestClass : OptimizedPersistable
  {
    [NonSerialized]
    Dictionary<string, object> m_localChanges = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    private int m_someIntVar;
    IGeo_Co_Ord m_geoCord;
    IGeo_Co_Ord[] m_geoCords;
    object m_storeStructHere;
    public TestClass()
    {
      m_geoCord = new Geo_Co_Ord();
      m_geoCord.Latitude = 78.9;
      m_geoCord.Longitude = 88.3;
      m_geoCords = new IGeo_Co_Ord[10];
      for (int i = 0; i < 10; i++)
        m_geoCords[i] = m_geoCord;
      m_storeStructHere = m_geoCord;
    }

    public IGeo_Co_Ord GeoCord
    {
      get
      {
        return m_geoCord;
      }
    }
    public object StoredStructInObjectField
    {
      get
      {
        return m_storeStructHere;
      }
    }

    public int SomeIntVar
    {
      get
      {
        return m_someIntVar;
      }
      set
      {
        SetProperty<int>("SomeIntVar", ref m_someIntVar, value);
      }
    }

    private string m_someStringVar;

    public string SomeStringVar
    {
      get { return m_someStringVar; }
      set
      {
        SetProperty<string>("SomeStringVar", ref m_someStringVar, value);
      }
    }

    protected bool SetProperty<T>(string name, ref T value, T newValue)
    {
      if (EqualityComparer<T>.Default.Equals(value, newValue))
        return false;
      if (IsPersistent)
      {
        Update();
        if (!m_localChanges.ContainsKey(name))
        {
          object oldValue = value;
          if (oldValue is OptimizedPersistable)
            oldValue = (oldValue as OptimizedPersistable).Id;
          m_localChanges.Add(name, oldValue);
        }
      }
      value = newValue;
      return true;
    }

    /// <summary>
    /// Sets up some transient data fields after a database has been read from disk
    /// </summary>
    /// <param name="session">The session managing this object.</param>
    public override void InitializeAfterRead(SessionBase session)
    {
      m_localChanges = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

  }
  public interface IGeo_Co_Ord
  {
    Double Latitude { get; set; }
    Double Longitude { get; set; }
  }

  public struct Geo_Co_Ord : IGeo_Co_Ord
  {
    Double _Lat;
    public Double Latitude
    {
      get { return _Lat; }
      set
      {

        _Lat = value;

      }
    }

    Double _Long;

    public Double Longitude
    {
      get { return _Long; }
      set
      {

        _Long = value;

      }
    }

    public override string ToString()
    {
      return "Longitude: " + Longitude + " Latitude: " + Latitude;
    }
  };

  public class Class_A : OptimizedPersistable
  {
    public string string_field;
    public IClass_B b_field;
  }

  public interface IClass_B
  {
    int IntField { get; set; }
    string StringField { get; set; }
  }

  public class Class_B : OptimizedPersistable, IClass_B
  {
    private string string_field;
    private int int_field;


    public int IntField
    {
      get { return int_field; }
      set { int_field = value; }
    }
    public string StringField
    {
      get { return string_field; }
      set { string_field = value; }
    }
  }
}
