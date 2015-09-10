using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema;

namespace NUnitTests
{
  [TestFixture]
  public class ElectionTest
  {
    public const string systemDir = "c:\\NUnitTestDbs";

    /*public bool SaveElection(Guid InstalationID, string Jurisdiction, string APSGroup, AbsenteeProcessingSystem.Election ElEcTiOn)
    {
      bool Result = false;

      //'Check to see if the supporting data is a matching file
      if (ElEcTiOn == null)
      {
        return false;
      }

      string DatabasePath = HostingEnvironment.MapPath("/App_Data/") + Jurisdiction + "-" + APSGroup;
      DirectoryInfo DBDirInfo = new DirectoryInfo(DatabasePath);

      bool CreatedFromScratch = false;


      if (DBDirInfo.Exists)
      {
      }
      else
      {
        DBDirInfo.Create();
        CreatedFromScratch = true;
      }

      string FullVelocityPath = DBDirInfo.FullName + "\\SyncVelocitydb";
      SessionNoServer TmpAPSSyncSession = new SessionNoServer(FullVelocityPath);
      APSSynchData TmpSynchronizationData = new APSSynchData();

      if (CreatedFromScratch)
      {
        TmpAPSSyncSession.BeginUpdate();

        TmpSynchronizationData.Persist(TmpAPSSyncSession, TmpSynchronizationData);
        SaveToDisk("4.odb", DBDirInfo.FullName + "\\SyncVelocitydb\\4.odb");

        SyncElection ElectionToAdd = new SyncElection();
        ElectionToAdd.ElectionDate = ElEcTiOn.ElectionDate;
        ElectionToAdd.ElectionID = ElEcTiOn.GUID;
        ElectionToAdd.ElectionName = ElEcTiOn.Name;
        ElectionToAdd.Group = APSGroup;
        ElectionToAdd.Jurisdiction = Jurisdiction;
        ElectionToAdd.SyncedElection = ElEcTiOn;
        ElectionToAdd.ConnectedClients = new VelocityDbList<ConnectedClient>();
        ConnectedClient newConnectedClient = new ConnectedClient();
        newConnectedClient.ElectionID = ElectionToAdd.ElectionID;
        newConnectedClient.InstalationID = InstalationID;
        newConnectedClient.Group = APSGroup;
        newConnectedClient.Jurisdiction = Jurisdiction;
        newConnectedClient.LastReceivedActionTime = DateTime.Now;
        newConnectedClient.ListOfActionsReceivedFromClient = new VelocityDbList<AbsenteeProcessingSystem.APSAction>();
        newConnectedClient.ListOfActionsSentToClient = new VelocityDbList<Guid>();
        ElectionToAdd.ConnectedClients.Add(newConnectedClient);

        ElectionToAdd.Persist(TmpAPSSyncSession, ElectionToAdd);
        TmpSynchronizationData.SyncedElections.Add(ElectionToAdd);
        TmpAPSSyncSession.Commit();
        TmpAPSSyncSession.Close();

        return true;
      }
      else
      {
        SyncElection ElectionToAdd = new SyncElection();
        ElectionToAdd.ElectionDate = ElEcTiOn.ElectionDate;
        ElectionToAdd.ElectionID = ElEcTiOn.GUID;
        ElectionToAdd.ElectionName = ElEcTiOn.Name;
        ElectionToAdd.Group = APSGroup;
        ElectionToAdd.Jurisdiction = Jurisdiction;
        ElectionToAdd.SyncedElection = ElEcTiOn;
        ElectionToAdd.ConnectedClients = new VelocityDbList<ConnectedClient>();
        ConnectedClient newConnectedClient = new ConnectedClient();
        newConnectedClient.ElectionID = ElectionToAdd.ElectionID;
        newConnectedClient.InstalationID = InstalationID;
        newConnectedClient.Group = APSGroup;
        newConnectedClient.Jurisdiction = Jurisdiction;
        newConnectedClient.LastReceivedActionTime = DateTime.Now;
        newConnectedClient.ListOfActionsReceivedFromClient = new VelocityDbList<AbsenteeProcessingSystem.APSAction>();
        newConnectedClient.ListOfActionsSentToClient = new VelocityDbList<Guid>();
        ElectionToAdd.ConnectedClients.Add(newConnectedClient);
        TmpAPSSyncSession.BeginUpdate();
        TmpSynchronizationData = new APSSynchData();

        try
        {
          TmpSynchronizationData = (APSSynchData)TmpAPSSyncSession.Open(APSSynchData.PlaceInDatabase, 1, 1, true);
        }
        catch (Exception ex)
        {
          return false;
        }

        ElectionToAdd.Persist(TmpAPSSyncSession, ElectionToAdd);
        TmpSynchronizationData.SyncedElections.Add(ElectionToAdd);
        TmpAPSSyncSession.Commit();

        return true;

      }
      return false;
    }*/
    [Test]
    public void test()
    {
      ServerClientSession VelocityServerSession = new ServerClientSession(systemDir, "localhost");
      Woman TmpSynchronizationData = new Woman();//APSSynchData TmpSynchronizationData = default(APSSynchData);     
       // APSSynchData();
      //TmpSynchronizationData.SyncedElections = new VelocityDbList<Man>();
      VelocityServerSession.BeginUpdate();
      TmpSynchronizationData.Persist(VelocityServerSession, TmpSynchronizationData);
      VelocityServerSession.Commit();
      //SaveToDisk("4.odb", DatabasePath + "\\4.odb");
      VelocityServerSession.BeginUpdate();
    }
  }
}
