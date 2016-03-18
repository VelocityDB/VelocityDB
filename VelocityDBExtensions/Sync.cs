using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Sync;

namespace VelocityDBExtensions
{
  public static class Sync
  {
    static public void SyncWith(this SessionBase sessionToUpdate, SessionBase sessionOther)
    {
      SyncWith(sessionToUpdate, sessionOther, (session, version, change) => false);
    }

    static public void SyncWith(this SessionBase sessionToUpdate, SessionBase sessionToRead, Func<SessionBase, UInt64, Change, bool> doUpdate)
    {
      UInt64 currentVersion;
      UInt64 pageToReadVersion;
      bool conflictFound = false;
      using (var reader = sessionToRead.BeginRead())
      {
        Changes changes = (Changes)sessionToRead.Open(5, 1, 1, false);
        if (changes != null)
        {
          using (var updater = sessionToUpdate.BeginUpdate())
          {
            var dbs = sessionToUpdate.OpenAllDatabases();
            ReplicaSync matchingReplicaSync = null;
            foreach (ReplicaSync sync in sessionToUpdate.AllObjects<ReplicaSync>())
            {
              if (sync.SyncFromHost == sessionToRead.SystemHostName && sync.SyncFromPath == sessionToRead.SystemDirectory)
              {
                matchingReplicaSync = sync;
                break;
              }
            }
            if (changes.ChangeList.Count > 0)
            {
              foreach (TransactionChanges transactionChanges in changes.ChangeList)
              {
                if (matchingReplicaSync == null || matchingReplicaSync.TransactionNumber < transactionChanges.TransactionNumber)
                {
                  foreach (Change change in transactionChanges.ChangeList)
                  {
                    Database dbToUpdate = sessionToUpdate.OpenDatabase(change.DatabaseId, false, false);
                    Database dbToRead = sessionToRead.OpenDatabase(change.DatabaseId, false, false);
                    string dbName = dbToRead != null ? dbToRead.Name : null;
                    if (change.Deleted)
                    {
                      if (dbToUpdate == null) // does not exist
                        continue;
                      if (change.PageId == 0) // Database delete
                      {
                        currentVersion = dbToUpdate.Page.PageInfo.VersionNumber;
                        if (currentVersion < change.Version)
                          sessionToUpdate.DeleteDatabase(dbToUpdate);
                        else
                        {
                          conflictFound = true;
                          if (doUpdate(sessionToUpdate, currentVersion, change))
                            sessionToUpdate.DeleteDatabase(dbToUpdate);
                        }
                      }
                      else
                      {
                        Page page = sessionToUpdate.OpenPage(dbToUpdate, change.PageId);
                        if (page == null) // page does not exist
                          continue;
                        currentVersion = page.PageInfo.VersionNumber;
                        if (currentVersion < change.Version)
                          sessionToUpdate.DeletePage(dbToUpdate, page);
                        else
                        {
                          conflictFound = true;
                          if (doUpdate(sessionToUpdate, currentVersion, change))
                            sessionToUpdate.DeleteDatabase(dbToUpdate);
                        }
                      }
                    }
                    else
                    {
                      if (dbToUpdate == null) // does not exist
                        dbToUpdate = sessionToUpdate.NewDatabase(change.DatabaseId, 0, dbName);
                      if (change.PageId > 0)
                      {
                        Page pageToUpdate = sessionToUpdate.OpenPage(dbToUpdate, change.PageId);
                        Page pageToRead = sessionToRead.OpenPage(dbToRead, change.PageId);
                        if (pageToRead == null) // upcoming (not yet processed) changes must have deleted this page
                          continue;
                        currentVersion = pageToUpdate == null ? 0 : pageToUpdate.PageInfo.VersionNumber;
                        pageToReadVersion = pageToRead.PageInfo.VersionNumber;
                        if (currentVersion < pageToReadVersion || dbToUpdate.IsNew)
                          sessionToUpdate.ReplacePage(dbToUpdate, pageToUpdate, pageToRead);
                        else
                        {
                          conflictFound = true;
                          if (doUpdate(sessionToUpdate, currentVersion, change))
                            sessionToUpdate.ReplacePage(dbToUpdate, pageToUpdate, pageToRead);
                        }
                      }
                    }
                  }
                }
              }
              UInt64 lastTransactionNumber = changes.ChangeList.Last().TransactionNumber;
              if (matchingReplicaSync != null)
                matchingReplicaSync.TransactionNumber = lastTransactionNumber;
              if (conflictFound)
              {
                sessionToUpdate.Verify();
              }
              sessionToUpdate.Commit();
              if (matchingReplicaSync == null)
              {
                sessionToUpdate.BeginUpdate(); // separate transaction or else gets confused with databases added by sync
                matchingReplicaSync = new ReplicaSync(sessionToRead, lastTransactionNumber);
                sessionToUpdate.Persist(matchingReplicaSync);
                sessionToUpdate.Commit();
              }
            }
          }
        }
      }
    }

