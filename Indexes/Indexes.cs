using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDbSchema.Indexes;

namespace Indexes
{
  class Indexes
  {
    static readonly string s_licenseDbFile = "c:/4.odb";
    static readonly string s_systemDir = "Indexes"; // appended to SessionBase.BaseDatabasePath

    static void Main(string[] args)
    {
      try
      {
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
        InsuranceCompany insuranceCompany = new InsuranceCompany("Allstate", "858727878");
        Car car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
          odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        { // cleanup data from a possible prior run
          session.BeginUpdate();
          foreach (Database db in session.OpenAllDatabases(true))
            if (db.DatabaseNumber >= 10 || db.DatabaseNumber == SessionBase.IndexDescriptorDatabaseNumber)
              session.DeleteDatabase(db);
          session.Commit();
          File.Copy(s_licenseDbFile, Path.Combine(session.SystemDirectory, "4.odb"));
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          //session.AddToIndexInSeperateThread = false;
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
          modelYear = new DateTime(2006, 1, 1);
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
          modelYear = new DateTime(2001, 1, 1);
          brandName = "Lincoln";
          modelName = "Town Car";
          maxSpeed = 220;
          odometer = 250000;
          registrationState = "Texas";
          registrationPlate = "TX543433";
          insurancePolicy = "CAA7878775";
          car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, maxSpeed,
            odometer, registrationState, registrationPlate, insuranceCompany, insurancePolicy);
          session.Persist(car);
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
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
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.TraceIndexUsage = true;
          session.BeginUpdate();
          // these LINQ statements will trigger a binary search lookup (not a linear search) of the matching Car objects in the BTreeSet
          Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar).First();
          c.Color = "Green";
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.TraceIndexUsage = true;
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
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.TraceIndexUsage = true;
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
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginUpdate();
          for (int i = 0; i < 10000; i++)
          { // add some junk to make search harder
            car = new Car(color, maxPassengers, fuelCapacity, litresPer100Kilometers, modelYear, brandName, modelName, i,
            odometer, registrationState, registrationPlate + i, insuranceCompany, insurancePolicy);
            session.Persist(car);
          }
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.TraceIndexUsage = true;
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
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginUpdate();
          Car c = (from aCar in session.Index<Car>("color") where aCar.Color == "Blue" select aCar).First();
          c.Unpersist(session);
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginRead();
          foreach (Car c in session.Index<Car>())
            Console.WriteLine(c.ToStringDetails(session));
          Console.WriteLine("Blue Cars");
          foreach (Car c in (from aCar in session.Index<Car>() where aCar.Color == "Blue" select aCar))
            Console.WriteLine(c.ToStringDetails(session));
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginUpdate();
          InsuranceCompany prior = insuranceCompany;
          try
          {
            for (int i = 0; i < 100000; i++)
            {
              insuranceCompany = new InsuranceCompany("AAA", "858787878");
              session.Persist(insuranceCompany);
            }
            Debug.Assert(false); // should not get here
          }
          catch (UniqueConstraintException)
          {
          }
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.TraceIndexUsage = true;
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginRead();
          Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(InsuranceCompany)));
          var q = from company in session.Index<InsuranceCompany>("name", db) where company.Name == "AAA" select company;
          foreach (InsuranceCompany company in q)
            Console.WriteLine(company.ToStringDetails(session)); // only one will match
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }

        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginUpdate();
          Customer joe = new Customer("Joe");
          session.Persist(joe);
          Customer karim = new Customer("Karim");
          session.Persist(karim);
          Customer tony = new Customer("Tony");
          session.Persist(tony);
          Customer wayne = new Customer("Wayne");
          session.Persist(wayne);
          Order order = new Order(joe);
          Payment payment = new Payment(order);
          order = new Order(karim);
          payment = new Payment(order);
          payment = new Payment(order);
          payment = new Payment(order);
          order = new Order(tony);
          payment = new Payment(order);
          payment = new Payment(order);
          order = new Order(wayne);
          payment = new Payment(order);
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }

        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.TraceIndexUsage = true;
          Stopwatch sw = new Stopwatch();
          sw.Start();
          session.BeginRead();
          var karim = (from customer in session.Index<Customer>("m_name") where customer.Name == "Karim" select customer).FirstOrDefault();
          var karimOrders = karim.Orders;
          var karimOrderFromOrders = (from order in session.Index<Order>("m_customer") where order.Customer == karim select order).ToArray();
          session.Commit();
          sw.Stop();
          Console.WriteLine(sw.Elapsed);
        }

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}
