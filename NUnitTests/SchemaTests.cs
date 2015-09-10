using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using VelocityDbSchema.Samples.AllSupportedSample;
using NUnit.Framework;
using System.Collections;

namespace NUnitTests
{
  [TestFixture]
  public class SchemaTests
  {
    public const string systemDirCvsImport = @"c:\NUnitTestDbs\CsvImport";
    public const string systemDir = "c:\\NUnitTestDbs";
    public const string csvExportDir = @"c:\NUnitTestDbs\CsvExport";
    public string systemHost = Dns.GetHostName();

    [Test]
    public void CsvExport()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.Compact();
        session.BeginRead();
        session.ExportToCSV(csvExportDir);
        session.Commit();
      }
    }

    public void Verify(string dir)
    {
      using (SessionNoServer session = new SessionNoServer(dir))
      {
        session.BeginRead();
        session.Verify();
        session.Commit();
      }
    }

    [Test]
    public void CsvImport()
    {
      using (SessionNoServer session = new SessionNoServer(systemDirCvsImport))
      {
        session.BeginUpdate();
        session.ImportFromCSV(csvExportDir);
        session.Commit();
      }
      Verify(systemDirCvsImport);
    }

    [Test]
    [ExpectedException(typeof(OptimisticLockingFailed))]
    public void schemaUpdateMultipleSessions()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir))
      {
        session.BeginUpdate();
        Placement place = new Placement(555, 1, 1, 10, 10);
        Simple1 s1 = new Simple1(1);
        s1.Persist(place, session);
        s1 = null;
        using (ServerClientSession session2 = new ServerClientSession(systemDir))
        {
          Placement place2 = new Placement(556, 1, 1, 10, 10);
          session2.BeginUpdate();
          Simple2 s2 = new Simple2(2);
          s2.Persist(place2, session2);
          s2 = null;
          session.Commit();
          session2.Commit(); // optemistic locking will fail due to session2 working with a stale schema (not the one updated by session 1)
          session.BeginUpdate();
          s1 = (Simple1)session.Open(555, 1, 1, false);
          s2 = (Simple2)session.Open(556, 1, 1, false);
          session.Commit();
          session2.BeginUpdate();
          s1 = (Simple1)session2.Open(555, 1, 1, false);
          s2 = (Simple2)session2.Open(556, 1, 1, false);
          session2.Commit();
        }
      }
    }
  }
}
