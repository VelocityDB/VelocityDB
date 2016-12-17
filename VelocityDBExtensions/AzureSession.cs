using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDBExtensions
{
  /// <summary>
  /// Prototype for using Windows Azure API and Steams directly. Currently running into <see cref="Stream"/> related issues.
  /// Possibly related to positioning within stream not always working correctly
  /// Instead of using AzureSession, use other session classes with shared Azure drive
  /// See https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-files/
  /// example :
  /// net use z: \\samples.file.core.windows.net\logs /u:samples<storage-account-key>
  /// </summary>
  public class AzureSession : SessionNoServer
  {
    static readonly Int64 s_initialFileSize = 10000000;
    CloudStorageAccount m_cloudStorageAccount;
    CloudFileClient m_cloudFileClient;
    CloudFileShare m_cloudShare;
    CloudFileDirectory m_rootDir;
    CloudFileDirectory m_databaseDir;
    string m_shareName;

    public AzureSession(string connectionString, string shareName, string systemDir, int waitForLockMilliseconds = 5000, bool optimisticLocking = true,
      bool enableCache = true, CacheEnum objectCachingDefaultPolicy = CacheEnum.Yes)
      : base(systemDir, waitForLockMilliseconds, optimisticLocking, enableCache, objectCachingDefaultPolicy)
    {
      m_cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
      if (Path.IsPathRooted(systemDir) == false)
        SystemDirectory = systemDir;
      m_shareName = shareName;
      m_cloudFileClient = m_cloudStorageAccount.CreateCloudFileClient();
      m_cloudShare = m_cloudFileClient.GetShareReference(shareName);
      if (m_cloudShare.Exists())
      {
        m_rootDir = m_cloudShare.GetRootDirectoryReference();
        m_databaseDir = m_rootDir.GetDirectoryReference(systemDir);
        m_databaseDir.CreateIfNotExists();
      }
    }

    /// <inheritdoc />
    public override Stream FileOpen(Database db, FileAccess fileAccess, ref string errorMessage, FileMode fileMode = FileMode.Open, bool excusiveAccess = false, int waitOverride = -1, bool signalError = true)
    {
      if (waitOverride == -1)
        waitOverride = WaitForLockMilliseconds;
      if (db.FileStream != null && ((db.FileStream.CanRead == false && fileAccess == FileAccess.Read) ||
                                    (db.FileStream.CanWrite == false && (excusiveAccess || fileAccess == FileAccess.ReadWrite))))
      {
        db.FileStream.Dispose();
        db.FileStream = null;
      }
      if (db.FileStream == null)
        if (excusiveAccess)
        {
          CloudFile cloudFile = m_databaseDir.GetFileReference(db.FileInfo.Name);
          FileRequestOptions fileRequestOptions = new FileRequestOptions();
          fileRequestOptions.ServerTimeout = TimeSpan.FromMilliseconds(waitOverride);
          fileRequestOptions.ParallelOperationThreadCount = 1;
          if (fileAccess == FileAccess.Read)
            db.FileStream = cloudFile.OpenRead(null, fileRequestOptions);
          else
            db.FileStream = cloudFile.OpenWrite(s_initialFileSize, null, fileRequestOptions);
          //db.FileInfo, fileAccess, ref errorMessage, FileShare.None, fileMode, waitOverride, UseExternalStorageApi, signalError);
        }
        else
        {
          CloudFile cloudFile = m_databaseDir.GetFileReference(db.FileInfo.Name);
          FileRequestOptions fileRequestOptions = new FileRequestOptions();
          fileRequestOptions.ServerTimeout = TimeSpan.FromMilliseconds(waitOverride);
          if (fileAccess == FileAccess.Read)
            db.FileStream = cloudFile.OpenRead(null, fileRequestOptions);
          else
            db.FileStream = cloudFile.OpenWrite(s_initialFileSize, null, fileRequestOptions);
        }
      return db.FileStream;
    }

    public override Stream FileOpen(FileInfo fileInfo, FileAccess fileAccess, ref string errorMessage, FileShare fileShare, FileMode fileMode, int waitForLockMilliseconds, bool useExternalStorage, bool signalError = true)
    {
      Stream fStream = null;
      DateTime dateTimeStart = DateTime.Now;
      int sleepTime = Math.Min(300, waitForLockMilliseconds);
      while (true)
        try
        {
          if (fileMode == FileMode.CreateNew)
          { // possibly reuse aborted new file (aborted if not exclusively locked)
            CloudFile cloudFile = m_databaseDir.GetFileReference(fileInfo.Name);
            if (cloudFile.Exists())
              throw new DatabaseAlreadyExistsException(fileInfo.FullName);
            cloudFile.Create(s_initialFileSize);
            FileRequestOptions fileRequestOptions = new FileRequestOptions();
            fileRequestOptions.ServerTimeout = TimeSpan.FromMilliseconds(waitForLockMilliseconds);
            fileRequestOptions.ParallelOperationThreadCount = 1;
            fStream = cloudFile.OpenWrite(s_initialFileSize, null, fileRequestOptions);
          }
          else
          {
            CloudFile cloudFile = m_databaseDir.GetFileReference(fileInfo.Name);
            FileRequestOptions fileRequestOptions = new FileRequestOptions();
            fileRequestOptions.ServerTimeout = TimeSpan.FromMilliseconds(waitForLockMilliseconds);
            if (fileAccess == FileAccess.Read)
              fStream = cloudFile.OpenRead(null, fileRequestOptions);
            else
              fStream = cloudFile.OpenWrite(s_initialFileSize, null, fileRequestOptions);
          }
          return fStream;
        }
        catch (FileNotFoundException e)
        {
          fStream?.Dispose();
          if (signalError)
            throw e;
          errorMessage += e.Message;
          return null;
        }
        catch (System.IO.IOException e)
        {
          TimeSpan span = DateTime.Now - dateTimeStart;
          if (span.TotalMilliseconds >= waitForLockMilliseconds)
          {
            if (signalError)
            {
              fStream?.Dispose();
              throw e;
            }
            else
            {
              errorMessage += e.Message;
              return null;
            }
          }
          else
          {
#if WINDOWS_UWP
            System.Threading.Tasks.Task.Delay(sleepTime);
#else
            Thread.Sleep(sleepTime); // give thread that is holding lock a chance to release the lock
#endif
            if (sleepTime > 25)
              --sleepTime;
          }
        }
    }

    public override bool ContainsDatabase(DatabaseLocation location, UInt32 dbNum, string extension = ".odb")
    {
      if (location != null)
      {
        if (dbNum < location.StartDatabaseNumber || dbNum > location.EndDatabaseNumber)
          return false;
        //string fullPath = location.DirectoryPath + Path.DirectorySeparatorChar + dbNum.ToString() + extension;
        string fullPath = dbNum.ToString() + extension; // handle multiple database locations later
        CloudFile cloudFile = m_databaseDir.GetFileReference(fullPath);
        return cloudFile.Exists();
      }
      return false;
    }

    /// <inheritdoc />
    public override bool CreateDirectory(string path)
    {
      var share = m_rootDir.GetDirectoryReference(path);
      if (!share.Exists())
      {
        m_rootDir = m_cloudShare.GetRootDirectoryReference();
        m_databaseDir = m_rootDir.GetDirectoryReference(path);
        m_databaseDir.CreateIfNotExists();
        return true;
      }
      return false;
    }

    /// <inheritdoc />
    public override bool DatabaseStillExist(Database db)
    {
      CloudFile cloudFile = m_databaseDir.GetFileReference(db.FileInfo.Name);
      return cloudFile.Exists();
    }

    /// <inheritdoc />
    public override void DeleteFile(FileInfo fileInfo)
    {
      var share = m_rootDir.GetDirectoryReference(fileInfo.Directory.Name);
      var file = share.GetFileReference(fileInfo.Name);
      file.Delete();
    }

    /// <inheritdoc />
    protected override void MoveFile(Database db, string newPath)
    {
      FileInfo newFileInfo = new FileInfo(newPath);
      m_databaseDir = m_rootDir.GetDirectoryReference(db.FileInfo.Directory.Name);
      CloudFile cloudFile = m_databaseDir.GetFileReference(db.FileInfo.Name);
      CloudFile destFile = m_databaseDir.GetFileReference(newFileInfo.Name);
      destFile.StartCopy(cloudFile); // Azure provides NO rename/move functionality !!!
      while (destFile.CopyState.Status == Microsoft.WindowsAzure.Storage.Blob.CopyStatus.Pending)
        Thread.Sleep(100);
      cloudFile.Delete();
      db.FileInfo = newFileInfo;
    }

    /// <inheritdoc />
    public override List<Database> OpenAllDatabases(bool update = false)
    {
      SortedSet<Database> dbSet = new SortedSet<Database>();
      foreach (Database db in NewDatabases)
        dbSet.Add(db);
      foreach (DatabaseLocation location in (IEnumerable<DatabaseLocation>)DatabaseLocations)
      {
        m_databaseDir = m_rootDir.GetDirectoryReference(location.DirectoryPath);
        foreach (CloudFile file in m_databaseDir.ListFilesAndDirectories())
        {
          string fileName = file.Name;
          int indexEnd = fileName.IndexOf('.');
          string dbNumStr = fileName.Substring(0, indexEnd);
          UInt32 dbNum;
          if (UInt32.TryParse(dbNumStr, out dbNum) && dbNum >= location.StartDatabaseNumber && dbNum <= location.EndDatabaseNumber)
          {
            Database db = OpenDatabase(dbNum, update);
            dbSet.Add(db);
          }
        }
      }
      return dbSet.ToList<Database>();
    }

    /// <inheritdoc />
    protected override Database reopenDatabaseForUpdate(Database dbReadOnly, Schema schema, out bool updated, bool forceReplace = false)
    {
      if (dbReadOnly.IsNew && forceReplace == false)
      {
        updated = false;
        return dbReadOnly;
      }
      string errorMessage = String.Empty;
      Stream fStream = null;
      try
      {
        fStream = OpenStreamForUpdate(dbReadOnly, ref errorMessage, false);
        if (fStream == null)
          if (OptimisticLocking)
            throw new OptimisticLockingFailed(errorMessage + "for Database: " + dbReadOnly.DatabaseNumber.ToString());
          else
            throw new PageUpdateLockException(errorMessage + "for Database: " + dbReadOnly.DatabaseNumber.ToString());
        if (OptimisticLocking)
        {
          if (dbReadOnly.FileStream == null)
            fStream.Dispose();
          fStream = OpenStreamForRead(dbReadOnly, ref errorMessage);
        }
        //Stream fStream = FileOpen(dbReadOnly, FileAccess.ReadWrite, ref errorMessage, FileMode.Open, false, m_waitForLockMilliseconds);
        return LoadUpToDateDatabasePageFromStream(dbReadOnly, schema, fStream, out updated, forceReplace, true, true);
      }
      finally
      {
        if (fStream != null && dbReadOnly.FileStream == null) // don't close if SessionNoServer (locks tryDatabaseNumber by keeping file Open)
          fStream.Dispose();
      }
    }
  }
}
