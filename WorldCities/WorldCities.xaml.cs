using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.WorldCities;

namespace WorldCities
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    static readonly string s_systemDir = "WorldCities"; // appended to SessionBase.BaseDatabasePath

    public MainWindow()
    {
      InitializeComponent();
      errorMessage.Content = null;
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          try
          {
            session.BeginRead();
            loadCities(session);
            session.Commit();
          }
          catch (SystemDatabaseNotFoundWithReadonlyTransactionException)
          {
          }
        }
      }
      catch (Exception ex)
      {
        errorMessage.Content = ex.Message == null ? ex.ToString() : ex.Message;
      }
    }

    void loadCities(SessionBase session)
    {
      Database db = session.OpenDatabase(session.DatabaseNumberOf(typeof(City)));
      if (db != null)
      {
        var src = from City city in db.AllObjects<City>() select city;
        CityName.ItemsSource = src;
        Country.ItemsSource = src;
        Latitude.ItemsSource = src;
        Longitude.ItemsSource = src;
        //State.ItemsSource = src;
        //Population.ItemsSource = src;
        CreateDatabasePanel.Visibility = System.Windows.Visibility.Collapsed;
        HelpLabel.Visibility = System.Windows.Visibility.Collapsed;
      }
    }

    private void worldCitiesTextFile_GotFocus(object sender, RoutedEventArgs e)
    {
      try
      {
        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        openFileDialog.InitialDirectory = "c:\\";
        openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 2;
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          worldCitiesTextFile.Text = openFileDialog.FileName;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    private void loadButton_Click(object sender, RoutedEventArgs e)
    {
      errorMessage.Content = null;
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          session.BeginUpdate();
          string line;
          using (StreamReader stream = new StreamReader(worldCitiesTextFile.Text, true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              string[] fields = line.Split(',');
              City city = new City(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6]);
              session.Persist(city);
            }
          }
          loadCities(session);
          session.Commit();
        }
      }
      catch (Exception ex)
      {
        errorMessage.Content = ex.Message == null ? ex.ToString() : ex.Message;
      }
    }

    void updateListBox(ListBox listBox, ListBox cityListBox, ListBox countryListBox, ListBox longitudeListBox, ListBox latitudeListBox)
    {
      int i = 0;
      foreach (City cityMain in listBox.Items)
      {
        ListBoxItem listBoxItem = (ListBoxItem)(listBox.ItemContainerGenerator.ContainerFromIndex(i++));
        if (listBoxItem == null)
          continue;
        bool matchCityName = cityListBox == null || cityListBox.SelectedItems.Count == 0;
        bool matchCountry = countryListBox == null || countryListBox.SelectedItems.Count == 0;
        bool matchLatitude = latitudeListBox == null || latitudeListBox.SelectedItems.Count == 0;
        bool matchLongitude = longitudeListBox == null || longitudeListBox.SelectedItems.Count == 0;

        if (matchCityName == false)
          foreach (City city in cityListBox.SelectedItems)
            if (city.CityName == cityMain.CityName)
            {
              matchCityName = true;
              break;
            }
        if (matchCountry == false)
          foreach (City city in countryListBox.SelectedItems)
            if (city.CountryCode == cityMain.CountryCode)
            {
              matchCountry = true;
              break;
            }
        if (matchLongitude == false)
          foreach (City city in Longitude.SelectedItems)
            if (city.Longitude == cityMain.Longitude)
            {
              matchLongitude = true;
              break;
            }
        if (matchLatitude == false)
          foreach (City city in Latitude.SelectedItems)
            if (city.Latitude == cityMain.Latitude)
            {
              matchLatitude = true;
              break;
            }

        if (matchCountry && matchCityName && matchLatitude && matchLongitude)
        {
          if (listBoxItem.IsSelected == false)
            listBoxItem.Background = Brushes.White;
        }
        else
        {
          if (listBoxItem.IsSelected)
          {
            listBoxItem.IsSelected = false;
            updateListBox(listBox, cityListBox, countryListBox, longitudeListBox, latitudeListBox);
            break;
          }
          listBoxItem.Background = Brushes.LightGray;
        }
      }
    }

    private void CityName_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateListBox(Country, CityName, Country, Longitude, Latitude);
      updateListBox(Longitude, CityName, Country, Longitude, Latitude);
      updateListBox(Latitude, CityName, Country, Longitude, Latitude);
      updateListBox(CityName, CityName, Country, Longitude, Latitude);
    }

    private void Country_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateListBox(CityName, CityName, Country, Longitude, Latitude);
      updateListBox(Longitude, CityName, Country, Longitude, Latitude);
      updateListBox(Latitude, CityName, Country, Longitude, Latitude);
      updateListBox(Country, CityName, Country, Longitude, Latitude);
    }

    private void Latitude_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateListBox(CityName, CityName, Country, Longitude, Latitude);
      updateListBox(Country, CityName, Country, Longitude, Latitude);
      updateListBox(Longitude, CityName, Country, Longitude, Latitude);
      updateListBox(Latitude, CityName, Country, Longitude, Latitude);
    }

    private void Longitude_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateListBox(CityName, CityName, Country, Longitude, Latitude);
      updateListBox(Country, CityName, Country, Longitude, Latitude);
      updateListBox(Latitude, CityName, Country, Longitude, Latitude);
      updateListBox(Longitude, CityName, Country, Longitude, Latitude);
    }

    private void CityName_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      updateListBox(CityName, null, Country, Longitude, Latitude);
    }

    private void Country_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      updateListBox(Country, CityName, null, Longitude, Latitude);
    }

    private void Latitude_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      updateListBox(Latitude, CityName, Country, Longitude, null);
    }

    private void Longitude_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      updateListBox(Longitude, CityName, Country, null, Latitude);
    }
  }
}
