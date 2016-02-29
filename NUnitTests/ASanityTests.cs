using System;
using System.Collections.Generic;
using System.Linq;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDBExtensions;
using VelocityDbSchema;
using VelocityDbSchema.Samples.AllSupportedSample;
using NUnit.Framework;
using System.Diagnostics;
using VelocityDbSchema.NUnit;
using VelocityDb.Collection.Comparer;
using VelocityDb.Collection.BTree;
using System.Threading.Tasks;

namespace NUnitTests
{
  public enum RecordType
  {
    Person,
    RecordDefinition
  }

  [TestFixture]
  public class ASanityTests
  {
    public const string systemDir = "c:\\NUnitTestDbs";
    private int _count;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      _count = 0;
    }

    [TestCase(false, (UInt32)8676)]
    [TestCase(true, (UInt32)8677)]
    public void DatabaseVersion(bool standalone, UInt32 dbNumber)
    {
      SessionBase session;
      if (standalone)
        session = new SessionNoServer(systemDir);
      else
        session = new ServerClientSession(systemDir);
      session.BeginUpdate();
      Placement place = new Placement(dbNumber);
      int i = 0;
      UInt64[] ids = new UInt64[10];
      VelocityDbSchema.Samples.Sample1.Person person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
      person.Persist(place, session);
      ids[i++] = person.Id;
      Database db = session.OpenDatabase(dbNumber);
      Assert.AreEqual(0, db.PersistentVersion());
      session.Commit();
      Assert.AreEqual(1, db.PersistentVersion());
      session.BeginUpdate();
      Assert.AreEqual(1, db.PersistentVersion());
      person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
      person.Persist(place, session);
      ids[i++] = person.Id;
      person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
      person.Persist(place, session);
      ids[i++] = person.Id;
      Assert.AreEqual(1, db.PersistentVersion());
      session.Commit();
      Assert.AreEqual(2, db.PersistentVersion());
      session.BeginRead();
      Assert.AreEqual(2, db.PersistentVersion());
      session.Commit();
      session.BeginUpdate();
      person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
      person.Persist(place, session);
      ids[i++] = person.Id;
      session.Commit();
      Assert.AreEqual(3, db.PersistentVersion());
      session.BeginUpdate();
      for (int j = 0; j < i; j++)
      {
        session.DeleteObject(ids[j]);
      }
      session.Commit();
      Assert.AreEqual(4, db.PersistentVersion());
      session.BeginRead();
      Assert.AreEqual(4, db.PersistentVersion());
      session.Commit();
      session.BeginUpdate();
      session.DeleteDatabase(db);
      session.Commit();
      session.Dispose();
    }
    [TestCase(false, 5, (UInt32)7676)]
    [TestCase(false, 1000000, (UInt32)7676)]
    [TestCase(true, 1000000, (UInt32)7676)]
    [TestCase(true, 50000, (UInt32)7676)]
    [TestCase(false, 100000, (UInt32)7676)]
    public void DeleteObjectsTestCreate(bool standalone, int count, UInt32 dbNumber)
    {
      SessionBase session;
      if (standalone)
        session = new SessionNoServer(systemDir);
      else
        session = new ServerClientSession(systemDir);
      VelocityDbSchema.Samples.Sample1.Person person;
      session.BeginUpdate();
      //session.SetTraceDbActivity(7676);
      var tw = new Stopwatch();
      tw.Start();
      UInt64[] ids = new UInt64[count];
      Placement place = new Placement(dbNumber);
      for (int i = 0; i < count; i++)
      {
        person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
        person.Persist(place, session);
        ids[i] = person.Id;
      }
      tw.Stop();
      Console.WriteLine("{0} records created in {1} ms.", count, tw.ElapsedMilliseconds);
      session.Commit();
      tw.Reset();
      tw.Start();
      session.BeginUpdate();
      for (int i = 0; i < count; i++)
      {
        session.DeleteObject(ids[i]);
      }
      session.Commit();
      tw.Stop();
      Console.WriteLine("{0} records deleted by Id in {1} ms.", count, tw.ElapsedMilliseconds);
      session.BeginRead();
      Database db = session.OpenDatabase(dbNumber);
      AllObjects<VelocityDbSchema.Samples.Sample1.Person> allPersons = db.AllObjects<VelocityDbSchema.Samples.Sample1.Person>();
      int ct = allPersons.Count();
      List<VelocityDbSchema.Samples.Sample1.Person> personList = db.AllObjects<VelocityDbSchema.Samples.Sample1.Person>().ToList();
      Assert.IsEmpty(personList);
      session.Commit();
      tw.Reset();
      tw.Start();
      session.BeginUpdate();
      for (int i = 0; i < count; i++)
      {
        person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
        person.Persist(place, session);
        ids[i] = person.Id;
      }
      session.Commit();
      tw.Stop();
      Console.WriteLine("{0} records created in {1} ms.", count, tw.ElapsedMilliseconds);
      tw.Reset();
      tw.Start();
      session.BeginUpdate();
      db = session.OpenDatabase(dbNumber);
      foreach (VelocityDbSchema.Samples.Sample1.Person p in db.AllObjects<VelocityDbSchema.Samples.Sample1.Person>())
      {
        p.Unpersist(session);
      }
      session.Commit();
      tw.Stop();
      Console.WriteLine("{0} records deleted in {1} ms.", count, tw.ElapsedMilliseconds);
      session.BeginRead();
      db = session.OpenDatabase(dbNumber);
      allPersons = db.AllObjects<VelocityDbSchema.Samples.Sample1.Person>();
      ct = allPersons.Count();
      personList = allPersons.ToList();
      Assert.IsEmpty(personList);
      //session.Verify();
      session.Commit();
      session.Dispose();
    }

