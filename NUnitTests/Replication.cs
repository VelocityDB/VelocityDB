using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Session;

namespace NUnitTests
{
  [TestFixture]
  class Replication
  {
    public string s_systemHost = null;
    public string s_systemHost2 = "FindPriceBuy";

    [Test]
    public void HighAvalailabiltyByReplication()
    {
      var alternateSystemBoot = new List<ReplicaInfo> { new ReplicaInfo { Path = "Replica1" }, new ReplicaInfo { Path = "Replica2" } };
      var p1 = SessionBase.BaseDatabasePath + "/Replica1";
      var p2 = SessionBase.BaseDatabasePath + "/Replica2";
      var p3 = SessionBase.BaseDatabasePath + "/Replica3";
      var p3remote = $"\\{s_systemHost2}/databases/Replica3";
      if (Directory.Exists(p1))
        Directory.Delete(p1, true);
      if (Directory.Exists(p2))
        Directory.Delete(p2, true);
      if (Directory.Exists(p3))
        Directory.Delete(p3, true);
      if (Directory.Exists(p3remote))
        Directory.Delete(p3remote, true);

      using (var session = new ServerClientSession(alternateSystemBoot))
      {
        session.BeginUpdate();
        for (int i = 0; i < 100; i++)
        {
          var s = i.ToString();
          session.Persist(s);
        }
        session.Commit();
      }

      alternateSystemBoot = new List<ReplicaInfo> { new ReplicaInfo { Path = "Replica1" }, new ReplicaInfo { Path = "Replica2" }, new ReplicaInfo { Path = "Replica3", Host = s_systemHost2 } };
      using (var session = new ServerClientSession(alternateSystemBoot))
      {
        session.BeginUpdate();
        for (int i = 0; i < 100; i++)
        {
          var s = i.ToString();
          session.Persist(s);
        }
        session.Commit();
      }

      using (var session = new ServerClientSession(alternateSystemBoot))
      {
        session.BeginUpdate();
        for (int i = 0; i < 10; i++)
        {
          var s = i.ToString();
          session.Persist(s);
        }
        if (Directory.Exists(p2))
          Directory.Delete(p2, true);
        session.Commit();
      }

      using (var session = new ServerClientSession(alternateSystemBoot))
      {
        session.BeginRead();
        foreach (var s in session.AllObjects<string>())
          Console.WriteLine(s);
        session.Commit();
      }

      using (var session = new ServerClientSession(alternateSystemBoot))
      {
        session.BeginUpdate();
        for (int i = 0; i < 10; i++)
        {
          var s = i.ToString();
          session.Persist(s);
          if (Directory.Exists(p2))
            Directory.Delete(p2, true);
        }
        session.Commit();
      }

      using (var session = new ServerClientSession(alternateSystemBoot))
      {
        session.BeginRead();
        foreach (var s in session.AllObjects<string>())
          Console.WriteLine(s);
        session.Commit();
      }

      using (var session = new SessionNoServer("Replica1"))
      {
        session.Verify();
      }
      using (var session = new SessionNoServer("Replica2"))
      {
        session.Verify();
      }
      using (var session = new ServerClientSession("Replica3", s_systemHost2))
      {
        session.Verify();
      }
    }
  }
}
