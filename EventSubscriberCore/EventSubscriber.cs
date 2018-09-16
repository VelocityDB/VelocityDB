// This sample console application demonstrates the usage of the event subscription ServerClientSession api. Get notified when other sessions changes objects your session is intrested in.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema;

namespace EventSubscriber
{
  class EventSubscriber
  {
    static readonly string s_systemDir = "EventSubscriberCore"; 

    static void Main(string[] args)
    {
      using (ServerClientSession session = new ServerClientSession(s_systemDir))
      {
        session.BeginUpdate();
        session.RegisterClass(typeof(Woman));
        session.RegisterClass(typeof(Man));
        session.SubscribeToChanges(typeof(Person));
        session.SubscribeToChanges(typeof(Woman), "OlderThan50");
        Person robinHood = new Person("Robin", "Hood", 30, 1234, null, null);
        session.Persist(robinHood);
        Person billGates = new Person("Bill", "Gates", 56, 234, robinHood, null);
        session.Persist(billGates);
        Person steveJobs = new Person("Steve", "Jobs", 56, 456, billGates, null);
        session.Persist(steveJobs);
        session.Commit();
        Thread t = new Thread(UpdaterThread);
        t.Start();
        Thread.Sleep(600);
        
        for (int i = 0; i < 50; i++)
        {
          List<Oid> changes = session.BeginReadWithEvents();
          if (changes.Count == 0)
          {
            Console.WriteLine("No changes events at: " + DateTime.Now.ToString("HH:mm:ss:fff"));
            Thread.Sleep(250);
          }
          foreach (Oid id in changes)
          {
            object obj = session.Open(id);
            Console.WriteLine("Received change event for: " + obj + " at: " + DateTime.Now.ToString("HH:mm:ss:fff"));;
            //session.UnsubscribeToChanges(typeof(Person));
          }
          Console.WriteLine();
          session.Commit();
        }       
        t.Join();
      }
    }

    static void UpdaterThread()
    {
      using (ServerClientSession session = new ServerClientSession(s_systemDir))
      {
        session.BeginUpdate();
        Person Mats = new Person("Mats", "Persson", 22, 1234, null, null);
        session.Persist(Mats);
        Woman Kinga = new Woman("Kinga", "Persson", 56, 234, null, Mats);
        foreach (Person p in session.AllObjects<Person>(true))
        {
          p.Age = (ushort) (p.Age + 1); 
        }
        session.Persist(Kinga);
        session.Commit();
// 5 events
        Thread.Sleep(5000);

        session.BeginUpdate();
        Woman Lidia = new Woman("Lidia", "Persson", 22, 1234, null, null);
        session.Persist(Lidia);
        session.Commit();
// 0 events 
        Thread.Sleep(500);

        session.BeginUpdate();
        Woman Margareta = new Woman("Margareta", "Persson", 98, 1234, null, null);
        session.Persist(Margareta);
        session.Commit();
// 1 event
        Thread.Sleep(500);

        session.BeginUpdate();
        Woman Oliwia = new Woman("Oliwia", "Persson", 50, 1234, null, null);
        session.Persist(Oliwia);
        session.Commit();
// 0 events
        Thread.Sleep(500);

        session.BeginUpdate();
        foreach (Woman p in session.AllObjects<Woman>())
        {
          p.Age = (ushort) (p.Age + 1); 
        }
        session.Commit();
// 3 events
        session.BeginUpdate();
        foreach (Woman p in session.AllObjects<Woman>())
        {
          p.Age = (ushort)(p.Age + 1);
        }
        session.Commit();
// 3 events
      }
    }
  }
}
