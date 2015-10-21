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