    [Test]
    [Repeat(5)]
    public void Bug1Test()
    {
      _count++;
      VelocityDbSchema.Samples.Sample1.Person person;
      var sess = new ServerClientSession(systemDir);
      sess.BeginUpdate();
      var count = 10000;
      var tw = new Stopwatch();
      tw.Start();
      for (int i = 0; i < count; i++)
      {
        person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
        person.Persist(sess, person);
      }
      tw.Stop();
      Console.WriteLine("{0} records in {1} ms., Count is: {2}", _count, tw.ElapsedMilliseconds, _count);
      sess.Commit();
      sess.Dispose();
    }

    [Test]
    public void Bug2Test()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        const ushort nodeMaxSize = 5000;
        const ushort comparisonByteArraySize = sizeof(Int32); // enough room to hold entire idNumber of a Person
        const bool comparisonArrayIsCompleteKey = true;
        const bool addIdCompareIfEqual = false;
        session.BeginUpdate();
        //mySession.SetTraceAllDbActivity();
        CompareByField<Entity> compareByField = new CompareByField<Entity>("idNumber", session, addIdCompareIfEqual);
        BTreeSet<Entity> bTree = new BTreeSet<Entity>(compareByField, session, nodeMaxSize, comparisonByteArraySize, comparisonArrayIsCompleteKey);
        Placement place = new Placement(40);
        bTree.Persist(place, session);
        for (int i = 0; i < 5; i++)
        {
          var x = new Entity();
          x.idNumber = 250000;
          x.UserID = 100116;
          x.FirstName = "Bill";
          x.LastName = "Martin";
          x.MiddleName = "Bob";

          if (!bTree.Contains(x))
          {
            x.Persist(place, session);
            bTree.Add(x);
          }
        }
        Assert.IsTrue(bTree.Count == 1);
        session.Commit();
      }
    }

    void VersionManagerTest(SessionBase sess)
    {
      VelocityDbSchema.Samples.Sample1.Person person;
      sess.BeginUpdate();
      foreach (DatabaseLocation loc in sess.DatabaseLocations)
        Console.WriteLine(loc.ToStringDetails(sess, false));
      Console.WriteLine();
      var count = 10000;
      var vm = GetVersionManager(sess) ?? new VersionManager<VelocityDbSchema.Samples.Sample1.Person>(RecordType.Person);
      var startCount = vm.Count;
      var tw = new Stopwatch();
      tw.Start();
      for (int i = startCount; i < count + startCount; i++)
      {
        person = new VelocityDbSchema.Samples.Sample1.Person("Bill" + i, "Gates", 56);
        person.Persist(sess, person);
        vm.Add(new WeakIOptimizedPersistableReference<VelocityDbSchema.Samples.Sample1.Person>(person));
      }
      vm.Persist(sess, vm);
      tw.Stop();
      Console.WriteLine("{0} records in {1} ms.", count, tw.ElapsedMilliseconds);
      sess.Commit();

      // Now go get the vm and lookup the items
      if (sess is SessionNoServer)
        sess = new SessionNoServer(systemDir);
      else
        sess = new ServerClientSession(systemDir);
      sess.BeginRead();
      var vm2 = GetVersionManager(sess);
      if (vm2 != null)
      {
        Assert.IsTrue(vm2.Count > count - 1);
        person = vm2.Tip.GetTarget(false, sess);
        Assert.IsNotNull(person);
        var person2 = vm2.Tip.GetTarget(false, sess);
        Assert.AreSame(person, person2);
        Console.WriteLine("{0} records in version manager and tip is {1}.", vm2.Count, person2);
      }
      sess.Commit();
      sess.BeginUpdate();
      Database db = sess.OpenDatabase(VersionManager<VelocityDbSchema.Samples.Sample1.Person>.versionMangerDatabase, false, false);
      if (db != null)
        sess.DeleteDatabase(db);
      sess.Dispose();
    }

    [Test]
    public void aVersionManagerTest()
    {
      VersionManagerTest(new SessionNoServer(systemDir));
    }

    [Test]
    public void aVersionManagerTestServer()
    {
      VersionManagerTest(new ServerClientSession(systemDir));
    }

    [Test]
    public void GermanString()
    {
      UInt64 id = 0;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        VelocityDbSchema.Person person = new VelocityDbSchema.Person();
        person.LastName = "Med vänliga hälsningar";
        id = session.Persist(person);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        VelocityDbSchema.Person person = session.Open<VelocityDbSchema.Person>(id);
        person.LastName = "Mit freundlichen Grüßen";
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        VelocityDbSchema.Person person = session.Open<VelocityDbSchema.Person>(id);
        person.LastName = "Med vänliga hälsningar";
        session.Commit();
      }
    }

    private VersionManager<VelocityDbSchema.Samples.Sample1.Person> GetVersionManager(SessionBase session)
    {
      Database db = session.OpenDatabase(VersionManager<VelocityDbSchema.Samples.Sample1.Person>.versionMangerDatabase, false, false);
      if (db != null)
      {
        var versionManager = db.AllObjects<VersionManager<VelocityDbSchema.Samples.Sample1.Person>>().FirstOrDefault(vmObject => vmObject.RecordType == RecordType.Person);
        return versionManager;
      }
      else
        return default(VersionManager<VelocityDbSchema.Samples.Sample1.Person>);
    }

    [Test]
    public void License()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Database database;
        session.BeginUpdate();
        for (uint i = 60000000; i < 60000010; i++)
        {
          database = session.NewDatabase(i);
          Assert.NotNull(database);
        }
        session.Commit();
        session.BeginUpdate();
        for (uint i = 60000000; i < 60000010; i++)
        {
          database = session.OpenDatabase(i);
          Assert.NotNull(database);
        }
        session.Commit();
        session.BeginUpdate();
        for (uint i = 60000000; i < 60000010; i++)
        {
          database = session.OpenDatabase(i);
          session.DeleteDatabase(database);
        }
        session.Commit();
      }
    }

    [Test]
    public void Simple()
    {
      UInt64 id;
      // UInt64 autoIncrement = 0;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        VelocityDbSchema.Person person = new VelocityDbSchema.Person();
        session.Persist(person);
        //Assert.Greater(person.AutoIncrement, autoIncrement);
        UnknownSex u = new UnknownSex();
        session.Persist(u);
        //Assert.Greater(u.AutoIncrement, autoIncrement);
        UnknownSex u2 = new UnknownSex();
        session.Persist(u2);
        //Assert.Greater(u2.AutoIncrement, u.AutoIncrement);
        Man man = new Man();
        session.Persist(man);
        //Assert.AreEqual(man.AutoIncrement, autoIncrement); // Man overrides PlacementDatabaseNumber so no AutoIncrement feature
        id = man.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Man man = session.Open<Man>(id);
        Man man2 = (Man)man.Clone();
        Assert.NotNull(man);
        Assert.IsFalse(man2.IsPersistent);
        Assert.IsNull(man2.Shape);
        Assert.AreEqual(man2.Id, 0);
        Assert.AreEqual(man2.ShortId, 0);
        Assert.True(man != man2);
        session.Commit();
      }
    }

    [Test]
    public void DictionaryTest()
    {
      ulong id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict.Add("Mats", 52);
        dict.Add("Robin", 16);
        dict.Add("Kinga", 53);
        id = session.Persist(dict);
        var comp = dict.Comparer;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        object dictObj = session.Open(id);
        Dictionary<string, int> dict = (Dictionary<string, int>)dictObj;
        var comp = dict.Comparer;
        if (dict["Mats"] != 52)
          throw new Exception("failed");
        session.Commit();
      }
    }

    [TestCase(5000)]
    public void ManyTransactions(int numTransactions)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        for (int i = 0; i < numTransactions; i++)
        {
          session.BeginUpdate();
          Man man = new Man();
          session.Persist(man);
          session.Commit();
        }
      }
    }

    [Test]
    public void Transaction1()
    {
      Assert.Throws<AlreadyInTransactionException>(() =>
      {
        UInt64 id;
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginRead();
          session.BeginUpdate();
          Man man = new Man();
          man.Persist(session, man);
          id = man.Id;
          session.Commit();
        }
      });
    }

    [Test]
    public void Transaction2()
    {
      Assert.Throws<TryingToBeginReadOnlyTransactionWhileInUpdateTransactionException>(() =>
      {
        UInt64 id;
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          session.BeginRead();
          Man man = new Man();
          man.Persist(session, man);
          id = man.Id;
          session.Commit();
        }
      });
    }

    [Test]
    public void ArrayOfWeakRefs()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        ArrayOfWeakrefs weakRefArray = new ArrayOfWeakrefs(session);
        weakRefArray.Persist(session, weakRefArray);
        session.Commit();
        using (SessionNoServer session2 = new SessionNoServer(systemDir))
        {
          session2.BeginRead();
          weakRefArray = (ArrayOfWeakrefs)session2.Open(weakRefArray.Id);
          session2.Commit();
        }
      }
    }

    [Test]
    public void StoreCatTest()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        StoreCat storeCat = new StoreCat();
        Placement place = new Placement(StoreCat.PlaceInDatabase, 2, 1, 10, 10);
        storeCat.Persist(place, session);
        UInt64 id = storeCat.Id;
        session.Commit();
        using (SessionNoServer session2 = new SessionNoServer(systemDir))
        {
          session2.BeginRead();
          StoreCat storeCat2 = session2.Open<StoreCat>(id);
          if (storeCat2.cat.Name != "Boze")
            Console.WriteLine("Wrong storeName");
          session2.Commit();
        }
      }
    }

    [Test]
    public void GenericList()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        StoreList storeList = new StoreList();
        Placement place = new Placement(55, 2, 1, 10, 10);
        storeList.Persist(place, session);
        UInt64 id = storeList.Id;
        session.Commit();
        using (SessionNoServer session2 = new SessionNoServer(systemDir))
        {
          session2.BeginRead();
          StoreList storeList2 = session2.Open<StoreList>(id);
          if (storeList2.in16 != 16)
            Console.WriteLine("Should be 16");
          session2.Commit();
        }
      }
    }

    [Test]
    public void StoreStructTest()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        StoreStruct storeStruct = new StoreStruct();
        Placement place = new Placement(15, 2, 1, 10, 10);
        storeStruct.Persist(place, session);
        UInt64 id = storeStruct.Id;
        session.Commit();
        using (SessionNoServer session2 = new SessionNoServer(systemDir))
        {
          session2.BeginRead();
          StoreStruct storeStruct2 = (StoreStruct)session2.Open(id);
          if (storeStruct2.in16 != 16)
            Console.WriteLine("Should be 16");
          session2.Commit();
        }
      }
    }

    [Test]
    public void SimpleApiServer()
    {
      long ssn = 555555;
      UInt16 age = 1;
      VelocityDbSchema.Person mats;
      Placement place;
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        // skip delete database since it invalidates indices

        session.BeginUpdate();
        Database db = session.OpenDatabase(10, true, false);
        if (db != null)
          session.DeleteDatabase(db);
        session.Commit();
        session.BeginUpdate();
        place = new Placement(10, 2, 1, 1, 10);
        DateTime birthday = new DateTime(1960, 6, 13);
        mats = new Man("Mats", "Persson", age++, ssn++, birthday);
        mats.Persist(place, session);
        session.Commit();
        session.ClearServerCache();
      }
      UInt64 mOid1 = Oid.Encode(10, 2, 1);
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        UInt32 dbNum = Oid.DatabaseNumber(mOid1);
        mats = (VelocityDbSchema.Person)session.Open(mOid1);
        Woman kinga = null;
        mats = new Man("Mats", "Persson", age++, ssn++, new DateTime(1960, 6, 13));
        Cat cat = new Cat("Boze", 8);
        mats.m_pets.Add(cat);
        Bird bird = new Bird("Pippi", 1);
        cat.friends.Add(bird);
        mats.Persist(place, session);
        kinga = new Woman("Kinga", "Persson", age, ssn, mats, mats);
        kinga.Persist(place, session);
        VelocityDbSchema.Person robin = new VelocityDbSchema.Person("Robin", "Persson", 13, 1, mats, null);
        robin.Persist(place, session);
        mOid1 = mats.Id;
        mats = null;
        mats = (VelocityDbSchema.Person)session.Open(mOid1);
        session.Commit();
        session.ClearServerCache();
      }

      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        mats = (VelocityDbSchema.Person)session.Open(mOid1);
        session.Commit();
        session.ClearServerCache();
      }

      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginRead();
        ulong mOid2 = mats.Id;
        mats = (VelocityDbSchema.Person)session.Open(mOid2);
        session.Commit();
        session.ClearServerCache();
      }
    }

    [Test]
    public void SimpleObject0()
    {
      UInt64 id;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var rec = new TestRecord();
        rec.Persist(session, rec);
        id = rec.Id;
        session.Commit();
        session.Compact();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        TestRecord tr2 = (TestRecord)session.Open(id);
        session.Commit();
      }
    }

    [Test]
    public void SimpleObject1()
    {
      UInt64 id;
      TestRec tr;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        tr = new TestRec(55);
        session.Persist(tr, tr);
        id = tr.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        TestRec tr2 = (TestRec)session.Open(id);
        int trInt = (int)tr.Stuff;
        int tr2Int = (int)tr2.Stuff;
        session.Commit();
      }
    }

    [Test]
    public void SimpleObject2()
    {
      UInt64 id;
      TestRec tr;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        tr = new TestRec("Arch");
        session.Persist(tr, tr);
        id = tr.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        TestRec tr2 = (TestRec)session.Open(id);
        string trString = (string)tr.Stuff;
        string tr2String = (string)tr2.Stuff;
        Assert.AreEqual(trString, tr2String);
        session.Commit();
      }
    }

    [Test]
    public void SimpleObject3()
    {
      UInt64 id;
      TestRec tr;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        int[] intArray = new Int32[] { 6, 7 };
        tr = new TestRec(intArray);
        session.Persist(tr, tr);
        id = tr.Id;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        TestRec tr2 = session.Open<TestRec>(id);
        int[] trArray = (int[])tr.Stuff;
        int[] tr2Array = (int[])tr2.Stuff;
        Assert.AreEqual(trArray, tr2Array);
        session.Commit();
      }
    }

    [Test]
    [Repeat(3)]
    public void SortedMapTest()
    {
      var client = new SessionNoServer(systemDir);
      client.BeginUpdate();
      var set = new SortedMap<string, object>();
      set.Add("test", "Test text for example.");
      set.Persist(client, set);
      client.Commit();
      client.BeginRead();
      var readSet = client.AllObjects<SortedMap<string, object>>(false, false).FirstOrDefault(rec => rec.Id == set.Id);
      Assert.IsNotNull(readSet);
      client.Commit();
    }

    [Test]
    public void CastObjectArrayToWeakRefArrayTest()
    {
      var client = new SessionNoServer(systemDir);
      client.BeginUpdate();
      var rec = new CastRecord();
      rec.Persist(client, rec);
      client.Commit();
      client.BeginUpdate();
      var rec2 = new CastRecord();
      rec2.Fields.Add("test", new WeakIOptimizedPersistableReference<CastRecord>(rec));
      rec2.Persist(client, rec2);
      client.Commit();
    }

    [Test]
    public void SortedMapStringValueTest()
    {
      var client = new SessionNoServer(systemDir);
      const string key = "test";
      const string value = "string value text";
      client.BeginUpdate();
      var originalRecord = new StringRecord();
      originalRecord.Fields.Add(key, value);
      originalRecord.Persist(client, originalRecord);
      client.Commit();
      client = new SessionNoServer(systemDir);
      client.BeginUpdate();
      var newRecord = client.AllObjects<StringRecord>(false, false).FirstOrDefault(r => r.Id == originalRecord.Id);
      Assert.IsNotNull(newRecord);
      Assert.AreEqual(originalRecord.Fields[key], newRecord.Fields[key]);
      client.Commit();
      client.Dispose();
    }

    [Test]
    public void SortedMapTest2()
    {
      const string key = "test";
      const string value = "Test text for example.";
      var client = new SessionNoServer(systemDir);
      client.BeginUpdate();
      var originalRecord = new SortedMap<string, object>();
      originalRecord.Add(key, value);
      originalRecord.Persist(client, originalRecord);
      client.Commit();
      client.BeginRead();
      var newRecord = client.AllObjects<SortedMap<string, object>>(false, false).FirstOrDefault(r => r.Id == originalRecord.Id);
      Assert.IsNotNull(newRecord);
      Assert.AreEqual(originalRecord[key], newRecord[key]);
      client.Commit();
      client.Dispose();
    }

    public void UpdateRecordTest(SessionBase session)
    {
      session.BeginUpdate();
      var rec = new StringRecord2();
      rec.Fields["Title"] = "Simple Title";
      rec.Persist(session, rec);
      session.Commit();
      session.BeginUpdate();
      rec.Fields["Title"] = "Different title text";
      var rec2 = rec.Copy();
      rec2.Persist(session, rec2);
      session.Commit();
    }

    [Test]
    public void UpdateRecordTest()
    {
      UpdateRecordTest(new SessionNoServer(systemDir));
    }

    [Test]
    public void UpdateRecordTestServer()
    {
      UpdateRecordTest(new ServerClientSession(systemDir));
    }

    [TestCase(1000000)]
    public void AutoPlacementDbRollover(int howMany)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        FourPerPage f;
        for (UInt64 i = 0; i < 1000000; i++)
        {
          f = new FourPerPage(i);
          session.Persist(f);
        }
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(FourPerPage));
        Database db = session.OpenDatabase(dbNum);
        int ct = 0;
        foreach (FourPerPage f in db.AllObjects<FourPerPage>())
          ct++;
        Assert.AreEqual(ct, howMany);
        session.Commit();
      }
    }

    [TestCase(100000)]
    public void StringInternTest(int howMany)
    {
      UInt64 myId = 0;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        StringInternArrayOfStrings doIntern = new StringInternArrayOfStrings(howMany);
        myId = session.Persist(doIntern);
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        UInt32 dbNum = session.DatabaseNumberOf(typeof(StringInternArrayOfStrings));
        Database db = session.OpenDatabase(dbNum);
        long beforeRead = GC.GetTotalMemory(true);
        StringInternArrayOfStrings myIntern = (StringInternArrayOfStrings)session.Open(myId);
        long afterRead = GC.GetTotalMemory(true);
        System.Console.WriteLine("Memory before reading large string array object: " + beforeRead + " After read: " + afterRead + " " + myIntern.ToString());
        session.Commit();
      }
    }
    [Test]
    public void SortedObjectsSample()
    {
      Oid bTreeId;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        const UInt32 numberOfPersons = 100000;
        const ushort nodeMaxSize = 5000;
        const ushort comparisonByteArraySize = sizeof(UInt64); // enough room to hold entire idNumber of a Person
        const bool comparisonArrayIsCompleteKey = true;
        const bool addIdCompareIfEqual = false;
        VelocityDbSchema.Samples.AllSupportedSample.Person person;
        session.BeginUpdate();
        //mySession.SetTraceAllDbActivity();
        CompareByField<VelocityDbSchema.Samples.AllSupportedSample.Person> compareByField = new CompareByField<VelocityDbSchema.Samples.AllSupportedSample.Person>("idNumber", session, addIdCompareIfEqual);
        BTreeSet<VelocityDbSchema.Samples.AllSupportedSample.Person> bTree = new BTreeSet<VelocityDbSchema.Samples.AllSupportedSample.Person>(compareByField, session, nodeMaxSize, comparisonByteArraySize, comparisonArrayIsCompleteKey);
        session.Persist(bTree); // Persist the root of the BTree so that we have something persisted that can be flushed to disk if memory available becomes low
        for (int i = 0; i < numberOfPersons; i++)
        {
          person = new VelocityDbSchema.Samples.AllSupportedSample.Person();
          session.Persist(person);
          bTree.Add(person);
        }
        bTreeId = bTree.Oid;
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        BTreeSet<VelocityDbSchema.Samples.AllSupportedSample.Person> bTree = (BTreeSet<VelocityDbSchema.Samples.AllSupportedSample.Person>)session.Open(bTreeId);
        foreach (VelocityDbSchema.Samples.AllSupportedSample.Person person in (IEnumerable<VelocityDbSchema.Samples.AllSupportedSample.Person>)bTree)
        {
          if (person.IdNumber > 196988888791402)
          {
            Console.WriteLine(person);
            break;
          }
        }
        session.Commit();
        session.BeginRead();
        // this LINQ statement will trigger a binary search lookup (not a linear serach) of the matching Person objects in the BTreeSet
        Console.WriteLine((from person in bTree where person.IdNumber > 196988888791402 select person).First());
        session.Commit();
      }
    }

    [Test]
    public void Rajan()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        session.Persist("A");
        session.Persist("B");
        session.Persist("C");
        Placement place = new Placement(900, 1, 1);
        session.Persist("D", place);
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(string)));
        var strings = from string str in db.AllObjects<string>() where str.Length > 0 select str;
        foreach (var item in strings)
        {
          Console.WriteLine(item);
        }
        db = session.OpenDatabase(900);
        strings = from string str in db.AllObjects<string>() where str.Length > 0 select str;
        foreach (var item in strings)
        {
          Console.WriteLine(item);
        }
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        BTreeMap<string, VelocityDbList<int>> map = new BTreeMap<string, VelocityDbList<int>>(null, session);
        session.Persist(map);
        session.Commit();
      }
    }

    [Test]
    public void sessionPoolTest()
    {
      const int numberOfSessions = 5;
      using (SessionPool pool = new SessionPool(numberOfSessions, () => new SessionNoServer(systemDir)))
      {
        {
          int sessionId = -1;
          SessionBase session = null;
          try
          {
            session = pool.GetSession(out sessionId);
            using (SessionBase.Transaction transaction = session.BeginUpdate())
            {
              for (int i = 0; i < 1000; i++)
              {
                Man man = new Man();
                session.Persist(man);
              }
              session.Commit();
            }
          }
          catch (Exception e)
          {
            Console.WriteLine(e.Message);
            throw e;
          }
          finally
          {
            pool.FreeSession(sessionId, session);
          }
      }

      Parallel.ForEach(Enumerable.Range(0, numberOfSessions * 5),
        x =>
        {
          int sessionId = -1;
          SessionBase session = null;
          try
          {
            session = pool.GetSession(out sessionId);
            if (session.InTransaction == false)
              session.BeginRead();
            var allMen = session.AllObjects<Man>();
            int allMenCt = allMen.Count();
            foreach (Man man in allMen)
            {
              double lat = man.Lattitude;
            }
            Console.WriteLine("Man Count is: " + allMenCt + " Session Id is: " + sessionId + " Current task id is: " + (Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "unknown"));
          }
          catch (Exception e)
          {
            if (session != null)
              session.Abort();
            Console.WriteLine(e.Message);
            throw e;
          }
          finally
          {
            pool.FreeSession(sessionId, session);
          }
        });
    }
  }

    [Test]
    public void NonPersistentTest()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Test_Persist2 persistable2 = new Test_Persist2();
        persistable2.date_time_field = DateTime.Parse("1/1/1970");
        Test_Non_Persist non_persist = new Test_Non_Persist();
        non_persist.string_field = "bbb";
        non_persist.date_field = DateTime.Parse("1/1/1960");
        Test_Persist1 persistable1 = new Test_Persist1();
        persistable1.Field1 = "aaaa";
        persistable1.my_test_class = persistable2;
        persistable1.My_NonPersistable_Thing = non_persist;
        session.Persist(persistable1);
        session.Commit();
      }
    }
  }

