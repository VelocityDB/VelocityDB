using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DatabaseManager.Model;
using VelocityDb.Session;
using System.Net;
using VelocityDb;
using VelocityDb.Collection;

namespace DatabaseManager
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    AllFederationsViewModel m_viewModel;
    public MainWindow()
    {
      InitializeComponent();
      m_viewModel = new AllFederationsViewModel();
      base.DataContext = m_viewModel;
    }

    private void AddMenuItem_Click(object sender, RoutedEventArgs e)
    {
      FederationInfo info = new FederationInfo();
      ConnectionDialog popup = new ConnectionDialog(info);
      bool? result = popup.ShowDialog();
      if (result != null && result.Value)
      {
        SessionBase session = m_viewModel.ActiveSession;
        session.BeginUpdate();
        session.Persist(info);
        session.Commit();
        m_viewModel = new AllFederationsViewModel();
        base.DataContext = m_viewModel;
      }
    }

    private void Create1000TestObjectsMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      FederationViewModel view = (FederationViewModel)menuItem.DataContext;
      FederationInfo info = view.Federationinfo;
      SessionBase session = view.Session;
      if (session.InTransaction)
        session.Commit();
      session.BeginUpdate();
      try
      {
        for (int i = 0; i < 1000; i++)
        {
          VelocityDbList<OptimizedPersistable> list = new VelocityDbList<OptimizedPersistable>();
          //for (int j = 0; j < 10; j++)
          //  list.Add(new OptimizedPersistable());
          session.Persist(list);
        }
        session.Commit();
        m_viewModel = new AllFederationsViewModel();
        base.DataContext = m_viewModel;
      }
      catch (Exception ex)
      {
        session.Abort();
        MessageBox.Show(ex.Message);
      }
    }
    
    private void RemoveDatabaseLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      DatabaseLocationViewModel view = (DatabaseLocationViewModel)menuItem.DataContext;
      DatabaseLocation dbLocation = view.DatabaseLocation;
      SessionBase session = dbLocation.Session;
      if (session.InTransaction)
        session.Commit();
      session.BeginUpdate();
      try
      {
        session.DeleteLocation(dbLocation);
        session.Commit();
        m_viewModel = new AllFederationsViewModel();
        base.DataContext = m_viewModel;
      }
      catch (Exception ex)
      {
        session.Abort();
        MessageBox.Show(ex.Message);
      }
    }
    private void RestoreDatabaseLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      DatabaseLocationViewModel view = (DatabaseLocationViewModel)menuItem.DataContext;
      DatabaseLocation dbLocation = view.DatabaseLocation;
      SessionBase session = dbLocation.Session;
      if (session.InTransaction)
        session.Commit();
      //DatabaseLocationMutable newLocationMutable = new DatabaseLocationMutable(session);
      //newLocationMutable.DirectoryPath = dbLocation.DirectoryPath;
      //newLocationMutable.HostName = dbLocation.HostName;
      //var popup = new RestoreDialog(newLocationMutable);
      //bool? result = popup.ShowDialog();
      //if (result != null && result.Value)
      {
        dbLocation.Page = null; // fake it as a transient object before restore !
        dbLocation.Id = 0;      // be careful about doing this kind of make transient tricks, references from objects like this are still persistent.
       // if (session.OptimisticLocking) // && session.GetType() == typeof(ServerClientSession))
        {
         // session.Dispose();
         // session = new ServerClientSession(session.SystemDirectory, session.SystemHostName, 2000, false, false); // need to use pessimstic locking for restore
          // = new SessionNoServer(session.SystemDirectory); // need to use pessimstic locking for restore
        }
        session.BeginUpdate();
        try
        {
          session.RestoreFrom(dbLocation, DateTime.Now);
          session.Commit(false, true); // special flags when commit of a restore ...
          m_viewModel = new AllFederationsViewModel();
          base.DataContext = m_viewModel;
        }
        catch (Exception ex)
        {
          session.Abort();
          MessageBox.Show(ex.Message);
        }
      }
    }

    private void EditDatabaseLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      DatabaseLocationViewModel view = (DatabaseLocationViewModel)menuItem.DataContext;
      DatabaseLocation dbLocation = view.DatabaseLocation;
      SessionBase session = dbLocation.Session;
      DatabaseLocationMutable newLocationMutable = new DatabaseLocationMutable(session);
      newLocationMutable.BackupOfOrForLocation = dbLocation.BackupOfOrForLocation;
      newLocationMutable.CompressPages = dbLocation.CompressPages;
      newLocationMutable.PageEncryption = dbLocation.PageEncryption;
      newLocationMutable.StartDatabaseNumber = dbLocation.StartDatabaseNumber;
      newLocationMutable.EndDatabaseNumber = dbLocation.EndDatabaseNumber;
      newLocationMutable.IsBackupLocation = dbLocation.IsBackupLocation;
      newLocationMutable.DirectoryPath = dbLocation.DirectoryPath;
      newLocationMutable.HostName = dbLocation.HostName;
      var popup = new NewDatabaseLocationDialog(newLocationMutable, dbLocation);
      bool? result = popup.ShowDialog();
      if (result != null && result.Value)
      {
        try
        {
          DatabaseLocation newLocation = new DatabaseLocation(newLocationMutable.HostName, newLocationMutable.DirectoryPath, newLocationMutable.StartDatabaseNumber,
            newLocationMutable.EndDatabaseNumber, session, newLocationMutable.CompressPages, newLocationMutable.PageEncryption, newLocationMutable.IsBackupLocation,
            newLocationMutable.IsBackupLocation ? newLocationMutable.BackupOfOrForLocation : dbLocation.BackupOfOrForLocation);
          session.BeginUpdate();
          session.NewLocation(newLocation);
          session.Commit();
          m_viewModel = new AllFederationsViewModel();
          base.DataContext = m_viewModel;
        }
        catch (Exception ex)
        {
          session.Abort();
          MessageBox.Show(ex.Message);
        }
      }
    }

    private void NewDatabaseLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      FederationViewModel view = (FederationViewModel)menuItem.DataContext;
      FederationInfo info = view.Federationinfo;
      SessionBase session = view.Session;
      DatabaseLocationMutable newLocationMutable = new DatabaseLocationMutable(session);
      var popup = new NewDatabaseLocationDialog(newLocationMutable, null);
      bool? result = popup.ShowDialog();
      if (result != null && result.Value)
      {
        try
        {
          DatabaseLocation newLocation = new DatabaseLocation(newLocationMutable.HostName, newLocationMutable.DirectoryPath, newLocationMutable.StartDatabaseNumber,
            newLocationMutable.EndDatabaseNumber, session, newLocationMutable.CompressPages, newLocationMutable.PageEncryption, newLocationMutable.BackupOfOrForLocation != null,
            newLocationMutable.BackupOfOrForLocation);
          if (session.InTransaction)
            session.Commit();
          session.BeginUpdate();
          session.NewLocation(newLocation);
          session.Commit();
          m_viewModel = new AllFederationsViewModel();
          base.DataContext = m_viewModel;
        }
        catch (Exception ex)
        {
          session.Abort();
          MessageBox.Show(ex.Message);
        }
      }
    }

    private void CopyFederationMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      FederationViewModel view = (FederationViewModel)menuItem.DataContext;
      FederationInfo info = view.Federationinfo;
      SessionBase session = view.Session;
      var lDialog = new System.Windows.Forms.FolderBrowserDialog()
      {
        Description = "Choose Federation Copy Folder",
      };
      if (lDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        string copyDir = lDialog.SelectedPath;
        session.CopyAllDatabasesTo(copyDir);
        session = info.Session;
        session.BeginUpdate();
        FederationCopyInfo copyInfo = new FederationCopyInfo(Dns.GetHostName(), copyDir);
        session.Persist(copyInfo);
        info.Update();
        info.FederationCopies.Add(copyInfo);
        session.Commit();
      }
    }

    private void ValidateFederationMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      FederationViewModel view = (FederationViewModel)menuItem.DataContext;
      FederationInfo info = view.Federationinfo;
      SessionBase session = view.Session;
      session.Verify();
      session = info.Session;
      session.BeginUpdate();
      info.Update();
      info.Validated.Add(DateTime.Now);
      session.Commit();
      MessageBox.Show("Databases validated without errors, " + DateTime.Now);
    }

    private void RemoveFederationInfoMenuItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = (MenuItem)sender;
      FederationViewModel view = (FederationViewModel)menuItem.DataContext;
      FederationInfo info = view.Federationinfo;
      SessionBase session = info.Session;
      session.BeginUpdate();
      info.Unpersist(session);
      session.Commit();
      m_viewModel = new AllFederationsViewModel();
      base.DataContext = m_viewModel;
    }
  }
}