    static public SyncOperationStatistics MicrosoftSync(this SessionBase sessionToUpdate, SessionBase sessionToRead)
    {
      SyncProvider sourceProvider = new SyncProvider(sessionToRead);
      SyncProvider destProvider = new SyncProvider(sessionToUpdate);
      SyncOrchestrator syncAgent = new SyncOrchestrator();
      syncAgent.LocalProvider = sourceProvider;
      syncAgent.RemoteProvider = destProvider;
      return syncAgent.Synchronize();
    }
  }

  /// <summary>
  /// VelocityDB standard Microsoft Sync provider as described in https://msdn.microsoft.com/en-us/library/bb902826(v=sql.110).aspx
  /// </summary>
  public class SyncProvider : KnowledgeSyncProvider, IChangeDataRetriever, INotifyingChangeApplierTarget
  {
    SessionBase m_session;
    SyncIdFormatGroup m_idFormatGroup;

    public SyncProvider(SessionBase session)
    {
      m_session = session;
      m_idFormatGroup = new SyncIdFormatGroup();

      m_idFormatGroup.ChangeUnitIdFormat.IsVariableLength = false;
      m_idFormatGroup.ChangeUnitIdFormat.Length = sizeof(UInt64);

      m_idFormatGroup.ItemIdFormat.IsVariableLength = false;
      m_idFormatGroup.ItemIdFormat.Length = sizeof(UInt64);

      m_idFormatGroup.ReplicaIdFormat.IsVariableLength = false;
      m_idFormatGroup.ReplicaIdFormat.Length = sizeof(UInt64);
    }

    /// <inheritdoc />
    public override SyncIdFormatGroup IdFormats
    {
      get
      {
        return m_idFormatGroup;
      }
    }

    /// <inheritdoc />
    public override void BeginSession(SyncProviderPosition position, SyncSessionContext syncSessionContext)
    {
      if (!syncSessionContext.IsCanceled())
      {
        if (position == SyncProviderPosition.Local)
          m_session.BeginUpdate();
        else
          m_session.BeginRead();
      }
    }

    /// <inheritdoc />
    public override void EndSession(SyncSessionContext syncSessionContext)
    {
      if (syncSessionContext.IsCanceled())
        m_session.Abort();
      else
        m_session.Commit();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    /// <inheritdoc />
    public override ChangeBatch GetChangeBatch(uint batchSize, SyncKnowledge destinationKnowledge, out object changeDataRetriever)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override FullEnumerationChangeBatch GetFullEnumerationChangeBatch(uint batchSize, SyncId lowerEnumerationBound, SyncKnowledge knowledgeForDataRetrieval, out object changeDataRetriever)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <inheritdoc />
    public override void GetSyncBatchParameters(out uint batchSize, out SyncKnowledge knowledge)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void ProcessChangeBatch(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void ProcessFullEnumerationChangeBatch(ConflictResolutionPolicy resolutionPolicy, FullEnumerationChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return base.ToString();
    }

    public object LoadChangeData(LoadChangeContext loadChangeContext)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public ulong GetNextTickCount()
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public IChangeDataRetriever GetDataRetriever()
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public bool TryGetDestinationVersion(ItemChange sourceChange, out ItemChange destinationVersion)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public void SaveItemChange(SaveChangeAction saveChangeAction, ItemChange change, SaveChangeContext context)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public void SaveChangeWithChangeUnits(ItemChange change, SaveChangeWithChangeUnitsContext context)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public void SaveConflict(ItemChange conflictingChange, object conflictingChangeData, SyncKnowledge conflictingChangeKnowledge)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }

    public void StoreKnowledgeForScope(SyncKnowledge knowledge, ForgottenKnowledge forgottenKnowledge)
    {
      // anyone know how to implement ???
      throw new NotImplementedException();
    }
  }
}
