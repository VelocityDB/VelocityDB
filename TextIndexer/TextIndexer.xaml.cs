// This sample app demonstrates the use of inverted index, commonly used in search engines such as Google
// Optionally download some text file books from http://www.gutenberg.org/browse/scores/top, decompress and put in directory: d:\Books (or other path, see below)

using System;
using System.Runtime;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.TextIndexer;
using VelocityDb.Collection.BTree;
using HtmlAgilityPack;

namespace TextIndexer
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    static readonly string s_systemDir = "TextIndexer"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_booksDir = "d:/Books";
    static readonly string s_licenseDbFile = "c:/4.odb";

    SessionNoServer session;
    List<DataGrid> dataGridList;
    List<DataTable> dataTableList;
    public MainWindow()
    {
      const ushort btreeNodeSize = 5000;
      GCSettings.LatencyMode = GCLatencyMode.Batch;// try to keep the WeakIOptimizedPersistableReference objects around longer      
      dataGridList = new List<DataGrid>();
      dataTableList = new List<DataTable>();
      InitializeComponent();
      session = new SessionNoServer(s_systemDir);
      Placement placerIndexRoot = new Placement(IndexRoot.PlaceInDatabase);
      session.BeginUpdate();
      Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
      File.Copy(s_licenseDbFile, Path.Combine(session.SystemDirectory, "4.odb"), true);
      IndexRoot indexRoot;
      Database db = session.OpenDatabase(IndexRoot.PlaceInDatabase, false, false);
      if (db == null)
      {
        session.NewDatabase(IndexRoot.PlaceInDatabase, 0, "IndexRoot");
        session.NewDatabase(Lexicon.PlaceInDatabase, 0, "Lexicon");
        session.NewDatabase(Document.PlaceInDatabase, 0, "Document");
        session.NewDatabase(Repository.PlaceInDatabase, 0, "Repository");
        session.NewDatabase(DocumentText.PlaceInDatabase, 0, "DocumentText");
        session.NewDatabase(Word.PlaceInDatabase, 0, "Word");
        indexRoot = new IndexRoot(btreeNodeSize, session);
        if (Directory.Exists(s_booksDir))
        {
          string[] directoryTextFiles = Directory.GetFiles(s_booksDir, "*.txt");
          foreach (string fileName in directoryTextFiles)
          {
            listBoxPagesToAdd.Items.Add(fileName);
          }
        }
        else
        {
          wordMinCt.Text = 1.ToString();
          listBoxPagesToAdd.Items.Add("http://www.VelocityDB.com/");
          // other database products
          listBoxPagesToAdd.Items.Add("https://foundationdb.com/");
          listBoxPagesToAdd.Items.Add("http://www.oracle.com/us/products/database/index.html");
          listBoxPagesToAdd.Items.Add("http://www-01.ibm.com/software/data/db2/");
          listBoxPagesToAdd.Items.Add("http://www.versant.com/");
          listBoxPagesToAdd.Items.Add("http://web.progress.com/en/objectstore/");
          listBoxPagesToAdd.Items.Add("https://www.mongodb.org/");
          listBoxPagesToAdd.Items.Add("http://cassandra.apache.org/");
          listBoxPagesToAdd.Items.Add("http://www.sybase.com/");
          listBoxPagesToAdd.Items.Add("http://www.mcobject.com/perst");
          listBoxPagesToAdd.Items.Add("http://www.marklogic.com/what-is-marklogic/");
          listBoxPagesToAdd.Items.Add("http://hamsterdb.com/");
          listBoxPagesToAdd.Items.Add("http://www.firebirdsql.org/");
          listBoxPagesToAdd.Items.Add("http://www.h2database.com/");
          listBoxPagesToAdd.Items.Add("http://www.oracle.com/technology/products/berkeley-db");
          listBoxPagesToAdd.Items.Add("http://www.scimore.com/");
          listBoxPagesToAdd.Items.Add("http://www.stsdb.com/");
          listBoxPagesToAdd.Items.Add("http://www.sqlite.org/about.html");
          listBoxPagesToAdd.Items.Add("http://www.mysql.com/products/enterprise/techspec.html");
          listBoxPagesToAdd.Items.Add("http://www.objectivity.com");
          listBoxPagesToAdd.Items.Add("http://vistadb.net/");
          listBoxPagesToAdd.Items.Add("http://www.google.com/search?q=object+database&sourceid=ie7&rls=com.microsoft:en-us:IE-SearchBox&ie=&oe=");
        }
        indexRoot.Persist(session, indexRoot);
      }
      else
        indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));

      if (indexRoot.repository.documentSet.Count > 0)
      {
        List<Document> docs = indexRoot.repository.documentSet.ToList<Document>().Take(50).ToList<Document>();
        inDbListBox.ItemsSource = docs;
      }
      updateDataGrids(indexRoot);
      session.Commit();
      //verify();
    }

    public void createLocalInvertedIndex(Document doc, Word word, UInt64 wordCt, Placement wordPlacement, Placement wordHitPlacement)
    {
      WordHit wordHit;
      BTreeSetOidShort<Word> wordSet = doc.WordSet;
      if (wordSet.TryGetKey(word, ref word))
      {
        wordHit = doc.WordHit[word];
        wordHit.Add(wordCt);
      }
      else
      {
        word.Persist(wordPlacement, session);
        wordSet.Add(word);
        wordHit = new WordHit(doc, wordCt++, session);
        doc.WordHit.ValuePlacement = wordHitPlacement;
        doc.WordHit.AddFast(word, wordHit);
      }
    }

    public void createGlobalInvertedIndex(IndexRoot indexRoot)
    {
      Placement wordPlacement = new Placement(Lexicon.PlaceInDatabase, 2);
      BTreeSetOidShort<Word> wordSet = indexRoot.lexicon.WordSet;
      BTreeSet<Document> docSet = indexRoot.repository.documentSet;
      Word existingWord = null;
      foreach (Document doc in docSet)
      {
        if (doc.Indexed == false)
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
              wordSet.Add(existingWord);
            }
            existingWord.DocumentHit.AddFast(doc);
          }
          doc.Indexed = true;
        }
      }
    }

    public void textToWords(Document doc, IndexRoot indexRoot, string docTextString, Placement documentPlacement,
      Placement documentTextPlacement, Placement wordPlacement, Placement wordHitPlacement)
    {
      DocumentText docText = new DocumentText(docTextString, doc);
      Word word;
      doc.Persist(documentPlacement, session);
      doc.Page.Database.Name = doc.Name;
      docText.Persist(documentTextPlacement, session);
      indexRoot.repository.documentSet.Add(doc);
      doc.Content = docText;
      docTextString = docTextString.ToLower();
      string[] excludedWords = new string[] { "and", "the" };
      char[] splitChars = new char[] { ' ', '\n', '(', '"', '!', ',', '(', ')', '\t' };
      string[] words = docTextString.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
      UInt64 wordCt = 0;
      int i = 0;
      string aWord;
      char[] trimEndChars = new char[] { ';', '.', '"', ',', '\r', ':', ']', '!', '?', '+', '(', ')', '\'', '{', '}', '-', '`', '/', '=' };
      char[] trimStartChars = new char[] { ';', '&', '-', '#', '*', '[', '.', '"', ',', '\r', ')', '(', '\'', '{', '}', '-', '`' };
      foreach (string wordStr in words)
      {
        i++;
        aWord = wordStr.TrimEnd(trimEndChars);
        aWord = aWord.TrimStart(trimStartChars);
        word = new Word(aWord);
        if (aWord.Length > 1 && excludedWords.Contains(aWord) == false)
        {
          createLocalInvertedIndex(doc, word, wordCt, wordPlacement, wordHitPlacement);
          ++wordCt;
        }
      }
    }

    public Document parseHtml(string url, IndexRoot indexRoot)
    {
      Document doc = new Document(url, indexRoot, session);
      Placement docPlacement = new Placement(Document.PlaceInDatabase);
      Placement docTextPlacement = new Placement(Document.PlaceInDatabase, 2);
      Placement wordPlacement = new Placement(Document.PlaceInDatabase, 3);
      Placement wordHitPlacement = new Placement(Document.PlaceInDatabase, 100);
      using (WebClient client = new WebClient())
      {
        string html = client.DownloadString(url);
        string pageBody = "";
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//text()"))
          pageBody += " " + node.InnerText;
        textToWords(doc, indexRoot, pageBody, docPlacement, docTextPlacement, wordPlacement, wordHitPlacement);
      }
      return doc;
    }

    public Document parseTextFile(string url, IndexRoot indexRoot, Placement docPlacement)
    {
      Document doc = new Document(Path.GetFileName(url), indexRoot, session);
      Placement docTextPlacement = new Placement(docPlacement.TryDatabaseNumber, (ushort)(docPlacement.TryPageNumber + 1));
      Placement wordPlacement = new Placement(docPlacement.TryDatabaseNumber, (ushort)(docPlacement.TryPageNumber + 2));
      Placement wordHitPlacement = new Placement(docPlacement.TryDatabaseNumber, (ushort)(docPlacement.TryPageNumber + 10));
      using (StreamReader reader = new StreamReader(url))
      {
        textToWords(doc, indexRoot, reader.ReadToEnd(), docPlacement, docTextPlacement, wordPlacement, wordHitPlacement);
      }
      return doc;
    }

    void updateDataGrids(IndexRoot indexRoot, int indexOfRemoved = -1)
    {
      if (indexRoot == null)
        return;
      if (indexRoot.lexicon.WordSet.Count == 0)
        return;
      stackPanel.IsEnabled = false;
      bool aRefresh = stackPanel.Children.Count > 0;
      if (indexOfRemoved >= 0 && aRefresh)
        stackPanel.Children.RemoveAt(0);
      else if (stackPanel.Children.Count > 0)
        stackPanel.Children.Clear();
      DataGrid dataGrid = new DataGrid();
      dataGrid.AutoGenerateColumns = true;
      dataGrid.MaxColumnWidth = 150;
      dataGridList.Add(dataGrid);
      DataTable table = new DataTable("Word Count");
      DataColumn wordColumn = new DataColumn("Words (all pages)", Type.GetType("System.String"));
      DataColumn countColumn = new DataColumn("Count", Type.GetType("System.UInt32"));
      table.Columns.Add(wordColumn);
      table.Columns.Add(countColumn);
      DataRow newRow;
      int pageIndex = 0;
      int min = 3;
      int.TryParse(wordMinCt.Text, out min);
      foreach (Word word in indexRoot.lexicon.WordSet)
      {
        if (word.GlobalCount >= min)
        {
          newRow = table.NewRow();
          newRow[0] = word.aWord;
          newRow[1] = word.GlobalCount;
          table.Rows.Add(newRow);
        }
      }
      DataView dataView = new DataView(table);
      dataView.Sort = "Count desc";
      dataGrid.ItemsSource = dataView;
      stackPanel.Children.Insert(pageIndex++, dataGrid);
      if (indexOfRemoved >= 0 && aRefresh)
        stackPanel.Children.RemoveAt(indexOfRemoved + 1);
      else
      {
        List<Document> docs = indexRoot.repository.documentSet.ToList<Document>().ToList<Document>();
        foreach (Document page in docs)
        {
          DataTable pageTable = new DataTable();
          dataTableList.Add(pageTable);
          string pageName = page.url.TrimEnd('/');
          int index = pageName.IndexOf("//");
          if (index >= 0)
            pageName = pageName.Remove(0, index + 2);
          index = pageName.IndexOf("www.");
          if (index >= 0)
            pageName = pageName.Remove(0, index + 4);
          pageName = pageName.Replace('.', ' ');
          pageName = pageName.Replace('/', ' ');
          DataColumn wordColumnPage = new DataColumn(pageName, Type.GetType("System.String"));
          DataColumn countColumnPage = new DataColumn("Count", Type.GetType("System.Int32"));
          pageTable.Columns.Add(wordColumnPage);
          pageTable.Columns.Add(countColumnPage);
          foreach (KeyValuePair<Word, WordHit> pair in page.WordHit)
          {
            if ((int)pair.Value.Count >= min)
            {
              newRow = pageTable.NewRow();
              string aString = pair.Key.aWord;
              newRow.SetField<string>(wordColumnPage, aString);
              newRow.SetField<int>(countColumnPage, (int)pair.Value.Count);
              //wc.Add(new WordCount(aString, (uint) hit.wordPositionSet.Count));
              pageTable.Rows.Add(newRow);
            }
          }
          dataGrid = new DataGrid();
          dataGrid.AutoGenerateColumns = true;
          dataGrid.MaxColumnWidth = 150;
          dataGridList.Add(dataGrid);
          dataView = new DataView(pageTable);
          dataView.Sort = "Count desc";
          dataGrid.ItemsSource = dataView;
          stackPanel.Children.Insert(pageIndex++, dataGrid);
        }
      }
      stackPanel.IsEnabled = true;
    }


    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {

    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      session.BeginUpdate();
      IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
      Placement docPlacement = new Placement(Document.PlaceInDatabase);
      foreach (string str in listBoxPagesToAdd.Items)
      {
        Document doc = null;
        try
        {
          if (str.Contains(".html") || str.Contains(".htm") || str.Contains("http") || str.Contains("aspx"))
            doc = parseHtml(str, indexRoot);
          else
            doc = parseTextFile(str, indexRoot, docPlacement);
        }
        catch (WebException ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
      createGlobalInvertedIndex(indexRoot);
      listBoxPagesToAdd.Items.Clear();
      List<Document> docs = indexRoot.repository.documentSet.ToList<Document>().Take(50).ToList<Document>();
      inDbListBox.ItemsSource = docs;
      session.Commit();
      session.BeginRead();
      updateDataGrids(indexRoot);
      session.Commit();
    }

    private void addToPageListButton_Click(object sender, RoutedEventArgs e)
    {
      listBoxPagesToAdd.Items.Add(pageToAdd.Text);
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)e.Source;
      Document myItem = (Document)menuItem.DataContext;
      session.BeginUpdate();
      try
      {
        IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
        int index;
        if (indexRoot.repository.documentSet.TryGetKey(myItem, ref myItem))
          index = myItem.Remove(indexRoot, session);
        else
          index = -1; // weird case - should not happen
        inDbListBox.ItemsSource = indexRoot.repository.documentSet.ToList<Document>();
        updateDataGrids(indexRoot, index);
        session.Commit();
      }
      catch
      {
        session.Abort();
      }
    }

    public void verify(bool startTrans = true)
    {
      if (startTrans)
        session.BeginRead();
      IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
      int i = 0;
      int j = 0;
      int k = 0;
      foreach (Word word in indexRoot.lexicon.WordSet)
      {
        i++;
        foreach (Document doc in word.DocumentHit)
        {
          j++;
          if (doc == null)
            throw new UnexpectedException("bad documentHit BTreeSet");
          foreach (KeyValuePair<Word, WordHit> pair in doc.WordHit)
          {
            k++;
            if (pair.Value == null || pair.Key == null)
              throw new UnexpectedException("bad document WordHit");
            if (indexRoot.lexicon.WordSet.Contains(pair.Key) == false)
              throw new UnexpectedException("missing lexicon word");
          }
        }
      }
      if (startTrans)
        session.Commit();
    }

    private void updateWordTables_Click(object sender, RoutedEventArgs e)
    {
      if (session != null)
      {
        try
        {
          session.BeginRead();
          IndexRoot indexRoot = (IndexRoot)session.Open(Oid.Encode(IndexRoot.PlaceInDatabase, 1, 1));
          updateDataGrids(indexRoot);
        }
        finally
        {
          session.Commit();
        }
      }
    }
  }

  class WordCount : IComparable
  {
    public WordCount(string w, UInt32 c)
    {
      Word = w;
      Count = c;
    }
    public string Word { get; set; }
    public UInt32 Count { get; set; }

    public int CompareTo(object obj)
    {
      WordCount otherWordCount = obj as WordCount;
      if (otherWordCount != null)
      {
        int compare = otherWordCount.Count.CompareTo(Count);
        if (compare == 0)
          return Word.CompareTo(otherWordCount.Word);
        return compare;
      }
      else
        throw new ArgumentException("Object is not a WordCount");
    }

    public override string ToString()
    {
      return Word + "\t" + Count;
    }
  }
}
