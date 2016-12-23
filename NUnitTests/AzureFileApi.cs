using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDBExtensions2;
using VelocityDbSchema.Samples.AllSupportedSample;

namespace NUnitTests
{
  [TestFixture]
  public class AzureFileApi
  {
    [Test]
    public void Azure()
    {
      UInt64 id = 42949738497;
      AllSupported allSuported, allSupported2;
      string connectionString = null;
      using (StreamReader sr = new StreamReader("c:/AzureConnectionString.txt"))
      {
        connectionString = sr.ReadToEnd();
      }

      // A better way of using Azure files is to mount cloud directory as a local drive.
      // Such as: net use f: \\veleocitydb.file.core.windows.net\azure /u:veleocitydb [access key]
      // Add access key and update for your case.
      // Then you can use the mounted Azure directory just like you use any local drive!
      /*using (SessionBase session = new SessionNoServer("F:/azure", 99999, false))
      {
        session.BeginUpdate();
        allSuported = new AllSupported(3);
        allSuported.Persist(session, allSuported);
        id = allSuported.Id;
        session.Commit();
      }

      using (SessionNoServer session = new SessionNoServer("F:/azure", 99999, false))
      {
        session.BeginRead();
        allSupported2 = (AllSupported)session.Open(id);
        session.Commit();
      }*/

      using (SessionBase session = new AzureSession(connectionString, "azure", "azure", 99999, false))
      {
        session.BeginUpdate();
        allSuported = new AllSupported(3);
        allSuported.Persist(session, allSuported);
        id = allSuported.Id;
        session.Commit();
      }

      using (SessionNoServer session = new AzureSession(connectionString, "azure", "azure", 99999, false))
      {
        session.BeginRead();
        allSupported2 = (AllSupported)session.Open(id);
        Assert.NotNull(allSupported2);
        session.Commit();
      }

     /* Not yet working. Challenge is that an Azure Stream can only be Read only or Update only - not both. Another challenge is required calls to Flush() and resizing of files have to be explicit.
      * 
      * using (SessionBase session = new AzureSession(connectionString, "azure", "azure", 99999, true))
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
      }*/
    }
  }
}
