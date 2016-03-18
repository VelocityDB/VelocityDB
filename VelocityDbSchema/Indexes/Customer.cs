using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.Indexes
{
  public class Customer : OptimizedPersistable
  {
    [Index]
    [IndexStringByHashCode]
    string m_name;
    [AutoIncrement]
    UInt32 m_customerId;
    string m_streetAddress;
    VelocityDbList<Order> m_orders;

    public Customer(string name)
    {
      m_customerId = 0;
      m_name = name;
      m_orders = new VelocityDbList<Order>();
    }

    [FieldAccessor("m_name")]
    public string Name
    {
      get
      {
        return m_name;
      }
      set
      {
        Update();
        m_name = value;
      }
    }

    [FieldAccessor("m_customerId")]
    public UInt32 OrderId
    {
      get
      {
        return m_customerId;
      }
    }

    [FieldAccessor("m_orders")]
    public VelocityDbList<Order> Orders
    {
      get
      {
        return m_orders;
      }
    }

    public string StreetAddress
    {
      get
      {
        return m_streetAddress;
      }
      set
      {
        if (IsPersistent)
          Session.UpdateObject(this, false, false); // Use instead of Update() to avoid removing object from indices, OK when only updating a non indexed field
        m_streetAddress = value;
      }
    }
  }
}
