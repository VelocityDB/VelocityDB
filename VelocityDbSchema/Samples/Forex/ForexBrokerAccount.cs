using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Samples.Forex
{
  [UniqueConstraint]
  [Index("brokerName,accountNumber")]  
  public class ForexBrokerAccount
  {
    public enum BrokerNameEnum : ushort { Unknown = 0, Dukascopy, LMax, Alpari };

    BrokerNameEnum brokerName;
    long accountNumber;

    public ForexBrokerAccount(BrokerNameEnum brokerName, long accountNumber)
    {
      this.brokerName = brokerName;
      this.accountNumber = accountNumber;
    }

    [FieldAccessor("accountNumber")]
    public long AccountNumber
    {
      get
      {
        return accountNumber;
      }
    }

    [FieldAccessor("brokerName")]
    public BrokerNameEnum BrokerName
    {
      get
      {
        return brokerName;
      }
    }
  }
}
