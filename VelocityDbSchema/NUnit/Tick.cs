using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class Tick : OptimizedPersistable
  {
    static Random random = new Random(5);
    public string Symbol { get; set; }
    public DateTime Timestamp { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public int BidSize { get; set; }
    public int AskSize { get; set; }
    public string Provider { get; set; }
    private static string[] providers;
    public Tick()
    {
    }

    /// <summary>
    /// Generate random ticks
    /// </summary>
    public static IEnumerable<Tick> GenerateRandom(ulong count)
    {
      //Some symbols
      string[] symbols = new string[] { "EURUSD", "GBPCHF", "EURGBP", "JPYUSD", "GBPCAD" };
      string[] exchanges = new string[] { "NYSE", "TSE", "NASDAQ", "Euronext", "LSE", "SSE", "ASE", "SE", "NSEI" };
      providers = new string[] { "eSignal", "Gain", "NYSE", "TSE", "NASDAQ", "Euronext", "LSE", "SSE", "ASE", "SE", "NSEI" };
      double[] prices = new double[symbols.Length];
      double[] pipsizes = new double[symbols.Length];
      int[] digits = new int[symbols.Length];

      //Initialize startup data
      for (int i = 0; i < symbols.Length; i++)
      {
        prices[i] = 1.5;
        pipsizes[i] = 0.0001;
        digits[i] = 4;
      }

      DateTime timestamp = DateTime.Now;

      //Generate ticks
      for (ulong i = 0; i < count; i++)
      {
        int id = random.Next(symbols.Length);

        //generate random movement
        int pips = random.Next(0, 10);
        int direction = random.Next() % 2 == 0 ? 1 : -1;
        int spread = random.Next(2, 30);
        int seconds = random.Next(1, 30);

        //generate values
        string symbol = symbols[id];

        timestamp = timestamp.AddSeconds(seconds);

        double bid = prices[id];
        bid += direction * pips * pipsizes[id];
        bid = Math.Round(bid, digits[id]);

        double ask = bid + spread * pipsizes[id];
        ask = Math.Round(ask, digits[id]);

        //create tick
        Tick tick = new Tick();
        tick.Symbol = symbol;
        tick.Timestamp = timestamp;
        tick.Bid = bid;
        tick.Ask = ask;
        int bidSize = random.Next(0, 10000);
        int askSize = random.Next(0, 10000);
        string provider = providers[random.Next(providers.Length)];

        yield return tick;
      }
    }

    public override string ToString()
    {
      return String.Format("{0};{1:yyyy-MM-dd HH:mm:ss};{2};{3};{4};{5};{6}", Symbol, Timestamp, Bid, Ask, BidSize, AskSize, Provider);
    }
  }
}