using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Samples.Forex
{
  [Index("dateTime,buy,sell")] // ?? not sure what main index you would want
  public class ForexTick : OptimizedPersistable
  {
    public enum Currency : ushort { USD = 840, SEK = 752 } // fill in all possible traded currencies
    [Index]
    DateTime dateTime;
    double bid;
    double ask;
    int tickVolume;
    int realVolume;
    Currency buy;
    Currency sell;
    ForexBrokerAccount account;

    public ForexTick(DateTime dateTime, double bid, double ask, int tickVolume, int realVolume, Currency buy, Currency sell, ForexBrokerAccount account)
    {
      this.dateTime = dateTime;
      this.bid = bid;
      this.ask = ask;
      this.realVolume = realVolume;
      this.tickVolume = tickVolume;
      this.buy = buy;
      this.sell = sell;
      this.account = account;
    }

    [FieldAccessor("dateTime")]
    public DateTime DateTime
    {
      get   
      {
        return dateTime;
      }
    }

    [FieldAccessor("bid")]
    public double Bid
    {
      get
      {
        return bid;
      }
    }

    [FieldAccessor("ask")]
    public double Ask
    {
      get
      {
        return ask;
      }
    }

    [FieldAccessor("realVolume")]
    public int RealVolume
    {
      get
      {
        return realVolume;
      }
    }

    [FieldAccessor("tickVolume")]
    public int TickVolume
    {
      get
      {
        return tickVolume;
      }
    }

    [FieldAccessor("buy")]
    public Currency Buy
    {
      get
      {
        return buy;
      }
    }

    [FieldAccessor("sell")]
    public Currency Sell
    {
      get
      {
        return sell;
      }
    }

  [FieldAccessor("account")]
    public ForexBrokerAccount Account
    {
      get
      {
        return account;
      }
    }
  }
}
