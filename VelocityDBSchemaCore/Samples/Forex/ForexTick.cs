using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace VelocityDBSchema.Forex
{
  [Index("m_dateTime,m_buy,m_sell")] // ?? not sure what main index you would want
  public class ForexTick : OptimizedPersistable
  {
    public enum Currency : ushort
    {
      Q00, // Not set or Unknown
      ARS, // Argentine Peso
      AUD, // Australian Dollar
      BHD, // Bahraini Dinar
      BBD, // Barbadian Dollar
      BRL, // Brazilian Real
      GBP, // British Pound
      CAD, // Canadian Dollar
      XAF, // Central African CFA franc
      CLP, // Chilean Peso
      CNY, // Chinese Yuan
      CYP, // Cyprus Pound
      CZK, // Czech Koruna
      DKK, // Danish Krone
      XCD, // East Caribbean Dollar
      EGP, // Egyptian Pound
      EEK, // Estonian Kroon
      EUR, // Euro,
      HKD, // Hong Kong Dollar
      HUF, // Hungarian Forint
      ISK, // Icelandic Krona
      INR, // Indian Rupee
      IDR, // Indonesian Rupiah
      ILS, // Israeli Sheqel
      JMD, // Jamaican Dollar
      JPY, // Japanese Yen
      JOD, // Jordanian Dinar
      KES, // Kenyan Shilling
      LVL, // Latvian Lats
      LBP, // Lebanese Pound
      LTL, // Lithuanian Litas
      MYR, // Malaysian Ringgit
      MXN, // Mexican Peso
      MAD, // Moroccan Dirham
      NAD, // Namibian Dollar
      NPR, // Nepalese Rupee
      NZD, // New Zealand Dollar
      NOK, // Norwegian Krone
      OMR, // Omani Rial
      PKR, // Pakistani Rupee
      PAB, // Panamanian Balboa
      PHP, // Philippine Peso
      PLN, // Polish Zloty,
      QAR, // Qatari Riyal
      RON, // Romanian Leu
      RUB, // Russian Rouble
      SAR, // Saudi Riyal
      SGD, // Singapore Dollar
      ZAR, // South African Rand
      KRW, // South Korean Won
      LKR, // Sri Lankan Rupee
      SEK, // Swedish Krona
      CHF, // Swiss Franc
      THB, // Thai Baht
      TRY, // Turkish Lira
      AED, // United Arab Emirates Dirham
      USD, // US Dollar
      VEF, // Venezuelan bolivar
      XOF  // West African CFA franc
    };
    [Index]
    DateTime m_dateTime;
    double m_bid;
    double m_ask;
    double m_spread;
    long m_volumeMin;
    long m_volumeMax;
    Currency m_buy;
    Currency m_sell;
    ForexBrokerAccount m_account;

    public ForexTick(ForexBrokerAccount account, DateTime dateTime, double bid, double ask, double spread, long minVolume, long maxVolume, string symbol)
    {
      m_dateTime = dateTime;
      m_bid = bid;
      m_ask = ask;
      m_volumeMax = maxVolume;
      m_volumeMin = minVolume;
      m_spread = spread;
      string buy = symbol.Remove(0, 3);
      string sell = symbol.Remove(3);
      m_buy = (Currency)Enum.Parse(typeof(Currency), buy);
      m_sell = (Currency)Enum.Parse(typeof(Currency), sell);
      m_account = account;
    }

    [FieldAccessor("m_dateTime")]
    public DateTime DateTime
    {
      get
      {
        return m_dateTime;
      }
    }

    [FieldAccessor("m_bid")]
    public double Bid
    {
      get
      {
        return m_bid;
      }
    }

    [FieldAccessor("m_ask")]
    public double Ask
    {
      get
      {
        return m_ask;
      }
    }

    [FieldAccessor("m_spread")]
    public double Spread
    {
      get
      {
        return m_spread;
      }
    }

    [FieldAccessor("m_volumeMax")]
    public long VolumeMax
    {
      get
      {
        return m_volumeMax;
      }
    }

    [FieldAccessor("m_volumeMin")]
    public long MinVolume
    {
      get
      {
        return m_volumeMin;
      }
    }

    [FieldAccessor("m_buy")]
    public Currency Buy
    {
      get
      {
        return m_buy;
      }
    }

    [FieldAccessor("m_sell")]
    public Currency Sell
    {
      get
      {
        return m_sell;
      }
    }

    [FieldAccessor("m_account")]
    public ForexBrokerAccount Account
    {
      get
      {
        return m_account;
      }
    }
  }
}
