using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WindowsPhoneSDCard.Resources;
using VelocityDb.Session;
using VelocityDb.Collection.BTree;
using VelocityDbSchema.Samples.AllSupportedSample;
using VelocityDb.Collection.Comparer;
using VelocityDb;

namespace WindowsPhoneSDCard
{

  public partial class MainPage : PhoneApplicationPage
  {
    // Constructor
    static readonly string systemDir = "SortedObjects"; // appended to SessionBase.BaseDatabasePath

    public MainPage()
    {
      InitializeComponent();

      // Sample code to localize the ApplicationBar
      //BuildLocalizedApplicationBar();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
        const UInt32 numberOfPersons = 10000;
        const ushort nodeMaxSize = 5000;
        const ushort comparisonByteArraySize = sizeof(UInt64); // enough room to hold entire idNumber of a Person
        const bool comparisonArrayIsCompleteKey = true;
        const bool addIdCompareIfEqual = false;
        Person person;
        session.BeginUpdate();
        session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.None;
        //mySession.SetTraceAllDbActivity();
        BTreeSet<string> stringSet = new BTreeSet<string>(null, session);
        BTreeSetOidShort<string> stringSetShort = new BTreeSetOidShort<string>(null, session);
        BTreeMap<string, string> stringMap = new BTreeMap<string, string>(null, session);
        BTreeMapOidShort<string, string> stringMapShort = new BTreeMapOidShort<string, string>(null, session);
        CompareByField<Person> compareByField = new CompareByField<Person>("idNumber", session, addIdCompareIfEqual);
        BTreeSet<Person> bTree = new BTreeSet<Person>(compareByField, session, nodeMaxSize, comparisonByteArraySize, comparisonArrayIsCompleteKey);
        session.Persist(bTree); // Persist the root of the BTree so that we have something persisted that can be flushed to disk if memory available becomes low
        for (int i = 0; i < numberOfPersons; i++)
        {
          person = new Person();
          // session.Persist(person);
          bTree.AddFast(person);
        }
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.UseExternalStorageApi = true;
        session.BeginRead();
        BTreeSet<Person> bTree = session.AllObjects<BTreeSet<Person>>().First();
        foreach (Person person in (IEnumerable<Person>)bTree)
        {
          if (person.IdNumber > 196988888791402)
          {
            Console.WriteLine(person);
            break;
          }
        }
        session.Commit();
      }
    }

    // Sample code for building a localized ApplicationBar
    //private void BuildLocalizedApplicationBar()
    //{
    //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
    //    ApplicationBar = new ApplicationBar();

    //    // Create a new button and set the text value to the localized string from AppResources.
    //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
    //    appBarButton.Text = AppResources.AppBarButtonText;
    //    ApplicationBar.Buttons.Add(appBarButton);

    //    // Create a new menu item with the localized string from AppResources.
    //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
    //    ApplicationBar.MenuItems.Add(appBarMenuItem);
    //}
  }
}