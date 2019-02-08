using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VelocityDb.Session;
using VelocityDb;
using System.Diagnostics;
using VelocityDb.Collection;
using System.IO;

namespace NUnitTests
{
  [TestFixture]

  public class RecoveryTests
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    public void Recover1(SessionBase session)
    {
      Database db = null;
      session.BeginUpdate();
      session.RegisterClass(typeof(SortedSetAny<int>));
      session.RegisterClass(typeof(SortedSetAny<float>));
      db = session.OpenDatabase(88, true, false);
      if (db != null)
        session.DeleteDatabase(db);
      db = session.OpenDatabase(89, true, false);
      if (db != null)
        session.DeleteDatabase(db);
      session.Commit();
      session.BeginUpdate();
      db = session.NewDatabase(88);
      session.FlushUpdates();
      session.Abort();
      session.BeginUpdate();
      db = session.NewDatabase(88);
      SortedSetAny<float> floatSet;
      Oid floatSetOid;
      string dbPath = System.IO.Path.Combine(systemDir, "89.odb");
      Placement place = new Placement(88);
      for (int i = 0; i < 10; i++)
      {
        floatSet = new SortedSetAny<float>();
        floatSet.Persist(place, session);
        floatSetOid = floatSet.Oid;
      }
      db = session.NewDatabase(89);
      session.Commit();
      File.Delete(dbPath);
      session.BeginUpdate();
      db = session.NewDatabase(89);
      session.Commit();
      FileInfo info = new FileInfo(dbPath);
      info.CopyTo(dbPath + "X");
      session.BeginUpdate();
      SortedSetAny<int> intSet;
      place = new Placement(89);
      for (int i = 0; i < 10; i++)
      {
        intSet = new SortedSetAny<int>();
        intSet.Persist(place, session);
      }
      db = session.OpenDatabase(88);
      var list = db.AllObjects<SortedSetAny<float>>();
      foreach (SortedSetAny<float> set in list)
        set.Unpersist(session);
      db = session.OpenDatabase(89);
      session.Commit();
      intSet = null;
      db = null; // release refs so that cached data isn't stale
      File.Delete(dbPath);
      info = new FileInfo(dbPath + "X");
      info.MoveTo(dbPath);
      session.BeginUpdate();
      intSet = (SortedSetAny<int>)session.Open(89, 1, 1, false);
      Debug.Assert(intSet == null);
      object o = session.Open(88, 1, 1, false);
      floatSet = (SortedSetAny<float>)o;
      Debug.Assert(floatSet != null);
      session.Checkpoint();
      db = session.OpenDatabase(88);
      session.DeleteDatabase(db);
      db = session.OpenDatabase(89);
      session.DeleteDatabase(db);
      session.Commit();
    }

    [Test]
    public void Recover1()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Recover1(session);
      }
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        Recover1(session);
      }
    }
  }
}
