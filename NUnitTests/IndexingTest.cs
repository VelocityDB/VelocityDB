using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDbSchema.NUnit;
using NUnit.Framework;
using System.IO;
using VelocityDb.Session;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Indexing;
using VelocityDbSchema.Indexes;
using System.Diagnostics;
using VelocityDb.Collection;
using VelocityDBExtensions;
//using static VelocityDb.Collection.BTree.Extensions.BTreeExtensions;
using VelocityDBExtensions.Extensions.BTree;
using VelocityDb.Exceptions;
using RelSandbox;

namespace NUnitTests
{
  [TestFixture]
  public class IndexingTest
  {
    static readonly int DefaultSize = 20000;

    public const string systemDir = "f:/IndexingTestDbs";
    static readonly string licenseDbFile = @"c:/4.odb";

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

    string DisplayData(bool useServerSession)
    {
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        StringBuilder sb = new StringBuilder();
        session.BeginRead();
        try
        {
          BTreeBase<RDerby1, RDerby1> index = null;
          try
          {
            session.Index<RDerby1>();
          }
          catch (IndexDatabaseNotSpecifiedException)
          {
            UInt32 dbId = session.DatabaseNumberOf(typeof(RDerby1));
            Database db = session.OpenDatabase(dbId);
            index = session.Index<RDerby1>(db);
          }

          // for testing
          //var index = session.Index<Test1_0_0Class>("name2");  


          // create variable so that can examine in Locals

          if (index != null)  // should not be null?
          {
            // get all
            var testEntries = from t in index
                              select t;

            int count = testEntries.ToList<RDerby1>().Count;
            int i = 0;

            sb.AppendLine("Session Details:");
            sb.AppendLine(index.ToStringDetails(session));
            sb.AppendLine("Results:");
            foreach (RDerby1 test in testEntries)
            {
              try
              {
                string values = test.Name + ", " + test.Name2 + ", " + test.Added;

                sb.AppendLine(string.Format("{0, -5:00000}: {1}", i++, values));

              }
              catch (Exception ex)
              {
                return ("Exception thrown in for each loop: " + ex.Message);
              }
            }
          }
          else
          {
            return ("Index was null");
            // why is the index null? 
          }
        }
        catch (Exception ex)
        {
          return ("Exception occurred prior to loop: " + ex.Message);
          // examine problem using breakpoint.
        }
        finally
        {
          session.Commit();
        }
        return sb.ToString();
      }


    }

    [Test]
    public void aaaa_Jean()
    {
      using (SessionBase session = new SessionNoServer(systemDir))
      {
        ConsoleTraceListener consoleTrace = null;
        if (Trace.Listeners.OfType<ConsoleTraceListener>().Count() == 0)
        {
          consoleTrace = new ConsoleTraceListener();
          Trace.Listeners.Add(consoleTrace);
        }
        try
        {
          session.BeginUpdate();
          string brandName = "Toyota";
          string color = "Blue";
          int maxPassengers = 5;
          int fuelCapacity = 40;
          double litresPer100Kilometers = 5;
          DateTime modelYear = new DateTime(2003, 1, 1);
          string modelName = "Highlander";
          int maxSpeed = 200;
          int odometer = 100000;
          string registrationState = "Texas";
          string registrationPlate = "TX343434";
          string insurancePolicy = "CAA7878787";
          DriversLicense license = new DriversLicense("California", "B7788888", DateTime.Now + new TimeSpan(1825, 0, 0, 0));
          Person person = new Person("Mats Persson", license);
          InsuranceCompany insuranceCompany = new InsuranceCompany("Skandia", "851727878");
          Car car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
            odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
          session.Persist(car);
          car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
            odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
          session.Persist(car);
          Assert.Fail();
        }
        catch (UniqueConstraintException ex)
        {
          Console.WriteLine($"Got exception as expected: {ex}");
        }
      }
    }

