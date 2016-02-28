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
  public class Payment : OptimizedPersistable
  {
    Order m_order;
    [AutoIncrement]
    UInt32 m_paymentId;

    public Payment(Order order)
    {
      m_paymentId = 0;
      m_order = order;
      order.Payments.Add(this);
    }

    [FieldAccessor("m_paymentId")]
    public UInt32 PaymentId
    {
      get
      {
        return m_paymentId;
      }
    }

    [FieldAccessor("m_order")]
    public Order Order
    {
      get
      {
        return m_order;
      }
    }
  }
}
