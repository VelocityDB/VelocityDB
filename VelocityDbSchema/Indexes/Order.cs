using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Indexes
{
  public class Order : OptimizedPersistable
  {
    [Index]
    Customer m_customer;
    [AutoIncrement]
    UInt32 m_orderId;
    VelocityDbList<Payment> m_payments;
    public Order(Customer customer)
    {
      m_orderId = 0;
      m_customer = customer;
      m_customer.Orders.Add(this);
      m_payments = new VelocityDbList<Payment>();
    }

    [FieldAccessor("m_customer")]
    public Customer Customer
    {
      get
      {
        return m_customer;
      }
      set
      {
        Update();
        m_customer = value;
      }
    }

    [FieldAccessor("m_orderId")]
    public UInt32 OrderId
    {
      get
      {
        return m_orderId;
      }
    }

    [FieldAccessor("m_payments")]
    public VelocityDbList<Payment> Payments
    {
      get
      {
        return m_payments;
      }
    }
  }
}
