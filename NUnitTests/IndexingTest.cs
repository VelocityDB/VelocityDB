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
namespace NUnitTests
{
  [TestFixture]
  public class IndexingTest
  {
    public const string systemDir = @"d:/IndexingTestDbs";
    static readonly string licenseDbFile = @"D:/4.odb";

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
      using (SessionBase session = useServerSession ? (SessionBase) new ServerClientSession(systemDir) : (SessionBase) new SessionNoServer(systemDir))
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
            throw new Exception("Exception occurred while commiting records: " + ex.Message);
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

    void CreateDirectoryAndCopyLicenseDb()
    {
      try
      {
        if (Directory.Exists(systemDir) == false)
          Directory.CreateDirectory(systemDir);
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"), true);
      }
      catch (DirectoryNotFoundException)
      {
        Directory.CreateDirectory(systemDir);
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"), true);
      }
    }
    [Test]
    public void CleanupPriorRun()
    {
      CreateDirectoryAndCopyLicenseDb();
      SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;
    }

    [TestCase(true)]
    [TestCase(false)]
    public void IndexSample(bool useServerSession)
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

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
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
        CreateDirectoryAndCopyLicenseDb();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
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
        session.Commit();
        session.BeginUpdate();
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
        session.Commit();
        session.BeginUpdate();
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
        Console.WriteLine("Cars in fuel efficierncy order");
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
        // these LINQ statements will trigger a binary search lookup (not a linear serach) of the matching Car objects in the BTreeSet
        Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar).First();
        c.Color = "Green";
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        // these LINQ statements will trigger a binary search lookup (not a linear serach) of the matching Car objects in the BTreeSet
        Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Green" select aCar).First();
        UInt64 id = c.Id;
        session.DeleteObject(id);
        session.Abort();
        session.BeginUpdate();
        session.DeleteObject(id);
        session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        session.BeginRead();
        // these LINQ statements will trigger a binary search lookup (not a linear serach) of the matching Car objects in the BTreeSet
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
        // these LINQ statements will trigger a binary search lookup (not a linear serach) of the matching Car objects in the BTreeSet
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

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(InsuranceCompanySpecial)));
        var q = from company in session.Index<InsuranceCompany>("name", db) where company.Name == "AAA" select company;
        foreach (InsuranceCompany company in q)
          Console.WriteLine(company.ToStringDetails(session)); // only one will match
        bool exists = (from anEntry in session.Index<InsuranceCompanySpecial>("name", db) where anEntry.Name == "AAA" select 0).Any();
        Assert.IsTrue(exists);
        session.Commit();
      }
    }

    [TestCase(true)]
    public void IndexSample2(bool useServerSession)
    {
      CreateDirectoryAndCopyLicenseDb();
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
  }
}
