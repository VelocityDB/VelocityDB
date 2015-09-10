using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDbSchema.OneDbPerClass;

namespace OneDbPerClass
{
  class OneDbPerClass
  {
    static readonly string s_systemDir = "OneDbPerClass"; // appended to SessionBase.BaseDatabasePath

    public void AddRaceCar()
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          session.BeginUpdate();
          RaceCar raceCar = new RaceCar();
          raceCar.Speed = 1976.7;
          session.Persist(raceCar);
          session.Commit();
        }      
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }    
    
    public void AddSomeBicycles()
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginUpdate();
          for (int i = 0; i < 1000000; i++)
          {
            Bicycle bicycle = new Bicycle();
            bicycle.Color = "red";
            session.Persist(bicycle);
          }
          for (int i = 0; i < 10; i++)
          {
            Bicycle bicycle = new Bicycle();
            bicycle.Color = "blue";
            session.Persist(bicycle);
          }
          for (int i = 0; i < 10; i++)
          {
            Bicycle bicycle = new Bicycle();
            bicycle.Color = "yellow";
            session.Persist(bicycle);
          }
          session.Commit();
        }      
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }    
    
    public void QuerySomeBicycles()
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginRead();
          Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(Bicycle)));
          Bicycle b1 = db.AllObjects<Bicycle>().ElementAt(50005);
          Bicycle b2 = db.AllObjects<Bicycle>().ElementAt<Bicycle>(50005);
          if (b1 != b2)
            throw new UnexpectedException("b1 != b2");
          var src = from Bicycle bike in db.AllObjects<Bicycle>() where bike.Color == "blue" select bike;
          foreach (Bicycle bike in src)
            Console.WriteLine(bike.ToStringDetails(session));
          session.Commit();
        }      
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    static int Main(string[] args)
    {
      OneDbPerClass oneDbPerClass = new OneDbPerClass();
      oneDbPerClass.AddRaceCar();
      oneDbPerClass.AddSomeBicycles();
      oneDbPerClass.QuerySomeBicycles();
      return 0;
    }
  }
}
