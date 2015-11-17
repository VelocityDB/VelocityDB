using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;

namespace DatabaseManager.Model
{
  public class DatabaseLocationMutable
  {
    public DatabaseLocationMutable(SessionBase session)
    {
      Session = session;
      StartDatabaseNumber = 100000000;
      EndDatabaseNumber = UInt32.MaxValue;
    }
    public string HostName { get; set;}
    public SessionBase Session { get; set; }

    public string DesKey { get; set; }
    public string DirectoryPath { get; set; }

    public DatabaseLocation BackupOfOrForLocation { get; set; }

    public PageInfo.compressionKind CompressPages { get; set; }
    public PageInfo.encryptionKind PageEncryption { get; set; }

    public UInt32 EndDatabaseNumber { get; set; }

    public UInt32 StartDatabaseNumber { get; set; }

    public bool IsBackupLocation { get; set; }
  }
}