    [TestCase(true, false)]
    [TestCase(false, false)]
    public void SimpleTest1_0_0(bool bGenerateUnique, bool useServerSession)
    {
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        try
        {
          session.BeginUpdate();
          session.CrossTransactionCacheAllDatabases();
          //session.RegisterClass(typeof(RDerby1));
          //Assert.True(session.Index<RDerby1>().Count == 0);
          // enter five records into data base
          Random random = new Random();
          for (int i = 0; i < 5; i++)
          {
            RDerby1 classData;
            string added = "added" + i;

            if (bGenerateUnique)
            {
              classData = new RDerby1("test" + i + "_" + random.Next(), "name2" + i.ToString() + "_" + random.Next(), added);
              session.Persist(classData);
            }
            else
              try
              {
                classData = new RDerby1("test" + i, "name2" + i.ToString(), added);
                session.Persist(classData);
              }
              catch (UniqueConstraintException)
              {
                Assert.Greater(i, 0);
              }
          }
          try
          {
            session.Commit();
          }
          catch (Exception ex)
          {
            throw new Exception("Exception occurred while committing records: " + ex.Message);
          }

        }
        catch (Exception ex)
        {
          // examine problem using breakpoint.
          throw new Exception("Exception occurred while persisting records: " + ex.Message);
        }
        finally
        {

        }
      }
      Console.WriteLine(DisplayData(useServerSession));
    }

    [Repeat(2)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    public void IndexSample(bool useServerSession, bool optimisticLocking)
    {
      string brandName = "Toyota";
      string color = "Blue";
      int maxPassengers = 5;
      int fuelCapacity = 40;
      double litresPer100Kilometers = 5;
      DateTime modelYear = new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      string modelName = "Highlander";
      int maxSpeed = 200;
      int odometer = 100000;
      string registrationState = "Texas";
      string registrationPlate = "TX343434";
      string insurancePolicy = "CAA7878787";
      DriversLicense license = new DriversLicense("California", "B7788888", DateTime.Now + new TimeSpan(1825, 0, 0, 0));
      Person person = new Person("Mats Persson", license);
      InsuranceCompany insuranceCompany = new InsuranceCompany("Allstate", "858727878");
      Car car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
        odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir, null, 2000, optimisticLocking) : (SessionBase)new SessionNoServer(systemDir, 5000, optimisticLocking))
      {
        session.BeginUpdate();
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"), true);
        foreach (Database db in session.OpenAllDatabases(true))
          if (db.DatabaseNumber >= 10 || db.DatabaseNumber == SessionBase.IndexDescriptorDatabaseNumber)
            session.DeleteDatabase(db);
        session.Commit();
        session.BeginUpdate();
        DatabaseLocation defaultLocation = session.DatabaseLocations.Default();
        List<Database> dbList = session.OpenLocationDatabases(defaultLocation, true);
        foreach (Database db in dbList)
          if (db.DatabaseNumber > Database.InitialReservedDatabaseNumbers)
            session.DeleteDatabase(db);
        session.DeleteLocation(defaultLocation);
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"), true);
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        int ct = 0;
        var mIndex = session.Index<Motorcycle>();
        if (mIndex != null)
          foreach (Motorcycle mc in session.Index<Motorcycle>())
          {
            Assert.NotNull(mc);
            ++ct;
          }
        Assert.AreEqual(ct, 0);
        session.Commit();
      }
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (int i = 0; i < 10000; i++)
        {
          Motorcycle mc = new Motorcycle();
          session.Persist(mc);
        }
        session.Checkpoint();
        int ct = 0;
        var mIndex = session.Index<Car>();
        if (mIndex != null)
          foreach (Car c in mIndex)
          {
            Assert.NotNull(c);
            ++ct;
          }
        Assert.AreEqual(ct, 0);
        ct = 0;
        session.RegisterClass(typeof(Person));
        foreach (Person p in session.AllObjects<Person>(true, false))
        {
          Assert.NotNull(p);
          ++ct;
        }
        session.Commit();
        session.BeginRead();
        ct = 0;
        foreach (Motorcycle mc in session.AllObjects<Motorcycle>(false, true))
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 10000);
        session.Commit();
        session.BeginRead();
        ct = 0;
        foreach (Motorcycle mc in session.AllObjects<Motorcycle>(false, true))
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 10000);
        session.Abort();
        session.BeginRead();
        ct = 0;
        foreach (Motorcycle mc in session.AllObjects<Motorcycle>(false, true))
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 10000);
        session.Commit();
        session.BeginRead();
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>())
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 10000);
        session.Abort();
        session.BeginRead();
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>())
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 10000);
        session.Commit();
        try
        {
          ct = 0;
          foreach (Motorcycle mc in session.Index<Motorcycle>())
          {
            Assert.NotNull(mc);
            ++ct;
          }
          Assert.AreEqual(ct, 10000);
        }
        catch (NotInTransactionException ex)
        {
          Console.WriteLine(ex.Message);
        }
        session.BeginUpdate();
        ct = 0;
        foreach (Motorcycle mc in session.AllObjects<Motorcycle>(false, true))
          if (++ct % 2 == 0)
            mc.Unpersist(session);
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>())
          ++ct;
        Assert.AreEqual(ct, 5000);
        session.Abort();
        session.BeginUpdate();
        ct = 0;
        foreach (Motorcycle mc in session.AllObjects<Motorcycle>(false, true))
          if (++ct % 2 == 0)
            mc.Unpersist(session);
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>())
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 5000);
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>("cc"))
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 5000);
        ct = 0;
        try
        {
          foreach (Motorcycle mc in session.Index<Motorcycle>("ccx"))
          {
            Assert.NotNull(mc);
            ++ct;
          }
          Assert.AreEqual(ct, 5000);
        }
        catch (FieldDoesNotExistException)
        {
        }
        session.Commit();
        session.BeginUpdate();
        ct = 0;
        double prior = -44.0;
        foreach (Motorcycle mc in session.Index<Motorcycle>("cc"))
        {
          Assert.NotNull(mc);
          mc.CC = mc.CC - prior;
          prior = mc.CC;
          ++ct;
        }
        Assert.AreEqual(ct, 2500);
        for (int i = 0; i < 95000; i++)
        {
          Motorcycle mc = new Motorcycle();
          session.Persist(mc);
        }
        session.FlushUpdates();
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>())
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 100000);
        session.Abort();
        session.BeginUpdate();
        ct = 0;
        prior = double.MinValue;
        foreach (Motorcycle mc in session.Index<Motorcycle>("cc"))
        {
          Assert.NotNull(mc);
          Assert.GreaterOrEqual(mc.CC, prior);
          prior = mc.CC;
          ++ct;
          if (ct < 25)
            Console.Write(prior.ToString() + " ");
        }
        ct = 0;
        prior = -44.0;
        foreach (Motorcycle mc in session.Index<Motorcycle>("cc"))
        {
          Assert.NotNull(mc);
          mc.CC = mc.CC - prior;
          prior = mc.CC;
          ++ct;
        }
        Assert.AreEqual(ct, 2500);
        session.Checkpoint();
        ct = 0;
        prior = double.MinValue;
        foreach (Motorcycle mc in session.Index<Motorcycle>("cc"))
        {
          Assert.NotNull(mc);
          Assert.GreaterOrEqual(mc.CC, prior);
          prior = mc.CC;
          ++ct;
        }
        for (int i = 0; i < 95000; i++)
        {
          Motorcycle mc = new Motorcycle();
          session.Persist(mc);
          DataBaseFileEntry dbEntry = new DataBaseFileEntry { Something = "Something" };
          session.Persist(dbEntry);
          mc.AddChild(dbEntry);
          mc.AddChild(dbEntry);
        }
        session.FlushUpdates();
        ct = 0;
        foreach (Motorcycle mc in session.Index<Motorcycle>())
        {
          Assert.NotNull(mc);
          ++ct;
        }
        Assert.AreEqual(ct, 100000);
        session.Commit();
        session.BeginRead();
        session.Abort();
        session.BeginUpdate();
        foreach (Motorcycle mc in session.Index<Motorcycle>())
        {
          Assert.NotNull(mc);
          VelocityDbList<DataBaseFileEntry> children = mc.Children;
          if (children != null && children.Count > 0)
            mc.RemoveChild(children[0]);
          ++ct;
        }
        session.Commit();
        session.BeginRead();
        ct = 0;
        prior = double.MinValue;
        foreach (Motorcycle mc in session.Index<Motorcycle>("cc"))
        {
          Assert.NotNull(mc);
          Assert.GreaterOrEqual(mc.CC, prior);
          prior = mc.CC;
          ++ct;
        }
        Assert.AreEqual(ct, 100000);
        session.Commit();
        Console.WriteLine("Motorcycle index Test OK");
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        session.Persist(car);
        registrationState = "Maine";
        car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
        odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
        session.Persist(car);
        color = "Red";
        maxPassengers = 5;
        fuelCapacity = 50;
        litresPer100Kilometers = 8;
        modelYear = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        brandName = "Toyota";
        modelName = "Tacoma";
        maxSpeed = 210;
        odometer = 50000;
        registrationState = "Texas";
        registrationPlate = "TX343433";
        insurancePolicy = "CAA7878777";
        car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
          odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
        session.Persist(car);
        color = "Black";
        maxPassengers = 5;
        fuelCapacity = 60;
        litresPer100Kilometers = 3;
        modelYear = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        brandName = "Lincoln";
        modelName = "Town Car";
        maxSpeed = 220;
        odometer = 250000;
        registrationState = "Texas";
        registrationPlate = "TX543433";
        insurancePolicy = "CAA7878775";
        for (int i = 0; i < 1; i++)
        {
          registrationState = RandomString(2);
          registrationPlate = RandomString(12);
          car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed, odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
          session.Persist(car);
          color = null;
          maxPassengers = i;
          fuelCapacity = 60;
          litresPer100Kilometers = 3;
          modelYear = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          brandName = "Null Car";
          modelName = null;
          maxSpeed = 220;
          odometer = 250000;
          insurancePolicy = null;
        }
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Console.WriteLine("Blue Cars");
        BTreeSet<Car> bTree = session.Index<Car>("color");
        foreach (Car c in (from aCar in bTree where aCar.Color == "Blue" select aCar))
          Console.WriteLine(c.ToStringDetails(session));
        Console.WriteLine("Cars in fuel efficiency order");
        foreach (Car c in session.Index<Car>("litresPer100Kilometers"))
          Console.WriteLine(c.ToStringDetails(session));
        Console.WriteLine("Vehicles ordered modelYear, brandName, modelName, color");
        foreach (Vehicle v in session.Index<Vehicle>())
          Console.WriteLine(v.ToStringDetails(session));
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        // these LINQ statements will trigger a binary search lookup (not a linear search) of the matching Car objects in the BTreeSet
        Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar).First();
        c.Color = "Green";
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        // these LINQ statements will trigger a binary search lookup (not a linear search) of the matching Car objects in the BTreeSet
        Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Green" select aCar).First();
        UInt64 id = c.Id;
        session.DeleteObject(id);
        session.Abort();
        session.BeginUpdate();
        session.DeleteObject(id);
        session.Commit();
      }

      using (var session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServerShared(systemDir))
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        session.BeginRead();
        // these LINQ statements will trigger a binary search lookup (not a linear search) of the matching Car objects in the BTreeSet
        Console.WriteLine("Blue Cars");
        foreach (Car c in (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar))
          Console.WriteLine(c.ToStringDetails(session));
        session.Commit();
        sw.Stop();
        Console.WriteLine(sw.Elapsed);
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (int i = 0; i < 10000; i++)
        { // add some junk to make search harder
          car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, i,
          odometer, registrationState, registrationPlate + i, insuranceCompany, insurancePolicy);
          session.Persist(car);
        }
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        // these LINQ statements will trigger a binary search lookup (not a linear search) of the matching Car objects in the BTreeSet
        Console.WriteLine("Blue Cars");
        foreach (Car c in (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar))
          Console.WriteLine(c.ToStringDetails(session));
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar).First();
        c.Unpersist(session);
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        foreach (Car c in session.Index<Car>())
          Console.WriteLine(c.ToStringDetails(session));
        Console.WriteLine("Blue Cars");
        foreach (Car c in (from aCar in session.Index<Car>() where aCar.Color == "Blue" select aCar))
          Console.WriteLine(c.ToStringDetails(session));
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        InsuranceCompany prior = insuranceCompany;
        try
        {
          for (int i = 0; i < 100000; i++)
          {
            insuranceCompany = new InsuranceCompany("AAA", "858787878");
            insuranceCompany.Persist(session, prior);
            prior = insuranceCompany;
          }
          Assert.IsTrue(false); // should not get here
        }
        catch (UniqueConstraintException)
        {
        }
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(InsuranceCompany)));
        var q = from company in session.Index<InsuranceCompany>("name", db) where company.Name == "AAA" select company;
        foreach (InsuranceCompany company in q)
          Console.WriteLine(company.ToStringDetails(session)); // only one will match
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        InsuranceCompany prior = insuranceCompany;
        try
        {
          for (int i = 0; i < 100000; i++)
          {
            insuranceCompany = new InsuranceCompany("AAA", "858787878");
            session.Persist(insuranceCompany);
          }
          Assert.IsTrue(false); // should not get here
        }
        catch (UniqueConstraintException)
        {
        }
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(InsuranceCompany)));
        var q = from company in session.Index<InsuranceCompany>("name", db) where company.Name == "AAA" select company;
        foreach (InsuranceCompany company in q)
          Console.WriteLine(company.ToStringDetails(session)); // only one will match
        bool exists = (from anEntry in session.Index<InsuranceCompany>("name", db) where anEntry.Name == "AAA" select 0).Any();
        Assert.IsTrue(exists);
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        try
        {
          for (int i = 0; i < 100000; i++)
          {
            insuranceCompany = new InsuranceCompanySpecial("AAA", "858787878");
            session.Persist(insuranceCompany);
          }
          Assert.IsTrue(false); // should not get here
        }
        catch (UniqueConstraintException)
        {
        }
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        try
        {
          for (int i = 0; i < 100000; i++)
          {
            insuranceCompany = new InsuranceCompanySpecial2("AAA", "858787878");
            session.Persist(insuranceCompany);
          }
          Assert.IsTrue(false); // should not get here
        }
        catch (UniqueConstraintException)
        {
        }
        session.Commit();
      }

      using (var session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServerShared(systemDir))
      {
        session.BeginRead();
        session.TraceIndexUsage = true;
        Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(InsuranceCompanySpecial)));
        var q = from company in session.Index<InsuranceCompany>("name", db) where company.Name == "AAA" select company;
        foreach (InsuranceCompany company in q)
          Console.WriteLine(company.ToStringDetails(session)); // only one will match
        bool exists = (from anEntry in session.Index<InsuranceCompanySpecial>("name", db) where anEntry.Name == "AAA" select 0).Any();
        Assert.IsTrue(exists);
        exists = (from anEntry in session.Index<InsuranceCompanySpecial>("name", db) where String.Equals(anEntry.Name, "AAA", StringComparison.OrdinalIgnoreCase) select anEntry).Any();
        Assert.IsTrue(exists);
        exists = (from anEntry in session.Index<InsuranceCompanySpecial>("name", db) where String.Equals(anEntry.Name, "AAA", StringComparison.OrdinalIgnoreCase) == true select anEntry).Any();
        Assert.IsTrue(exists);
        session.Commit();
      }
    }

    [TestCase(true)]
    public void IndexSample2(bool useServerSession)
    {
      Random rd = new Random();
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();

        for (int i = 0; i < 100; i++)
        {
          MeasurePoint p = new MeasurePoint(i + 1) { Data = new List<float>() };
          for (int j = 0; j < 440000; j++)
          {
            p.Data.Add(rd.Next(100));
          }
          session.Persist(p);
        }

        var value = session.DatabaseNumberOf(typeof(MeasurePoint));
        Database db = session.OpenDatabase(value, false, false);
        if (db != null)
        {
          var q = from point in session.Index<MeasurePoint>("key", db) where point.Key == 10 select point;
          foreach (MeasurePoint obj in q)
          {
            Console.WriteLine(obj.Key + " " + obj.Data.Count);
          }
        }
        session.Commit();
      }
    }

    [TestCase(false)]
    public void IOptimizedPersistableField(bool useServerSession)
    {
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (int i = 0; i < 10; i++)
        {
          var dict = new PersistableDynamicDictionary();
          session.Persist(dict);
        }
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        session.TraceIndexUsage = true;
        DateTime now = DateTime.UtcNow;
        var q = from dict in session.Index<PersistableDynamicDictionary>("m_creationTime") where dict.CreationTime < now && dict.CreationTime >= now.AddYears(-1) select dict;
        Assert.GreaterOrEqual(q.Count(), 10);
        q = from dict in session.Index<PersistableDynamicDictionary>("m_creationTime") where dict.CreationTime > DateTime.UtcNow.AddYears(-1) select dict;
        Assert.GreaterOrEqual(q.Count(), 10);
        session.Commit();
      }
    }

    [Test]
    public void MicaelGenerate()
    {
      int? size = null;
      int? seed = null;

      try
      {
        using (var ctx = new RelSandbox.Context(systemDir))
        {
          Generate(ctx, size ?? DefaultSize, seed.HasValue ? new Random(seed.Value) : new Random());
          ctx.Checkpoint();
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"*** {ex.Message}");
        Environment.ExitCode = 10;
      }

      Console.WriteLine("Done.");
    }


    [Test]
    [Repeat(2)]
    public void MicaelRelate()
    {
      try
      {
        using (var ctx = new RelSandbox.Context(systemDir))
        {
          Relate(ctx);
          ctx.Checkpoint();
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"*** {ex.Message}");
        Environment.ExitCode = 10;
      }

      Console.WriteLine("Done.");
    }

    static void Generate(RelSandbox.IContext ctx, int size, Random random)
    {
      GeneratePopulation(ctx, size, random);
      GenerateComms(ctx, size, random);
    }

    static void GeneratePopulation(RelSandbox.IContext ctx, int size, Random random)
    {
      //
      // Här skapar vi Person-artefakter och ger dem unika namn och slumpmässiga födelsedatum.
      //
      var max = (int)Math.Ceiling(Math.Pow(size / 2.0, 1.0 / 3.0));
      var cnt = 0;

      Console.WriteLine("Generating population...");
      Console.Write("0% ");

      for (int n1 = 0; n1 < max && cnt < size; ++n1)
      {
        for (int n2 = 0; n2 < max && cnt < size; ++n2)
        {
          for (int s = 0; s < max && cnt < size; ++s)
          {
            var m = ctx.AddPerson(RelSandbox.Data.MaleNames[n1], RelSandbox.Data.MaleNames[n2], RelSandbox.Data.SurNames[s], "M", GetBirthDate(random));
            var f = ctx.AddPerson(RelSandbox.Data.FemaleNames[n1], RelSandbox.Data.FemaleNames[n2], RelSandbox.Data.SurNames[s], "F", GetBirthDate(random));

            cnt += 2;

            if (100 * cnt % size < 100 * (cnt - 2) % size)
              Console.Write($"\r{100 * cnt / size}% (persons: {cnt}) ");
          }
        }
      }

      Console.WriteLine();
    }

    static void GenerateComms(RelSandbox.IContext ctx, int size, Random random)
    {
      //
      // Här skapar vi Communication-artefakter, dvs telefonsamtal, sms och mail, mellan Person-artefakter.
      // Vi skapar också Device-artefakter (mobiltelefoner) om det saknas, så att personerna har någonting att kommunicera via.
      // 
      var max = (int)Math.Ceiling(Math.Pow(size / 2.0, 1.0 / 3.0));
      int sms = 0;
      int calls = 0;
      int emails = 0;
      int devices = 0;

      Console.WriteLine("Generating communications...");
      Console.Write("0% ");

      for (var cnt = 1; cnt <= size; ++cnt)
      {
        // Slumpa fram en avsändare och en mottagare.
        var fn1 = random.Next(max);
        var fn2 = random.Next(max);
        var fs = random.Next(max);
        var fg = random.Next(2) == 0 ? "M" : "F";
        var tn1 = random.Next(max);
        var tn2 = random.Next(max);
        var ts = random.Next(max);
        var tg = random.Next(2) == 0 ? "M" : "F";

        if (fn1 == tn1 && fn2 == tn2 && fs == ts && fg == tg)
          continue;

        // Hitta personerna och skapa Device-artefakter åt dem om det saknas.
        var fNames = fg == "M" ? RelSandbox.Data.MaleNames : RelSandbox.Data.FemaleNames;
        var tNames = tg == "M" ? RelSandbox.Data.MaleNames : RelSandbox.Data.FemaleNames;
        var f = ctx.GetPersons(firstName: fNames[fn1], secondName: fNames[fn2], surName: RelSandbox.Data.SurNames[fs], gender: fg).FirstOrDefault();
        var t = ctx.GetPersons(firstName: tNames[tn1], secondName: tNames[tn2], surName: RelSandbox.Data.SurNames[ts], gender: tg).FirstOrDefault();

        if (f == null || t == null)
          continue;

        var fd = ctx.GetDevice(ownerUid: f.Uid);
        var td = ctx.GetDevice(ownerUid: t.Uid);

        if (fd == null)
        {
          fd = ctx.AddDevice(f.Uid);
          devices += 1;
        }

        if (td == null)
        {
          td = ctx.AddDevice(t.Uid);
          devices += 1;
        }

        // Lägg till ett antal samtal, sms och mail.
        int cc = random.Next(10) + 1;

        for (var ix = 0; ix < cc; ++ix)
        {
          int ct = random.Next(3);
          int sd = random.Next(336);
          int sh = random.Next(24);
          int sm = random.Next(60);
          int cd = random.Next(30);
          var st = new DateTime(2021, (sd / 28) + 1, (sd % 28) + 1, sh, sm, 0);
          var et = st.AddMinutes(cd);

          ctx.AddCommunication(CommType[ct], fd.Uid, td.Uid, st, et);

          calls += ct == 0 ? 1 : 0;
          sms += ct == 1 ? 1 : 0;
          emails += ct == 2 ? 1 : 0;
        }

        // Visa progressen.
        if (100 * cnt % size < 100 * (cnt - 1) % size)
          Console.Write($"\r{100 * cnt / size}% (devices: {devices} calls: {calls} sms: {sms} e-mails: {emails}) ");
      }

      Console.WriteLine();
    }

    static readonly string[] CommType = new[]
    {
            "Call",
            "SMS",
            "E-mail"
        };

    static DateTime GetBirthDate(Random random)
    {
      int y = random.Next(100) + 1920;
      int m = random.Next(12) + 1;
      int d = random.Next(28) + 1;

      return new DateTime(y, m, d);
    }

    void Relate(RelSandbox.Context ctx)
    {
      Unrelate(ctx);
      CreatePersonNodes(ctx, out var namesakes);
      RelateHouseholds(ctx, namesakes);
      RelateComms(ctx);

      // Kontrollera att allt ser rätt ut.
      var diff = Verify(ctx);

      if (diff.Any())
        throw new Exception($"{diff.Count} relations missing in index '_endNodeId'.");
    }

    static void Unrelate(RelSandbox.Context ctx)
    {
      //
      // Ta bort alla noder och relationer om det finns några.
      //
      Console.WriteLine("Deleting nodes...");
      ctx.DeleteNodes();
      Console.WriteLine("Deleting relations...");
      ctx.DeleteRelations();
    }

    static void CreatePersonNodes(RelSandbox.Context ctx, out Dictionary<string, List<RelSandbox.IArtifactPerson>> namesakes)
    {
      //
      // Här skapar vi Person-noder för alla Person-artefakter.
      // Under tiden bygger vi upp en Dictionary där vi kan hitta alla personer med samma NamesakeKey (efternamn).
      // 
      var pc = ctx.GetPersons().ToList();
      var cnt = 0;

      Console.WriteLine("Creating person nodes...");
      Console.Write("0% ");

      namesakes = new Dictionary<string, List<RelSandbox.IArtifactPerson>>();

      foreach (var p in pc)
      {
        var k = NamesakeKey(p);
        var l = namesakes.GetOrAdd(k, x => new List<IArtifactPerson>());

        l.Add(p);

        ctx.AddNode("Person", p.Uid);
        cnt += 1;

        // Visa progressen.
        if (100 * cnt % pc.Count < 100 * (cnt - 1) % pc.Count)
          Console.Write($"\r{100 * cnt / pc.Count}% (person nodes: {cnt}) ");
      }

      Console.WriteLine();
    }

    static void RelateHouseholds(RelSandbox.Context ctx, IDictionary<string, List<RelSandbox.IArtifactPerson>> namesakes)
    {
      //
      // Här "gifter vi ihop" vuxna kvinnor och män där namn och ålder är ungefär lika.
      // Det representeras av att vi skapar en Household-nod och relaterar Person-noderna till denna.
      // Om någon av personerna redan hör till en Household-nod så slår vi ihop dem till en gemensam.
      //
      //    Person-nod --LivesIn--> Household-nod <--LivesIn-- Person-nod
      //
      var today = new DateTime(1970, 1, 1);
      var hc = 0;
      var fc = ctx.GetPersons(gender: "F").ToList();
      var rc = 0;
      var cnt = 0;

      Console.WriteLine("Relating households...");

      foreach (var f in fc)
      {
        var k = NamesakeKey(f);
        var m = namesakes[k].FirstOrDefault(x => x.Gender == "M" && AgeDifference(f, x) <= 90);

        // Om kvinnan är minst 25 år gammal och vi hittat en matchande man, då gifter de sig (flyttar ihop).
        if (f.BirthDate.Year < 1995 && m != null)
        {
          // Försök hitta Person-noderna utifrån artefakt-id och eventuella Household-noder som de redan hör till.
          var fp = ctx.GetNode(what: "Person", artifactUid: f.Uid);
          var mp = ctx.GetNode(what: "Person", artifactUid: m.Uid);
          var fh = ctx.GetEndNodes(how: "LivesIn", startNodeId: fp.Id).SingleOrDefault();
          var mh = ctx.GetEndNodes(how: "LivesIn", startNodeId: mp.Id).SingleOrDefault();

          if (fh == null && mh == null)
          {
            // Både kvinnan och mannen är hemlösa.
            // De flyttar in i ett nytt hushåll.
            var h = ctx.AddNode("Household", 0);

            ctx.AddRelation("LivesIn", mp.Id, h.Id);
            ctx.AddRelation("LivesIn", fp.Id, h.Id);

            hc += 1;
            rc += 2;
          }
          else if (fh != null && mh != null)
          {
            // Både kvinnan och mannen hör redan till ett hushåll.
            // Alla i kvinnans hushåll flyttar in i mannens hushåll.
            foreach (var r in ctx.GetRelations(how: "LivesIn", endNodeId: fh.Id))
            {
              ctx.AddRelation("LivesIn", r.StartNodeId, mh.Id);
              ctx.DeleteRelation(r);
            }

            ctx.DeleteNode(fh);
            hc -= 1;
          }
          else if (fh != null)
          {
            // Mannen är hemlös, men kvinnan hör redan till ett hushåll.
            // Mannen flyttar in i kvinnans hushåll.
            ctx.AddRelation("LivesIn", mp.Id, fh.Id);
            rc += 1;
          }
          else
          {
            // Kvinnan är hemlös, men mannen hör redan till ett hushåll.
            // Kvinnan flyttar in i mannens hushåll.
            ctx.AddRelation("LivesIn", fp.Id, mh.Id);
            rc += 1;
          }
        }

        cnt += 1;

        // Visa progressen.
        if (100 * cnt % fc.Count < 100 * (cnt - 1) % fc.Count)
          Console.Write($"\r{100 * cnt / fc.Count}% (household nodes: {hc} relations: {rc}) ");
      }

      Console.WriteLine();
    }

    static void RelateComms(RelSandbox.Context ctx)
    {
      //
      // Artefakterna är redan skapade, dvs Person, Device och Communication. Person-noderna är också skapade.
      // Nu skapar vi Device- och Communication-noder samt tre typer av relationer - Owns, SendComm och RecvComm:
      // 
      //    Person-nod --Owns--> Device-nod
      //
      //    Device-nod --SendComm--> Communication-nod --RecvComm--> Device-nod
      //
      Console.WriteLine("Relating communication...");
      Console.Write("0% ");

      var ac = ctx.GetCommunications().ToList();
      var cnt = 0;
      var devs = 0;
      var comms = 0;
      var rels = 0;

      // För varje Communication-artefakt...
      foreach (var a in ac)
      {
        // Skapa en Communication-nod och försök hitta avsändande och mottagande Device-nod.
        var cn = ctx.AddNode("Communication", a.Uid);
        var fdn = ctx.GetNode(what: "Device", artifactUid: a.FromDevice);
        var tdn = ctx.GetNode(what: "Device", artifactUid: a.ToDevice);

        comms += 1;

        // Skapa avsändande Device-nod om den saknas samt en Owns-relation mellan ägarens Person-nod och Device-noden.
        if (fdn == null)
        {
          var d = ctx.GetDevice(uid: a.FromDevice);
          var pn = ctx.GetNode(artifactUid: d.OwnerUid);

          fdn = ctx.AddNode("Device", a.FromDevice);
          ctx.AddRelation("Owns", pn.Id, fdn.Id);
          devs += 1;
          rels += 1;

          // Kontrollera att vi kan hitta den nyskapade relationen genom Relation-index "_endNodeId".
          if (ctx.GetRelations(how: "Owns", endNodeId: fdn.Id).Count() != 1)
            throw new Exception("Newly added relation was not found by index '_endNodeId'.");
        }

        // Skapa mottagande Device-nod om den saknas samt en Owns-relation mellan ägarens Person-nod och Device-noden.
        if (tdn == null)
        {
          var d = ctx.GetDevice(uid: a.ToDevice);
          if (d == null)
            d = ctx.GetDevice(uid: a.ToDevice);
          var pn = ctx.GetNode(artifactUid: d.OwnerUid);

          tdn = ctx.AddNode("Device", a.ToDevice);
          ctx.AddRelation("Owns", pn.Id, tdn.Id);
          devs += 1;
          rels += 1;

          // Kontrollera att vi kan hitta den nyskapade relationen genom Relation-index "_endNodeId".
          if (ctx.GetRelations(how: "Owns", endNodeId: tdn.Id).Count() != 1)
            throw new Exception("Newly added relation was not found by index '_endNodeId'.");
        }

        // Skapa SendComm- och RecvComm-relationerna.
        ctx.AddRelation("SendComm", fdn.Id, cn.Id);
        ctx.AddRelation("RecvComm", cn.Id, tdn.Id);
        rels += 2;

        cnt += 1;

        // Visa progressen.
        if (100 * cnt % ac.Count < 100 * (cnt - 1) % ac.Count)
          Console.Write($"\r{100 * cnt / ac.Count}% (device nodes: {devs} communication nodes: {comms} relations: {rels}) ");
      }

      Console.WriteLine();
    }

    public static List<RelSandbox.Relation> Verify(RelSandbox.IContext Ctx)
    {
      Console.WriteLine("Verifying...");
      //
      // Kontrollera att vi kör 10.1.0.0 av VelocityDB och VelocityDBExtensions.
      //
      //var version = new Version(10, 1, 0, 0);
      //var vdbName = typeof(VelocityDb.Database).Assembly.GetName();
      //var extName = typeof(VelocityDBExtensions.Sync).Assembly.GetName();
      int devices = 0;
      int owners = 0;
      int foundByEndIndex = 0;
      int foundByStartIndex = 0;
      var diff = new List<RelSandbox.Relation>();

      //if (vdbName.Version != version)
      //  throw new Exception($"Version of VelocityDB is not {version}.");

      //if (extName.Version != version)
      //  throw new Exception($"Version of VelocityDBExtensions is not {version}.");

      //
      // Kontrollera att varje Device-nod ägs av en Person-nod, dvs det finns en Owns-relation mellan dem.
      //
      foreach (var dn in Ctx.GetNodes(what: "Device"))
      {
        // Kontrollera att vi kan hitta Device-artefakten utifrån Device-noden.
        var d = Ctx.GetDevice(uid: dn.ArtifactUid);

        if (d != null)
          devices += 1;
        else
          throw new Exception($"Device artifact {dn.ArtifactUid} was not found.");

        // Kontrollera att vi kan hitta Person-noden (ägaren) via artefakt-id.
        var pn = Ctx.GetNode(what: "Person", artifactUid: d.OwnerUid);

        if (pn != null)
          owners += 1;
        else
          throw new Exception($"Person node for artifact {d.OwnerUid} was not found.");

        // Kontrollera att vi kan hitta Owns-relationen via Device-noden (sökning via Relation-index "_endNodeId").
        var r1 = Ctx.GetRelations(how: "Owns", endNodeId: dn.Id).SingleOrDefault();
        if (r1 != null)
          foundByEndIndex += 1;

        // Kontrollera att vi kan hitta Owns-relationen via Person-noden (sökning via Relation-index "_startNodeId").
        var r2 = Ctx.GetRelations(how: "Owns", startNodeId: pn.Id).SingleOrDefault();
        if (r2 != null)
          foundByStartIndex += 1;

        // Om Owns-relationen bara kunde hittas via ett Relation-index, lägg den till diff-listan.
        if (r1 == null && r2 != null)
          diff.Add(r2);

        if (r1 != null && r2 == null)
          diff.Add(r1);

        // Om Owns-relationen saknas i båda Relation-index, kasta ett undantag.
        if (r1 == null && r2 == null)
          throw new Exception($"Owns relation between Person node {pn.Id} and Device node {dn.Id} was not found.");

        // Kontrollera att BTreeSet för Relation-index "_endNodeId" är rätt sorterad och att det innehåller en Owns-relation för Device-noden.
        //Ctx.VerifyRelationEndIndex(dn.Id);
      }

      return diff;
    }
    static string NamesakeKey(RelSandbox.IArtifactPerson person)
    {
      return $"{person.SurName}";
    }

    static double AgeDifference(RelSandbox.IArtifactPerson p1, RelSandbox.IArtifactPerson p2)
    {
      return Math.Abs((p1.BirthDate - p2.BirthDate).TotalDays);
    }

    static TimeSpan Time(Action action)
    {
      var t0 = DateTime.Now;

      action();

      return DateTime.Now - t0;
    }
  }
}
