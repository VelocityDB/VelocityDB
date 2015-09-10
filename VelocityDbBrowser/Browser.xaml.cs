using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbBrowser.ViewModel;

namespace VelocityDbBrowser
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Browser : Window
  {
    SessionBase session = null;

    public Browser()
    {
      InitializeComponent();
      systemDatabaseServer.Text = Dns.GetHostName();
      string dir = ChooseDatabaseDirectory();
      if (dir == null)
        systemDatabaseDirectory.Text = SessionBase.BaseDatabasePath;
      else
        systemDatabaseDirectory.Text = dir;
    }

    ~Browser()
    {
      if (session != null)
      {
        session.Dispose();
      }
    }

    string ChooseDatabaseDirectory()
    {
      FolderBrowserDialog chooseFolder = new FolderBrowserDialog();
      chooseFolder.Description = "Choose an existing VelocityDB Databases folder";
      chooseFolder.SelectedPath = SessionBase.BaseDatabasePath;
      DialogResult result = chooseFolder.ShowDialog();
      if (result == System.Windows.Forms.DialogResult.OK)
        return chooseFolder.SelectedPath;
      return null;
    }

    private void browseButton_Click(object sender, RoutedEventArgs e)
    {
      errorMessage.Content = null;
      try
      {
        SessionBase.DoWindowsAuthentication = (bool)UseWindowsAuthentication.IsChecked;
        if (this.noServerButton.IsChecked == true)
        {
          session = new SessionNoServer(systemDatabaseDirectory.Text, 2000, (bool) OptimisticLocking.IsChecked);
        }
        else
        {
          session = new ServerClientSession(systemDatabaseDirectory.Text, systemDatabaseServer.Text, 2000, (bool) OptimisticLocking.IsChecked);
        }
        session.BeginRead();
        List<Database> dbList = session.OpenAllDatabases(); // keep a reference to each db so they don't get garbage collected
        FederationViewModel viewModel = new FederationViewModel(session.Databases, session);
        base.DataContext = viewModel;
      }
      catch (Exception ex)
      {
        errorMessage.Content = ex.Message == null ? ex.ToString() :  ex.Message;
      }
    }

    private void systemDatabaseDirectory_GotFocus(object sender, RoutedEventArgs e)
    {
      try
      {
        if (this.localServerButton.IsChecked == true || this.noServerButton.IsChecked == true)
        {
          string dir = ChooseDatabaseDirectory();
          if (dir != null)
            systemDatabaseDirectory.Text = dir;  
        }
      }
      catch (Exception ex)
      {
        errorMessage.Content = ex.Message == null ? ex.ToString() : ex.Message;
      }
    }
  }
}
