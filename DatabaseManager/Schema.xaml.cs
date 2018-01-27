using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDBExtensions;

namespace DatabaseManager
{
  /// <summary>
  /// Interaction logic for Schema.xaml
  /// </summary>
  public partial class Schema : Window
  {
    AllFederationsSchemaViewModel m_viewModel;
    public Schema(string dbFilePath)
    {
      InitializeComponent();
      m_viewModel = new AllFederationsSchemaViewModel();
      DirectoryInfo dirInfo = m_viewModel.Initialize(dbFilePath);
      //DataCache.MaximumMemoryUse = 3000000000; // 3 GB, set this to what fits your case
      bool addedFd = false;
      if (dirInfo != null)
        addedFd = AddFederation(dirInfo);
      if (addedFd == false)
        base.DataContext = m_viewModel;
    }

    bool AddFederation(DirectoryInfo dirInfo)
    {
      FederationInfo info = new FederationInfo();
      if (dirInfo != null)
        info.SystemDbsPath = dirInfo.FullName;
      ConnectionDialog popup = new ConnectionDialog(info);
      bool? result = popup.ShowDialog();
      if (result != null && result.Value)
      {
        if (info.HostName == null || info.HostName.Length == 0)
          info.HostName = SessionBase.LocalHost;
        SessionBase session = m_viewModel.ActiveSession;
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
        session.Persist(info);
        session.Commit();
        m_viewModel = new AllFederationsSchemaViewModel();
        base.DataContext = m_viewModel;
        return true;
      }
      return false;
    }
  }
}
