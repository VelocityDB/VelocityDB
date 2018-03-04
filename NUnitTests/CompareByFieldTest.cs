using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDbSchema.Samples.AllSupportedSample;
using VelocityDbSchema;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class CompareByFieldTest
  {
    static Random randGen = new Random(5);
    public const string systemDir = "c:\\NUnitTestDbs";

    private string RandomString(int size)
    {
      StringBuilder builder = new StringBuilder();
      Random random = new Random();
      char ch;
      for (int i = 0; i < size; i++)
      {
        ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
        builder.Append(ch);
      }

      return builder.ToString();
    }

    [TestCase(0)]  
    [TestCase(10)]
    public void CompareString(int compArraySize)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("aString", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, (ushort) compArraySize);
        for (int i = 0; i < 10000; i++)
        {
          obj = new AllSupported(1, session);
          obj.aString = RandomString(10);
          sortedSet.Add(obj);
        }
        obj = new AllSupported(1, session);
        obj.aString = null;
        sortedSet.Add(obj);
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
          {
            if (prior.aString != null)
              if (currentObj.aString == null)
                Assert.Fail("Null is < than a non NULL");
              else
                Assert.Less(prior.aString, currentObj.aString);
          }
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareInt16()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("int16", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(Int16), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.int16 = (Int16) randGen.Next(Int16.MinValue, Int16.MaxValue);
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.int16, currentObj.int16);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }
   
    [Test]
    public void CompareInt32()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(2, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("int32", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(Int32), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.int32 = randGen.Next(Int32.MinValue, Int32.MaxValue);
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.int32, currentObj.int32);
          prior = currentObj;
          ct++;
        }        
        session.Commit();
      }
    }

    [Test]
    public void CompareInt32Descending()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(2, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("int32", session, false, false, false);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.int32 = randGen.Next(Int32.MinValue, Int32.MaxValue);
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Greater(prior.int32, currentObj.int32);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareInt32DescendingComparisonArray()
    {
#if DEBUG
      Assert.Throws<NotImplementedException>(() =>
      {
        using (var session = new SessionNoServerShared(systemDir))
        {
          session.BeginUpdate();
          AllSupported obj = new AllSupported(2, session);
          CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("int32", session, false, false, false);
          BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(Int32), true);
          for (int i = 0; i < 100000; i++)
          {
            obj = new AllSupported(1, session);
            obj.int32 = randGen.Next(Int32.MinValue, Int32.MaxValue);
            sortedSet.Add(obj);
          }
          int ct = 0;
          AllSupported prior = null;
          foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
          {
            if (prior != null)
              Assert.Greater(prior.int32, currentObj.int32);
            prior = currentObj;
            ct++;
          }
          session.Commit();
        }
      });
#endif
    }

    [Test]
    public void CompareInt64()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(2, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("int64", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(Int64), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.int64 = randGen.Next(Int32.MinValue, Int32.MaxValue) * randGen.Next();
          sortedSet.Add(obj);
        }
        int ct = 0;
        //foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        // {
        //  Console.WriteLine(currentObj.int32);
        //   ct++;
        // }
        AllSupported prior = null;
        ct = 0;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.int64, currentObj.int64);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareUInt16()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("uint16", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(UInt16), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.uint16 = (UInt16)randGen.Next(UInt16.MinValue, UInt16.MaxValue);
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.uint16, currentObj.uint16);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareUInt32()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("uint32", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(UInt32), true);
        for (int i = 0; i < 10000; i++)
        {
          obj = new AllSupported(1, session);
          obj.uint32 = (UInt32) randGen.Next();
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.uint32, currentObj.uint32);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareUInt64()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("uint64", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(UInt64), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.uint64 = (UInt64) (randGen.Next(Int32.MinValue, Int32.MaxValue) * randGen.Next());
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        ct = 0;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.uint64, currentObj.uint64);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }
    
    [Test]
    public void CompareSingle()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("single", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(Single), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.single = (Single)(randGen.NextDouble() - randGen.NextDouble()) * randGen.Next(UInt16.MinValue, UInt16.MaxValue);
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.single, currentObj.single);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CompareDouble(bool completeKey)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj = new AllSupported(1, session);
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("aDouble", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(double), completeKey);
        List<AllSupported> toRemove = new List<AllSupported>();
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.aDouble = (randGen.NextDouble() - randGen.NextDouble()) * randGen.Next(UInt16.MinValue, UInt16.MaxValue);
          if (i % 3500 == 0)
            toRemove.Add(obj);
          sortedSet.Add(obj);
        }
        session.Commit();
        session.BeginUpdate();
        foreach (AllSupported r in toRemove)
          sortedSet.Remove(r);
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.aDouble, currentObj.aDouble);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareDecimal()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj;
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("aDecimal", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(Decimal), true);
        for (int i = 0; i < 100000; i++)
        {
          obj = new AllSupported(1, session);
          obj.aDecimal = (Decimal) (randGen.NextDouble() - randGen.NextDouble()) * randGen.Next(UInt16.MinValue, UInt16.MaxValue);
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.aDecimal, currentObj.aDecimal);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareDateTime()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj;
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("dateTime", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(long), true);
        for (int i = 0; i < 20000; i++)
        {
          obj = new AllSupported(1, session);
          obj.dateTime = DateTime.FromBinary(randGen.Next(Int32.MinValue, Int32.MaxValue) * randGen.Next());
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.dateTime, currentObj.dateTime);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }

    [Test]
    public void CompareTimeSpan()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        AllSupported obj;
        CompareByField<AllSupported> compareByField = new CompareByField<AllSupported>("timeSpan", session);
        BTreeSet<AllSupported> sortedSet = new BTreeSet<AllSupported>(compareByField, session, 1000, sizeof(long), true);
        for (int i = 0; i < 10000; i++)
        {
          obj = new AllSupported(1, session);
          obj.timeSpan = TimeSpan.FromTicks(randGen.Next(Int32.MinValue, Int32.MaxValue) * randGen.Next());
          sortedSet.Add(obj);
        }
        int ct = 0;
        AllSupported prior = null;
        foreach (AllSupported currentObj in (IEnumerable<AllSupported>)sortedSet)
        {
          if (prior != null)
            Assert.Less(prior.timeSpan, currentObj.timeSpan);
          prior = currentObj;
          ct++;
        }
        session.Commit();
      }
    }
  }
}
