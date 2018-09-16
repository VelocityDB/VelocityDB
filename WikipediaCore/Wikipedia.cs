// This sample uses an XML export of the entire Wikipedia texts, download and uncompress: http://download.wikimedia.org/enwiki/latest/enwiki-latest-pages-articles.xml.bz2
// Change the input file path below or store file in d:\enwiki-latest-pages-articles.xml
// This sample creates an inverted index of all the words accouring in Wikedpedia.
// It keeps track of all words. There are many weird Wikipedia subjects out there so the number of different words is very large (652,870,281 lines of text with 12,772,300 subject titles)
// The inverted index is created using parallell threads. It could still needs some improvements to make it faster.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDbSchema.TextIndexer;
using VelocityDbSchema.Samples.Wikipedia;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VelocityDb.Collection.Comparer;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Wikipedia
{
  class Wikipedia
  {
    static long s_docCountIndexed = 0; // info about progress of indexing
    static readonly string s_systemDir = "WikipediaCore"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_wikipediaXmlFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "c:/SampleData/enwiki-latest-pages-articles.xml" : "/SampleData/enwiki-latest-pages-articles.xml";

    // Display any warnings or errors.
    private static void ValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
    {
      if (args.Severity == System.Xml.Schema.XmlSeverityType.Warning)
        Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
      else
        Console.WriteLine("\tValidation error: " + args.Message);
    }

    static void outputSomeInfo(SessionNoServer session)
    {
      IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
      var wordHits = indexRoot.Lexicon.TokenMap;
      using (StreamWriter writer = new StreamWriter("Wikipedia.txt"))
      {
        writer.WriteLine("Number of words in Lexicon is: " + indexRoot.Lexicon.IdToValue.Count);
        foreach (var p in wordHits)
        {
          var word = indexRoot.Lexicon.IdToValue[p.Key];
          writer.WriteLine(word + " " + p.Value.Count);
        }
        writer.Close();
      }
    }

    static void ImportEntireWikipedia()
    {
      const ushort btreeNodeSize = 10000;
      Console.WriteLine(DateTime.Now.ToString() + ", start importing Wikipedia text");
      //System.Xml.Schema.XmlSchema docSchema;
      //using (System.Xml.XmlTextReader schemaReader = new System.Xml.XmlTextReader("c:\\export-0_5.xsd"))
      //{
      //  docSchema = System.Xml.Schema.XmlSchema.Read(schemaReader, ValidationCallBack);
      // }
      int docCount = 0;
      using (SessionNoServer session = new SessionNoServer(s_systemDir, 5000, false, false, CacheEnum.No)) // turn of page and object caching
      {
        Console.WriteLine($"Running with databases in directory: {session.SystemDirectory}");
        //GCSettings.LatencyMode = GCLatencyMode.Batch;// try to keep the WeakIOptimizedPersistableReference objects around longer
        XmlComment xmlComment;
        XmlElement xmlElement;
        XmlEntity xmlEntity;
        XmlText xmlText;
        XmlWhitespace xmlWhitespace;
        session.BeginUpdate();
        // register all database schema classes used by the application in advance to avoid lock conflict later in parallel indexing
        Database db = session.OpenDatabase(IndexRoot.PlaceInDatabase, false, false);
        if (db != null)
        {
          outputSomeInfo(session);
          session.Abort();
          return;
        }
        //session.SetTraceDbActivity(Lexicon.PlaceInDatabase);
        //session.SetTraceAllDbActivity();
        XmlDocument xmlDocument = new XmlDocument("enwiki-latest-pages-articles.xml");
        IndexRoot indexRoot = new IndexRoot(btreeNodeSize, session);
        indexRoot.Persist(session, indexRoot, true);
        UInt32 currentDocumentDatabaseNum = 0;
        Document doc = null;
        bool titleElement = false;
        bool pageText = false;
        using (FileStream fs = new FileStream(s_wikipediaXmlFile, FileMode.Open))
        {
          //using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Decompress)) // if input was a .gz file
          {
            using (System.Xml.XmlTextReader textReader = new System.Xml.XmlTextReader(fs))
            {
              while (textReader.Read())
              {
                System.Xml.XmlNodeType nodeType = textReader.NodeType;
                switch (nodeType)
                {
                  case System.Xml.XmlNodeType.Attribute:
                    break;
                  case System.Xml.XmlNodeType.CDATA:
                    break;
                  case System.Xml.XmlNodeType.Comment:
                    xmlComment = new XmlComment(textReader.Value, xmlDocument);
                    break;
                  case System.Xml.XmlNodeType.Document:
                    break;
                  case System.Xml.XmlNodeType.DocumentFragment:
                    break;
                  case System.Xml.XmlNodeType.DocumentType:
                    break;
                  case System.Xml.XmlNodeType.Element:
                    xmlElement = new XmlElement(textReader.Prefix, textReader.LocalName, textReader.NamespaceURI, xmlDocument);
                    if (textReader.LocalName == "title")
                      titleElement = true;
                    else if (textReader.LocalName == "text")
                      pageText = true;
                    break;
                  case System.Xml.XmlNodeType.EndElement:
                    if (textReader.LocalName == "title" && doc != null)
                      titleElement = false;
                    else if (textReader.LocalName == "text" && doc != null)
                      pageText = false;
                    break;
                  case System.Xml.XmlNodeType.EndEntity:
                    break;
                  case System.Xml.XmlNodeType.Entity:
                    xmlEntity = new XmlEntity(textReader.LocalName, xmlDocument);
                    break;
                  case System.Xml.XmlNodeType.EntityReference:
                    break;
                  case System.Xml.XmlNodeType.None:
                    break;
                  case System.Xml.XmlNodeType.Notation:
                    break;
                  case System.Xml.XmlNodeType.ProcessingInstruction:
                    break;
                  case System.Xml.XmlNodeType.SignificantWhitespace:
                    break;
                  case System.Xml.XmlNodeType.Text:
                    xmlText = new XmlText(textReader.Value, xmlDocument);
                    if (titleElement)
                    {
                      doc = new Document(textReader.Value, indexRoot, session);
                      session.Persist(doc);
                      if (doc.DatabaseNumber != currentDocumentDatabaseNum)
                      {
                        if (currentDocumentDatabaseNum > 0)
                        {
                          session.FlushUpdates();
                          Console.WriteLine("Database: " + currentDocumentDatabaseNum + " is completed, done importing article " + docCount + " number of lines: " + textReader.LineNumber);
                        }
                        currentDocumentDatabaseNum = doc.DatabaseNumber;
                      }
                      //doc.Page.Database.Name = doc.Name;
                    }
                    else if (doc != null && pageText)
                    {
#if DEBUGx
                      Console.WriteLine(doc.Name + " line: " + textReader.LineNumber);
#endif
                      //if (textReader.LineNumber > 1000000)
                      //{
                      //  session.Commit();
                      //  return;
                      //}
                      DocumentText content = new DocumentText(textReader.Value, doc);
                      session.Persist(content, 10000);
                      doc.Content = content;
                      indexRoot.Repository.DocumentSet.AddFast(doc);
                      if (++docCount % 1000000 == 0)
                      {
                        //session.Commit(false); // skip recovery check, we do it in BeginUpdate which is enough
                        Console.WriteLine("Done importing article " + docCount + " number of lines: " + textReader.LineNumber);
                        //session.BeginUpdate();
                      }
                    }
                    break;
                  case System.Xml.XmlNodeType.Whitespace:
                    xmlWhitespace = new XmlWhitespace(textReader.Value, xmlDocument);
                    break;
                  case System.Xml.XmlNodeType.XmlDeclaration:
                    break;
                };
              }
              Console.WriteLine("Finished importing article " + docCount + " number of lines: " + textReader.LineNumber);
            }
          }
        }
        session.Commit();
      }
      Console.WriteLine(DateTime.Now.ToString() + ", done importing Wikipedia text");
    }

    static void createDocumentInvertedIndex(IndexRoot indexRoot, Document doc)
    {
      if (!doc.Indexed)
      {
        DocumentText docText = doc.Content;
        string text = docText.Text.ToLower();
        MatchCollection tagMatches = Regex.Matches(text, "[a-z][a-z$]+");
        if (++s_docCountIndexed % 50000 == 0)
          Console.WriteLine(DateTime.Now.ToString() + ", done indexing article: " + s_docCountIndexed);
        foreach (Match m in tagMatches)
          indexRoot.Lexicon.PossiblyAddToken(m.Value, doc);
        if (s_docCountIndexed % 1000 == 0)
          Console.WriteLine(DateTime.Now.ToString() + ", done indexing article: " + s_docCountIndexed + " Database: " + doc.DatabaseNumber + " is completed.");
        doc.Indexed = true;
      }
    }

    static void CreateInvertedIndex()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir, 5000, false, false, CacheEnum.No)) // turn of page and object caching))
      {
        session.BeginUpdate();
        session.RegisterClass(typeof(Repository));
        session.RegisterClass(typeof(IndexRoot));
        session.RegisterClass(typeof(Lexicon<string>));
        Console.WriteLine(DateTime.Now.ToString() + ", start creating inverted index");
        IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
        BTreeSet<Document> documentSet = indexRoot.Repository.DocumentSet;
        foreach (var doc in documentSet)
          createDocumentInvertedIndex(indexRoot, doc);
        session.Commit();
        Console.WriteLine(DateTime.Now.ToString() + ", done creating inverted index");
      }
    }

    static void Main(string[] args)
    {
      //SessionBase.BaseDatabasePath = "c:/Databases";
      DataCache.MaximumMemoryUse = 12000000000; // 12 GB, set this to what fits your case
      SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;
      SessionBase.BTreeAddFastTransientBatchSize = 5000;
      ImportEntireWikipedia();
      CreateInvertedIndex();
    }
  }
}