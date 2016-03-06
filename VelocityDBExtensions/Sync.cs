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
  }
}
