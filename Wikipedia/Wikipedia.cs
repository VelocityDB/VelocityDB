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

namespace Wikipedia
{
  class Wikipedia
  {
    static long s_docCountIndexed = 0; // info about progress of indexing
    static readonly string s_systemDir = "Wikipedia"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_licenseDbFile = "c:/4.odb";
    static readonly string s_wikipediaXmlFile = "d:/enwiki-latest-pages-articles.xml";

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
      BTreeSetOidShort<Word> wordSet = indexRoot.lexicon.WordSet;
      using (StreamWriter writer = new StreamWriter("Wikipedia.txt"))
      {
        writer.WriteLine("Number of words in Lexicon is: " + wordSet.Count);
        foreach (Word word in wordSet)
        {
          writer.WriteLine(word.aWord + " " + word.DocumentHit.Count);
        }
        writer.Close();
      }
    }

    static void importEntireWikipedia()
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
        Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
        //GCSettings.LatencyMode = GCLatencyMode.Batch;// try to keep the WeakIOptimizedPersistableReference objects around longer
        Placement documentPlacement = new Placement(Document.PlaceInDatabase, 1003, 1, 500, 1000, false, false, 1000, false);
        Placement contentPlacement = new Placement(Document.PlaceInDatabase, 1, 1, 500, UInt16.MaxValue, false, false, 1, false);
        XmlComment xmlComment;
        XmlElement xmlElement;
        XmlEntity xmlEntity;
        XmlText xmlText;
        XmlWhitespace xmlWhitespace;
        session.BeginUpdate();
        File.Copy(s_licenseDbFile, System.IO.Path.Combine(session.SystemDirectory, "4.odb"), true);
        // register all database schema classes used by the application in advance to avoid lock conflict later in parallell indexing
        session.RegisterClass(typeof(Repository));
        session.RegisterClass(typeof(IndexRoot));
        session.RegisterClass(typeof(Document));
        session.RegisterClass(typeof(Lexicon));
        session.RegisterClass(typeof(DocumentText));
        session.RegisterClass(typeof(Word));
        session.RegisterClass(typeof(WordGlobal));
        session.RegisterClass(typeof(WordHit));
        session.RegisterClass(typeof(BTreeSet<Document>));
        session.RegisterClass(typeof(OidShort));
        session.RegisterClass(typeof(BTreeMap<Word, WordHit>));
        session.RegisterClass(typeof(HashCodeComparer<Word>));
        session.RegisterClass(typeof(BTreeSetOidShort<Word>));
        session.RegisterClass(typeof(BTreeMapOidShort<Word, WordHit>));
        Database db = session.OpenDatabase(IndexRoot.PlaceInDatabase, false, false);
        if (db != null)
        {
          outputSomeInfo(session);
          session.Abort();
          return;
        }
        session.NewDatabase(IndexRoot.PlaceInDatabase, 0, "IndexRoot");
        session.NewDatabase(Lexicon.PlaceInDatabase, 0, "Lexicon");
        session.NewDatabase(Repository.PlaceInDatabase, 0, "Repository");
        for (UInt32 i = 40; i <= 186; i++)
        {
          session.NewDatabase(i, 512, "Document"); // pre allocate 146 Document databases presized to 512MB each
        }
        //session.SetTraceDbActivity(Lexicon.PlaceInDatabase);
        //session.SetTraceAllDbActivity();
        XmlDocument xmlDocument = new XmlDocument("enwiki-latest-pages-articles.xml");
        IndexRoot indexRoot = new IndexRoot(btreeNodeSize, session);
        indexRoot.Persist(session, indexRoot, true);
        Document doc = null;
        bool titleElement = false;
        bool pageText = false;
        UInt32 currentDocumentDatabaseNum = documentPlacement.StartDatabaseNumber;
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
                      doc.Persist(documentPlacement, session, true);
                      if (doc.DatabaseNumber != currentDocumentDatabaseNum)
                      {
                        session.FlushUpdates(session.OpenDatabase(currentDocumentDatabaseNum));
                        Console.WriteLine("Database: " + currentDocumentDatabaseNum +" is completed, done importing article " + docCount + " number of lines: " + textReader.LineNumber);
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
                      if (doc.DatabaseNumber != contentPlacement.TryDatabaseNumber)
                        contentPlacement = new Placement(doc.DatabaseNumber, (ushort)contentPlacement.StartPageNumber, 1, contentPlacement.MaxObjectsPerPage, contentPlacement.MaxPagesPerDatabase, false, false, 1, false);
                      content.Persist(contentPlacement, session, false);
                      Debug.Assert(content.DatabaseNumber == doc.DatabaseNumber);
                      doc.Content = content;
                      indexRoot.repository.documentSet.AddFast(doc);
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

    static void createDocumentInvertedIndex(SessionBase session, Database db, BTreeSet<Document> documentSet)
    {
      UInt32 dbNum = db.DatabaseNumber;
      Document doc = null;
      Document inputDoc = new Document(db.Id);
      Placement wordPlacement = new Placement(inputDoc.DatabaseNumber, 20000, 1, 25000, 65000, true, false, 1, false);
      Placement wordHitPlacement = new Placement(inputDoc.DatabaseNumber, 40000, 1, 25000, 65500, true, false, 1, false);
      //session.SetTraceDbActivity(db.DatabaseNumber);
      BTreeSetIterator<Document> iterator = documentSet.Iterator();
      iterator.GoTo(inputDoc);
      inputDoc = iterator.Current();
      while (inputDoc != null && inputDoc.Page.Database.DatabaseNumber == dbNum)
      {
        doc = (Document)session.Open(inputDoc.Page.Database, inputDoc.Id); // if matching database is availeble, use it to speed up lookup
        DocumentText docText = doc.Content;
        string text = docText.Text.ToLower();
        MatchCollection tagMatches = Regex.Matches(text, "[a-z][a-z.$]+");
        UInt64 wordCt = 0;
        WordHit wordHit;
        Word word;
        if (++s_docCountIndexed % 50000 == 0)
          Console.WriteLine(DateTime.Now.ToString() + ", done indexing article: " + s_docCountIndexed);
        BTreeSetOidShort<Word> wordSet = doc.WordSet;
        foreach (Match m in tagMatches)
        {
          word = new Word(m.Value);
          if (wordSet.TryGetKey(word, ref word))
          {
            //wordHit = doc.WordHit[word]; // to costly to add tight now - figure out a better way ?
            //wordHit.Add(wordCt);
          }
          else
          {
            word = new Word(m.Value);
            word.Persist(wordPlacement, session);
            wordSet.Add(word);
            wordHit = new WordHit(doc, wordCt++, session);
            //wordHit.Persist(wordHitPlacement, session);
            doc.WordHit.ValuePlacement = wordHitPlacement;
            doc.WordHit.AddFast(word, wordHit);
          }
        }
        inputDoc = iterator.Next();
      }
      session.FlushUpdates(db);
      session.ClearCachedObjects(db); // free up memory for objects we no longer need to have cached
      Console.WriteLine(DateTime.Now.ToString() + ", done indexing article: " + s_docCountIndexed + " Database: " + dbNum + " is completed.");
    }

    static void createInvertedIndex()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        session.EnableAutoPageFlush = false; // so that threads don't stomb on each other
        Console.WriteLine(DateTime.Now.ToString() + ", start creating inverted index");
        ParallelOptions pOptions = new ParallelOptions();
        pOptions.MaxDegreeOfParallelism = 2; // set to what is appropriate for your computer (cores & memory size)
        //pOptions.MaxDegreeOfParallelism = 1; // appears to work best with only 16GB of memory
        IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
        BTreeSet<Document> documentSet = indexRoot.repository.documentSet;
        List<Database> dbs = session.OpenAllDatabases(true);
        Parallel.ForEach<Database>(dbs, pOptions,
          (Database db, ParallelLoopState loop) => // method invoked by the loop on each iteration
          {
            if (db.DatabaseNumber >= Document.PlaceInDatabase)
              createDocumentInvertedIndex(session, db, documentSet);
          });
        session.Commit();
        Console.WriteLine(DateTime.Now.ToString() + ", done creating inverted index");
      }
    }

    static void createTopLevelInvertedIndex()
    {
      Console.WriteLine(DateTime.Now.ToString() + ", start creating top level inverted index");
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        Placement wordPlacement = new Placement(Lexicon.PlaceInDatabase, 2, 1, 1000, 50000, true, false, UInt32.MaxValue, false);
        session.BeginUpdate();
        IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
        BTreeSetOidShort<Word> wordSet = indexRoot.lexicon.WordSet;
        BTreeSet<Document> documentSet = indexRoot.repository.documentSet;
        Word existingWord = null;
        foreach (Document doc in documentSet)
        {
          foreach (Word word in doc.WordSet)
          {
            WordHit wordHit = doc.WordHit[word];
            if (wordSet.TryGetKey(word, ref existingWord))
            {
              existingWord.GlobalCount = existingWord.GlobalCount + (uint)wordHit.Count;
            }
            else
            {
              existingWord = new WordGlobal(word.aWord, session, (uint)wordHit.Count);
              existingWord.Persist(wordPlacement, session);
              indexRoot.lexicon.WordSet.Add(existingWord);
            }
            existingWord.DocumentHit.AddFast(doc);
          }
          doc.Indexed = true;
        }
        session.Commit();
        Console.WriteLine(DateTime.Now.ToString() + ", done creating top level inverted index");
      }
    }

    static void Main(string[] args)
    {
      SessionBase.BaseDatabasePath = "d:/Databases";
      DataCache.MaximumMemoryUse = 11000000000; // 11 GB, set this to what fits your case
      SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;
      importEntireWikipedia();
      createInvertedIndex();
      createTopLevelInvertedIndex();
    }
  }
}