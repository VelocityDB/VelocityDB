using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDBExtensions
{
  public static class ImportExportCsv
  {
    /// <summary>
    /// Export all persistent objects to .csv files, one file for each Type and version of Type.
    /// This is preview release, format may change. ImportFromCSV can be used to recreate your data.
    /// Note that Microsoft Excel can't handle many of these CSV files due to a field value limitation (at about 33000 chars)
    /// Notepad++ is one application that can read these files.
    /// Some fields like array data are encoded http://msdn.microsoft.com/en-us/library/dhx0d524(v=vs.110).aspx
    /// </summary>
    /// <param name="session">the active session</param>
    /// <param name="directory">Where to store the CSV files</param>
    static public void ExportToCSV(this SessionBase session, string directory)
    {
      const char fieldSeperator = ',';
      if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      StreamWriter writer;
      StreamWriter dbWriter = null;
      StreamWriter pageWriter = null;
      Dictionary<UInt32, StreamWriter> files = new Dictionary<uint, StreamWriter>();
      byte[] newLineBytes = Page.StringToByteArray(Environment.NewLine);
      try
      {
        List<Database> dbs = session.OpenAllDatabases();
        string filePath = "Database.csv";
        FileStream dbStream = new FileStream(Path.Combine(directory, filePath), FileMode.Create);
        dbWriter = new StreamWriter(dbStream);
        dbWriter.WriteLine($"Number{fieldSeperator}Name");
        filePath = "Page.csv";
        FileStream pageStream = new FileStream(Path.Combine(directory, filePath), FileMode.Create);
        pageWriter = new StreamWriter(pageStream);
        pageWriter.WriteLine($"DatabaseNumber{fieldSeperator}PageNumber{fieldSeperator}NumberOfSlots{fieldSeperator}FirstFreeSlot{fieldSeperator}Version{fieldSeperator}Encryption{fieldSeperator}Compression{fieldSeperator}TypeVersion");
        foreach (Database db in dbs)
        {
          if (db != null && db.DatabaseNumber != 4 && db.DatabaseNumber != 0) // we skip the license database because we can't restore it without encryption key
          {
            dbWriter.WriteLine(db.DatabaseNumber.ToString() + fieldSeperator + "\"" + db.Name + "\"");
            foreach (Page page in db)
            {
              if (page.PageNumber > 0)
              {
                pageWriter.WriteLine(page.Database.DatabaseNumber.ToString() + fieldSeperator +
                                     page.PageNumber + fieldSeperator +
                                     page.PageInfo.NumberOfSlots + fieldSeperator +
                                     page.PageInfo.FirstFreeSlot + fieldSeperator +
                                     page.PageInfo.VersionNumber + fieldSeperator +
                                     page.PageInfo.Encryption + fieldSeperator +
                                     page.PageInfo.Compressed + fieldSeperator +
                                     page.PageInfo.ShapeNumber);
                foreach (IOptimizedPersistable pObj in page)
                {
                  TypeVersion tv = pObj.GetTypeVersion();
                  if (!files.TryGetValue(tv.ShortId, out writer))
                  {
                    Type type = tv.VelocityDbType.Type;
                    string typeName = type == null ? "Unknown (not loaded)" : type.ToGenericTypeString();
                    string[] illegal = new string[] { "<", ">" };
                    foreach (var c in illegal)
                    {
                      typeName = typeName.Replace(c, string.Empty);
                    }
                    filePath = typeName + pObj.GetTypeVersion().ShortId + ".csv";
                    FileStream fStream = new FileStream(Path.Combine(directory, filePath), FileMode.Create);
                    writer = new StreamWriter(fStream);
                    files[pObj.GetTypeVersion().ShortId] = writer;
                    List<DataMember> members = tv.GetDataMemberList();
                    byte[] bytes = Page.StringToByteArray($"IOptimizedPersistable.id{fieldSeperator}"); // special transient member
                    fStream.Write(bytes, 0, bytes.Length);
                    if (tv.IsString)
                    {
                      bytes = Page.StringToByteArray($"String");
                      fStream.Write(bytes, 0, bytes.Length);
                    }
                    else if (tv.IsISerializable)
                    {
                      bytes = Page.StringToByteArray($"IsISerializable");
                      fStream.Write(bytes, 0, bytes.Length);                     
                    }
                    else
                    {
                      int l = members.Count;
                      for (int i = 0; i < l; i++)
                        {
                          if (i + 1 < l)
                            bytes = Page.StringToByteArray(members[i].FieldName + fieldSeperator);
                          else
                            bytes = Page.StringToByteArray(members[i].FieldName);
                          fStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    //writer.Write("\"" + pObj.Shape.velocityDbType.type.AssemblyQualifiedName + "\"");
                    writer.WriteLine();
                  }
                  string aRow = tv.EncodeForCsv(pObj, page.PageInfo, session);
                  writer.WriteLine(aRow);
                }
              }
            }
          }
        }
      }
      finally
      {
        foreach (StreamWriter s in files.Values)
        {
#if WINDOWS_UWP
          s.Flush();
          s.Dispose();
#else
          s.Close();
#endif
        }
#if WINDOWS_UWP
        pageWriter?.Dispose();
        dbWriter?.Dispose();
#else
        pageWriter?.Close();
        dbWriter?.Close();
#endif
      }
    }

    /// <summary>
    /// Restores database files, pages and objects from a .csv file data created with ExportToCSV
    /// </summary>
    /// <param name="session">the active session</param>
    /// <param name="csvDirectory">Path to directory containing CSV files</param>
    static public void ImportFromCSV(this SessionBase session, string csvDirectory)
    {
      const char fieldSeperator = ',';
      DirectoryInfo di = new DirectoryInfo(csvDirectory);
#if WINDOWS_PHONE
      List<FileInfo> files = di.GetFiles("*.csv").ToList();
#else
      List<FileInfo> files = di.GetFiles("*.csv", SearchOption.TopDirectoryOnly).ToList();
#endif
      Schema schema = session.OpenSchema(false);
      using (StreamReader textReader = new StreamReader(Path.Combine(csvDirectory, "Database.csv")))
      {
        string header = textReader.ReadLine();
        string dbInfoString = textReader.ReadLine();
        while (dbInfoString != null)
        {
          string[] dbInfo = dbInfoString.Split(fieldSeperator);
          UInt32 dbNum = UInt32.Parse(dbInfo[0]);
          string dbName = dbInfo[1].Trim(new char[] { '"' });
          Database db = null;
          if (dbNum < 10)
            db = session.OpenDatabase(dbNum, false, false);
          if (db == null)
            db = session.NewDatabase(dbNum, 0, dbName);
          dbInfoString = textReader.ReadLine();
        }
      }
      using (StreamReader textReader = new StreamReader(Path.Combine(csvDirectory, "Page.csv")))
      {
        string header = textReader.ReadLine();
        string pageInfoString = textReader.ReadLine();
        while (pageInfoString != null)
        {
          int i = 0;
          string[] pageInfo = pageInfoString.Split(fieldSeperator);
          UInt32 dbNum = UInt32.Parse(pageInfo[i++]);
          UInt16 pageNum = UInt16.Parse(pageInfo[i++]);
          UInt16 numberOfSlots = UInt16.Parse(pageInfo[i++]);
          UInt16 firstFreeSlot = UInt16.Parse(pageInfo[i++]);
          UInt64 versionNumber = UInt64.Parse(pageInfo[i++]);
          PageInfo.encryptionKind encryptionKind = (PageInfo.encryptionKind)Enum.Parse(typeof(PageInfo.encryptionKind), pageInfo[i++]);
          PageInfo.compressionKind compressed = (PageInfo.compressionKind)Enum.Parse(typeof(PageInfo.compressionKind), pageInfo[i++]);
          UInt32 typeVersion = UInt32.Parse(pageInfo[i]);
          Database db = session.OpenDatabase(dbNum, false, false);
          Page page = db.CachedPage(pageNum);
          if (page == null)
          {
            page = new Page(db, pageNum, typeVersion, numberOfSlots);
            page.PageInfo.Compressed = compressed;
            page.PageInfo.Encryption = encryptionKind;
            page.PageInfo.VersionNumber = versionNumber;
            page.PageInfo.NumberOfSlots = numberOfSlots;
            page.PageInfo.FirstFreeSlot = firstFreeSlot;
            session.UpdatePage(ref page);
          }
          pageInfoString = textReader.ReadLine();
        }
      }

      var schemaInternalTypeFiles = new[] {
        "VelocityDb.TypeInfo.Schema65747.csv",
        "VelocityDb.TypeInfo.TypeVersion65749.csv",
        "VelocityDb.TypeInfo.VelocityDbType65753.csv",
        "VelocityDb.TypeInfo.DataMember65745.csv",
        "VelocityDb.Collection.BTree.BTreeSetOidShortVelocityDb.TypeInfo.VelocityDbType65623.csv",
        "VelocityDb.Collection.Comparer.VelocityDbTypeComparer65655.csv"
      };
      var schemaInternalTypeFileInfo = new List<FileInfo>();
      foreach (var fileName in schemaInternalTypeFiles)
        schemaInternalTypeFileInfo.Add(di.GetFiles(fileName, SearchOption.TopDirectoryOnly).First());

      foreach (FileInfo info in schemaInternalTypeFileInfo)
      {
        string numberString = Regex.Match(info.Name, @"\d+").Value;
        if (numberString.Length > 0)
        {
          UInt32 typeShortId = UInt32.Parse(numberString);
          UInt16 slotNumber = (UInt16)typeShortId;
          TypeVersion tv = schema.GetTypeVersion(typeShortId, session);
          if (tv != null)
          {
            using (StreamReader textReader = new StreamReader(info.FullName))
            {
              CsvReader csvReader = new CsvReader(textReader, true);
              string[] fileldNames = csvReader.GetFieldHeaders();
              foreach (string[] record in csvReader)
              {
                tv.ObjectBytesFromStrings(record, fileldNames, session, schema);
              }
            }
          }
        }
      }
      Database schemaDb = session.OpenDatabase(Schema.SchemaDB);
      Page schemaPage = schemaDb.CachedPage(1);
      schemaPage.FinishUpCsvImport();
      var schemaTypes = new UInt32[] { 65747, 65749, 65753, 65745, 65623, 65655 };
      for (int i = 0; i < 2; i++)
        foreach (FileInfo info in files)
        {
          string numberString = Regex.Match(info.Name, @"\d+").Value;
          if (numberString.Length > 0)
          {
            UInt32 typeShortId = UInt32.Parse(numberString);
            UInt16 slotNumber = (UInt16)typeShortId;
            if (((i == 0 && slotNumber < Schema.s_bootupTypeCountExpanded) || (i == 1 && slotNumber >= Schema.s_bootupTypeCountExpanded)) && !schemaTypes.Contains(typeShortId))
            {
              TypeVersion tv = schema.GetTypeVersion(typeShortId, session);
              if (tv != null)
              {
                using (StreamReader textReader = new StreamReader(info.FullName))
                {
                  var csvReader = new CsvReader(textReader, true);
                  string[] fileldNames = csvReader.GetFieldHeaders();
                  foreach (string[] record in csvReader)
                  {
                    tv.ObjectBytesFromStrings(record, fileldNames, session, schema);
                  }
                }
              }
            }
          }
        }
    }
  }
}
