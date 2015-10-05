using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDBSchema.Forex
{
  [UniqueConstraint]
  [Index("m_brokerName,m_accountNumber")]
  public class ForexBrokerAccount : OptimizedPersistable
  {
    public enum BrokerNameEnum : ushort { Unknown = 0, Dukascopy, LMax, Alpari, Spotware, SalesDemo };

    BrokerNameEnum m_brokerName;
    int m_accountNumber;

    public ForexBrokerAccount(BrokerNameEnum brokerName, int accountNumber)
    {
      m_brokerName = brokerName;
      m_accountNumber = accountNumber;
    }

    [FieldAccessor("m_accountNumber")]
    public int AccountNumber
    {
      get
      {
        return m_accountNumber;
      }
    }

    [FieldAccessor("m_brokerName")]
    public BrokerNameEnum BrokerName
    {
      get
      {
        return m_brokerName;
      }
    }
  }
}