public class VersionManager<T> : VelocityDbList<WeakIOptimizedPersistableReference<T>> where T : IOptimizedPersistable
{
  public const UInt32 versionMangerDatabase = 100;

  /// <summary>
  ///   Initializes a new instance of the <see cref = "VersionManager&lt;T&gt;" /> class.
  /// </summary>
  /// <param name = "recType">Type of the record.</param>
  public VersionManager(RecordType recType)
  {
    RecordType = recType;
  }

  public RecordType RecordType { get; private set; }

  public override uint PlacementDatabaseNumber
  {
    get
    {
      return versionMangerDatabase;
    }
  }

  /// <summary>
  ///   Gets the Tip of the versioned item (that is the latest version).
  /// </summary>
  public WeakIOptimizedPersistableReference<T> Tip
  {
    get
    {
      WeakIOptimizedPersistableReference<T> tip = null;
      if (Count > 0)
      {
        tip = this[Count - 1];
      }
      return tip;
    }
  }

  public class TipManager : SortedMap<Oid, WeakIOptimizedPersistableReference<IOptimizedPersistable>>
  {
  }
}

  public class Test_Persist1 : OptimizedPersistable
  {
    public string Field1;
    public Test_Persist2 my_test_class;
    public Test_Non_Persist My_NonPersistable_Thing;

  }
  public class Test_Persist2 : OptimizedPersistable
  {
    public DateTime date_time_field;
  }

  public class Test_Non_Persist
  {
    public string string_field;
    public DateTime date_field;
    public Test_Persist2 my_test_class;
  }
}
