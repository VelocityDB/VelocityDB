using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.NUnit
{
  public class ComputerFileData : OptimizedPersistable
  {
    public ComputerFileData() { }
    public bool ItemDeleted { get; set; }
    public string PackagingErrors { get; set; }
    public bool PackagedSuccessfully { get; set; }
    public bool FileSelected { get; set; }
    public bool WDSSelected { get; set; }
    public bool FPPSearchSelected { get; set; }
    public bool RegexSelected { get; set; }
    public bool SharepointSelected { get; set; }
    public bool SharepointIndexedSearchSelected { get; set; }
    public bool WebSelected { get; set; }
    public bool DateFilterExcluded { get; set; }
    public bool IncludeFilterExcluded { get; set; }
    public bool ExcludeFilterExcluded { get; set; }
    public Int64 ContainerID { get; set; }
    public Int64 FolderID { get; set; }
    public Int64 FileID { get; set; }
    public Int64? ParentFileID { get; set; }
    public string Category { get; set; }
    public string ResourceImageType { get; set; }
    public string FullPath { get; set; }
    public int FullPathLength { get; set; }
    public string FolderPath { get; set; }
    public string FileName { get; set; }
    public string FileExt { get; set; }
    public string FileType { get; set; }
    public DateTime? LastAccessTime { get; set; }
    public DateTime? LastWriteTime { get; set; }
    public DateTime? CreationTime { get; set; }
    public long? Size { get; set; }
    public string Propertys { get; set; }
    public string SharepointType { get; set; }
    public string SharepointUserName { get; set; }
    public string SharepointPassword { get; set; }
    public string SharepointSite { get; set; }
    public string SharepointCreatedBy { get; set; }
    public string ShareParentURL { get; set; }
    public string SharepointVersion { get; set; }
    public string WebURL { get; set; }
    public string webParent { get; set; }
    public string WebUserName { get; set; }
    public string WebPassword { get; set; }
    public string MachineName { get; set; }

    public override CacheEnum Cache
    {
      get
      {
        return CacheEnum.Yes;
      }
    }
  }
}
