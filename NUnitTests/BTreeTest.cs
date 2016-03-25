using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDbSchema;
using VelocityDbSchema.NUnit;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class BTreeTest
  {
    public const string systemDir = "c:/NUnitTestDbs";
    public const string location2Dir = "c:/NUnitTestDbsLocation2";

    [Test]
    public void hashCodeComparerIntTest()
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement(222, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.BeginUpdate();
        HashCodeComparer<int> hashCodeComparer = new HashCodeComparer<int>();
        BTreeSet<int> bTree = new BTreeSet<int>(hashCodeComparer, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        for (int i = 0; i < 100000; i++)
        {
          bTree.Add(i);
        }
        bTree.Clear();
        for (int i = 0; i < 100000; i++)
        {
          bTree.Add(i);
        }        
        session.Commit();
        session.Compact();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeSet<int> bTree= (BTreeSet<int>)session.Open(id);
        int count = 0;
        foreach (int num in bTree)
        {
          count++;
        }
        Assert.True(100000 == count);
        session.Commit();
      }
    }    
    
    [Test]
    public void hashCodeComparerStringTest()
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement(223, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.Compact();
        session.BeginUpdate();
        HashCodeComparer<string> hashCodeComparer = new HashCodeComparer<string>();
        BTreeSet<string> bTree = new BTreeSet<string>(hashCodeComparer, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        for (int i = 0; i < 100000; i++)
        {
          bTree.Add(i.ToString());
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeSet<string> bTree= (BTreeSet<string>)session.Open(id);
        int count = 0;
        foreach (string str in bTree)
        {
          count++;
        }
        Assert.True(100000 == count);
        session.Commit();
      }
    }

    [TestCase(600010)]
    public void aCreateDefaultCompareIntKeyIntValue(int number)
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement((UInt32)number, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.Compact();
        session.BeginUpdate();
        BTreeMap<int, int> bTree = new BTreeMap<int, int>(null, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        for (int i = 0; i < number; i++)
        {
          bTree.Add(i, i + 1);
        }
        bTree.Clear();
        for (int i = 0; i < number; i++)
        {
          bTree.Add(i, i + 1);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeMap<int, int> bTree = (BTreeMap<int, int>)session.Open(id);
        int count = 0;
        int prior = 0;
        foreach (KeyValuePair<int, int> pair in bTree)
        {
          count++;
          Assert.True(pair.Key == prior++);
          Assert.True(pair.Key == pair.Value - 1);
        }
        Assert.True(number == count);
        session.Commit();
      }
    }

    [TestCase(100010)]
    public void aCreateDefaultCompareOidShortIntKeyIntValue(int number)
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement((UInt32)number, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.Compact();
        session.BeginUpdate();
        BTreeMapOidShort<int, int> bTree = new BTreeMapOidShort<int, int>(null, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        for (int i = 0; i < number; i++)
        {
          bTree.Add(i, i + 1);
        }
        bTree.Clear();
        for (int i = 0; i < number; i++)
        {
          bTree.Add(i, i + 1);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeMapOidShort<int, int> bTree = (BTreeMapOidShort<int, int>)session.Open(id);
        int count = 0;
        int prior = 0;
        foreach (KeyValuePair<int, int> pair in bTree)
        {
          count++;
          Assert.True(pair.Key == prior++);
          Assert.True(pair.Key == pair.Value - 1);
        }
        Assert.True(number == count);
        session.Commit();
      }
    }

    // this one is slower than expected, check TO DO
    [TestCase(6020)]
    public void bCreateDefaultCompareIntKeyPersonValue(int number)
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement((UInt32)number, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        Placement personPlace = new Placement((UInt32)number + 1, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.BeginUpdate();
        BTreeMap<int, Person> bTree = new BTreeMap<int, Person>(null, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        Person person;
        for (int i = 0; i < number; i++)
        {
          person = new Person();
          person.Persist(personPlace, session);
          bTree.Add(i, person);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeMap<int, Person> bTree = (BTreeMap<int, Person>)session.Open(id);
        int count = 0;
        int prior = 0;
        foreach (KeyValuePair<int, Person> pair in bTree)
        {
          count++;
          Assert.True(pair.Key == prior++);
          Assert.True(pair.Value != null);
        }
        Assert.True(number == count);
        session.Commit();
      }
    }
    
    [TestCase(600000)]
    public void CreateDefaultCompareIntKey(int number)
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement((UInt32)number, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.BeginUpdate();
        BTreeSetOidShort<int> bTree = new BTreeSetOidShort<int>(null, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        for (int i = 0; i < number; i++)
        {       
          if (i > 1000 && i < 20000)
            bTree.Add(i);
          else
            bTree.AddFast(i);
        }
        bTree.Clear();
        for (int i = 0; i < number; i++)
        {       
          if (i > 1000 && i < 20000)
            bTree.Add(i);
          else
            bTree.AddFast(i);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeSetOidShort<int> bTree = (BTreeSetOidShort<int>) session.Open(id);
        int count = 0;
        int prior = 0;
        foreach (int i in bTree)
        {
          count++;
          Assert.True(i == prior++);
        }
        Assert.True(number == count);
        session.Commit();
      }
    }

    [TestCase(600001)]
    public void CreateCompareStruct(int number)
    {
      Oid id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Placement place = new Placement((UInt32)number, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        session.BeginUpdate();
        BTreeSetOidShort<Oid> bTree = new BTreeSetOidShort<Oid>(null, session);
        bTree.Persist(place, session);
        id = bTree.Oid;
        for (int i = 0; i < number; i++)
        {
          bTree.Add(new Oid((ulong) i));
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeSetOidShort<Oid> bTree = (BTreeSetOidShort<Oid>) session.Open(id);
        int count = 0;
        int prior = 0;
        foreach (Oid oid in bTree)
        {
          count++;
          Assert.True(oid.Id == (ulong) prior++);
        }
        Assert.True(number == count);
        session.Commit();
      }
    }   
 
    [Test]
    public void CreateDefaultCompareFailException()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Man aMan;
        Woman aWoman;
        session.BeginUpdate();
        BTreeSet<Person> bTree = new BTreeSet<Person>(null, session);
        for (int i = 0; i < 1000000; i++)
        {
          aMan = new Man();
          aWoman = new Woman();
          bTree.Add(aMan);
          bTree.Add(aWoman);
        }
        session.Commit();
      }
    }

    [Test]
    public void CreateDefaultCompare()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Man aMan = new Man();
        Woman aWoman = new Woman();
        session.BeginUpdate();
        BTreeSet<Person> bTree = new BTreeSet<Person>(null, session);
        session.Persist(bTree);
        for (int i = 0; i < 50000; i++)
        {
          aMan = new Man();
          aMan.Persist(session, aMan);
          aWoman = new Woman();
          aWoman.Persist(session, aWoman);
          bTree.Add(aMan);
          Assert.AreEqual(bTree.GetKeyId(aMan), aMan.Id);
          bTree.Add(aWoman);
        }
        session.Commit();
      }
    }

    [TestCase(5000, 0)]
    [TestCase(50000, 0)]
    [TestCase(500000, 0)]
    [TestCase(5001, 15)]
    [TestCase(50001, 15)]
    [TestCase(500001, 15)]
    public void CreateCompareFields(int numberOfLoops, int comparisonByteArraySize)
    {
      GCLatencyMode gcLatencyMode = GCSettings.LatencyMode; 
      Person.s_randGen = new Random(5);
      try
      {
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          //session.ClientCache.MinimumAvailableMegaBytes = 1100;
          //session.SetTraceAllDbActivity();
          Man aMan;
          Woman aWoman;
          session.BeginUpdate();
          CompareByField<Person> compareByField = new CompareByField<Person>("m_firstName", session, false);
          compareByField.AddFieldToCompare("m_lastName");
          compareByField.AddFieldToCompare("m_age");
          BTreeSet<Person> bTree = new BTreeSet<Person>(compareByField, session, 2000, (ushort)comparisonByteArraySize);
          Placement place = new Placement((UInt32)numberOfLoops);
          bTree.Persist(place, session);
          for (int i = 0; i < numberOfLoops; i++)
          {
            aMan = new Man();
            aWoman = new Woman();
            bTree.AddFast(aMan);
            bTree.AddFast(aWoman);
            if (i % 5000 == 0)
              bTree.FlushTransients();    
          }
          session.Commit();
        }
      }
      finally
      {
        GCSettings.LatencyMode = gcLatencyMode;
      }
    }

    [TestCase(100000, 1000)] 
    [TestCase(1000000, 10000)]
    public void CreateTicksCompareFields(int numberOfTicks, int nodeSize)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir, 2000, false))
      {
        //session.SetTraceAllDbActivity();
        session.BeginUpdate();
        CompareByField<Tick> compareByField = new CompareByField<Tick>("<Bid>k__BackingField", session, true);
        //compareByField.AddFieldToCompare("<Timestamp>k__BackingField");
        BTreeSet<Tick> bTree = new BTreeSet<Tick>(compareByField, session, (UInt16) nodeSize, sizeof(double) + sizeof(UInt64), true);
        Placement place = new Placement((UInt32)numberOfTicks, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        Placement ticksPlace = new Placement((UInt32)numberOfTicks, 10000, 1, UInt16.MaxValue, UInt16.MaxValue);
        bTree.Persist(place, session);
        int i = 0;
        int dublicates = 0;
        foreach (var record in Tick.GenerateRandom((ulong) numberOfTicks))
        {
          session.Persist(record, ticksPlace);
          if (bTree.Add(record))
            i++;
          else
            dublicates++;
        }
        session.Commit();
        Console.WriteLine("Done creating and sorting with BTreeSet<Tick> " + i + " Tick objects by Bid value. Number of dublicates (not added to BTreeSet): " + dublicates);
      }
    }

    [TestCase(100001, 1000)] 
    [TestCase(1000001, 10000)]
    public void CreateTicksCompareFieldsOidShort(int numberOfTicks, int nodeSize)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        session.Open(10, 1, 1, false);
        session.Open(10, 1, 2, false);
        session.Open(10, 2, 1, false);
        session.Open(10, 2, 2, false);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir, 2000, false))
      {
        //session.SetTraceAllDbActivity();
        //session.ClientCache.MinimumAvailableMegaBytes = 1100;
        session.BeginUpdate();
        CompareByField<Tick> compareByField = new CompareByField<Tick>("<Bid>k__BackingField", session, true);
        //compareByField.AddFieldToCompare("<Timestamp>k__BackingField");
        BTreeSetOidShort<Tick> bTree = new BTreeSetOidShort<Tick>(compareByField, session, (UInt16) nodeSize, sizeof(double), true);
        Placement place = new Placement((UInt32)numberOfTicks, 1, 1, UInt16.MaxValue, UInt16.MaxValue);
        Placement ticksPlace = new Placement((UInt32)numberOfTicks, 10000, 1, UInt16.MaxValue, UInt16.MaxValue);
        bTree.Persist(place, session);
        int i = 0;
        int dublicates = 0;
        foreach (var record in Tick.GenerateRandom((ulong) numberOfTicks))
        {
          session.Persist(record, ticksPlace);
          if (bTree.Add(record))
            i++;
          else
            dublicates++;
        }
        session.Commit();
        Console.WriteLine("Done creating and sorting with BTreeSetOidShort<Tick>" + i + " Tick objects by Bid value. Number of dublicates (not added to BTreeSet): " + dublicates);
      }
    }

    [TestCase(5000)]
    [TestCase(50000)]
    [TestCase(500000)]
    [TestCase(5001)]
    [TestCase(50001)]
    [TestCase(500001)]
    public void LookupCompareFields(int bTreeDatabaseNumber)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Person personPrior = null, person;
        session.BeginRead();
        BTreeSet<Person> bTree = (BTreeSet<Person>)session.Open(Oid.Encode((uint)bTreeDatabaseNumber, 1, 1));
        BTreeSetIterator<Person> itr = bTree.Iterator();
        int ct = 0;
        while ((person = itr.Next()) != null)
        {
          if (personPrior != null)
          {
            Assert.LessOrEqual(personPrior.FirstName, person.FirstName);
          }
          ct++;
          personPrior = person;
        }
        int ct2 = 0;
        personPrior = null;
        foreach (Person pers in (IEnumerable<Person>)bTree)
        {
          if (personPrior != null)
          {
            Assert.LessOrEqual(personPrior.FirstName, pers.FirstName);
          }
          ct2++;
          personPrior = pers;
        }
        session.Commit();
        Assert.AreEqual(ct, ct2);
      }
    }

    [TestCase(5000)]
    [TestCase(50000)]
    [TestCase(500000)]    
    [TestCase(5001)]
    [TestCase(50001)]
    [TestCase(500001)]
    public void UnpersistCompareFields(int bTreeDatabaseNumber)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        BTreeSet<Person> bTree = (BTreeSet<Person>)session.Open(Oid.Encode((uint)bTreeDatabaseNumber, 1, 1));
        BTreeSetIterator<Person> itr = bTree.Iterator();
        itr.GoToLast();
        itr.Remove();
        session.Abort();
        session.BeginUpdate();
        bTree = (BTreeSet<Person>)session.Open(Oid.Encode((uint)bTreeDatabaseNumber, 1, 1));
        bTree.Unpersist(session);
        session.Commit();
        session.BeginRead();
        Database db = session.OpenDatabase((uint)bTreeDatabaseNumber, false);
        foreach (Page page in db)
          foreach (OptimizedPersistable obj in page)
            if (obj.PageNumber > 0)
              Assert.Fail("No objects should remain in this database");
        session.Commit();
      }
    }
  }
}
