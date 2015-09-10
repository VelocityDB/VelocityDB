using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class DatabaseTest
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    [Test]
    public void aCreateDatabases()
    {
      Database database;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50001000; i++)
        {
          database = session.NewDatabase(i);
          Assert.NotNull(database);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50001000; i++)
        {
          database = session.OpenDatabase(i);
          Assert.NotNull(database);
        }
        session.Commit();
      }
    }

    [Test]
    public void bServerIterateDatabases()
    {
      Database database;
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginRead();
        for (uint i = 50000000; i < 50001000; i++)
          database = session.OpenDatabase(i);
        session.Commit();
      }
    }

    [Test]
    public void b2ServerIterateDatabases()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginRead();
        session.OpenAllDatabases();
        session.Commit();
      }
    }

    [Test]
    public void cDeleteDatabases()
    {
      Database database;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50001000; i++)
        {
          database = session.OpenDatabase(i);
          session.DeleteDatabase(database);
        }
        session.Commit();
      }
    }

    [Test]
    public void dServerIterateDatabases()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginRead();
        session.OpenAllDatabases();
        session.Commit();
      }
    }

    [Test]
    public void eCreateDatabasesPreSize()
    {
      Database database;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50000100; i++)
        {
          database = session.NewDatabase(i, i - 50000000);
          Assert.NotNull(database);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50000100; i++)
        {
          database = session.OpenDatabase(i);
          Assert.NotNull(database);
        }
        session.Commit();
      }
    }

    [Test]
    public void fCreateDatabasesPreSizeServer()
    {
      Database database;
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000100; i < 50000200; i++)
        {
          database = session.NewDatabase(i, i - 50000100, "myServerSetDbName");
          Assert.NotNull(database);
        }
        session.Commit();
      }
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50000200; i++)
        {
          database = session.OpenDatabase(i);
          Assert.NotNull(database);
        }
        session.Commit();
      }
    }

    [Test]
    public void gDeleteDatabasesServer()
    {
      Database database;
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000000; i < 50000100; i++)
        {
          database = session.OpenDatabase(i);
          session.DeleteDatabase(database);
        }
        session.Commit();
      }
    }
    
    [Test]
    public void hDeleteDatabases()
    {
      Database database;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (uint i = 50000100; i < 50000200; i++)
        {
          database = session.OpenDatabase(i);
          session.DeleteDatabase(database);
        }
        session.Commit();
      }
    }
  }
}
