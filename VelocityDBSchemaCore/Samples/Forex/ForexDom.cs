using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDBSchema.Forex
{
  public class ForexDom : OptimizedPersistable
  {
    public enum BidOrAsk : byte { Bid = 0, Ask = 1 };
    double price;
    double volume;
    int stepsAboveOrBelow;
    BidOrAsk type;
    ForexTick ticket;

    public ForexDom(double price, double volume, int stepsAboveOrBelow, BidOrAsk type, ForexTick ticket)
    {
      this.price = price;
      this.volume = volume;
      this.stepsAboveOrBelow = stepsAboveOrBelow;
      this.type = type;
      this.ticket = ticket;
    }
  }
}
