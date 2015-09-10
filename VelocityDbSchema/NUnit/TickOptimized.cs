using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.NUnit
{

  public class TickOptimized : OptimizedPersistable
  {
#if !NET35
    [StringLength(8, ErrorMessage = "The ticker symbol value cannot exceed 8 characters. ")]
#endif
    string symbol;
    DateTime timestamp;
    double bid;
    double ask;
    int bidSize;
    int askSize;
    string provider;
#if !NET35
    [StringLength(9, ErrorMessage = "The ticker exchange value cannot exceed 9 characters. ")]
#endif
    string exchange;

    public string Symbol
    {
      get
      {
        return symbol;
      }
      set
      {
        Update();
        symbol = value;
      }
    }

    public DateTime Timestamp
    {
      get
      {
        return timestamp;
      }
      set
      {
        Update();
        timestamp = value;
      }
    }

    public double Bid
    {
      get
      {
        return bid;
      }
      set
      {
        Update();
        bid = value;
      }
    }

    public double Ask
    {
      get
      {
        return ask;
      }
      set
      {
        Update();
        ask = value;
      }
    }

    public int BidSize
    {
      get
      {
        return bidSize;
      }
      set
      {
        Update();
        bidSize = value;
      }
    }    
    
    public int AskSize
    {
      get
      {
        return askSize;
      }
      set
      {
        Update();
        askSize = value;
      }
    }

    public string Exchange
    {
      get
      {
        return exchange;
      }
      set
      {
        exchange = value;
      }
    }

    public string Provider 
    {
      get
      {
        return provider;
      }
      set
      {
        provider = value;
      }
    }

    public TickOptimized(Tick tick)
    {
      symbol = tick.Symbol;
      timestamp = tick.Timestamp;
      bid = tick.Bid;
      ask = tick.Ask;
      askSize = tick.AskSize;
      bidSize = tick.BidSize;
      provider = tick.Provider;
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlacementDatabaseNumber;
      }
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override byte[] WriteMe(TypeVersion typeVersion, bool addShapeNumber, PageInfo pageInfo, IOptimizedPersistable owner, SessionBase session, bool inFlush)
    {
      //return base.WriteMe(typeVersion, addShapeNumber, pageInfo, owner, session, inFlush);
      byte[] bytes;
      using (MemoryStream memStream = new MemoryStream(100))
      {
        if (pageInfo.ShapeNumber == 0 && addShapeNumber)
          memStream.Write(BitConverter.GetBytes(typeVersion.ShortId), 0, sizeof(Int32));
        int byteCount = SessionBase.TextEncoding.GetByteCount(symbol);
        bytes = new byte[byteCount + sizeof(Int32)];
        if (byteCount == 0)
          byteCount = -1;
        Buffer.BlockCopy(BitConverter.GetBytes(byteCount), 0, bytes, 0, sizeof(Int32));
        SessionBase.TextEncoding.GetBytes(symbol, 0, symbol.Length, bytes, sizeof(Int32));
        memStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(timestamp.Ticks);
        memStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(bid);
        memStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(ask);
        memStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(askSize);
        memStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(bidSize);
        memStream.Write(bytes, 0, bytes.Length);
        byteCount = SessionBase.TextEncoding.GetByteCount(provider);
        bytes = new byte[byteCount + sizeof(Int32)];
        if (byteCount == 0)
          byteCount = -1;
        Buffer.BlockCopy(BitConverter.GetBytes(byteCount), 0, bytes, 0, sizeof(Int32));
        SessionBase.TextEncoding.GetBytes(provider, 0, provider.Length, bytes, sizeof(Int32));
        memStream.Write(bytes, 0, bytes.Length);
        bytes = memStream.ToArray();
        return bytes;
      }
    }
 //   public override bool Cache
//    {
//      get
//      {
//        return true;
 //     }
 //   }

    public override string ToString()
    {
      return String.Format("{0};{1:yyyy-MM-dd HH:mm:ss};{2};{3};{4};{5};{6}", Symbol, Timestamp, Bid, Ask, BidSize, AskSize, Provider);
    }
  }
}