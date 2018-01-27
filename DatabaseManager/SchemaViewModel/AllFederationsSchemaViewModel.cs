using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using DatabaseManager.Model;
using VelocityDb;
using VelocityDb.Session;
using System.Windows;
using System.IO;

namespace DatabaseManager
{
  public class AllFederationsSchemaViewModel
  {
    SessionBase m_session;
    List<FederationSchemaViewModel> m_federationViews;

    void Initialize()
    {
      SessionBase.BaseDatabasePath = Properties.Settings.Default.BaseDatabasePath;
      m_session = new SessionNoServer(Properties.Settings.Default.DatabaseManagerDirectory);
      try
      {
        m_session.BeginUpdate();
        List<FederationSchemaViewModel> federationInfos = new List<FederationSchemaViewModel>();
        List<FederationInfo> federationInfosToRemove = new List<FederationInfo>();
        foreach (FederationInfo info in m_session.AllObjects<FederationInfo>())
        {
          try
          {
            federationInfos.Add(new FederationSchemaViewModel(info));
          }
          catch (Exception ex)
          {
            if (MessageBox.Show(ex.Message + " for " + info.HostName + " " + info.SystemDbsPath + " Remove this Database?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
              federationInfosToRemove.Add(info);
          }
        }
        foreach (FederationInfo info in federationInfosToRemove)
          info.Unpersist(m_session);
        if (federationInfos.Count() == 0)
        {
          string host = Properties.Settings.Default.DatabaseManagerHost;
          if (host == null || host.Length == 0)
            host = Dns.GetHostName();
          FederationInfo info = new FederationInfo(host,
            Properties.Settings.Default.DatabaseManagerDirectory,
            Properties.Settings.Default.TcpIpPortNumber,
            Properties.Settings.Default.DoWindowsAuthentication,
            null,
            Properties.Settings.Default.WaitForLockMilliseconds,
            Properties.Settings.Default.UseClientServer,
            "Database Manager");
          m_session.Persist(info);
          m_session.Commit();
          federationInfos.Add(new FederationSchemaViewModel(info));
        }
        if (m_session.InTransaction)
          m_session.Commit();
        m_federationViews = federationInfos;
      }
      catch (Exception ex)
      {
        if (m_session.InTransaction)
          m_session.Abort();
        if (MessageBox.Show(ex.Message + " for " + SessionBase.LocalHost + " " + Properties.Settings.Default.DatabaseManagerDirectory + " Remove this Database?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
          DirectoryInfo dir = new DirectoryInfo(Properties.Settings.Default.DatabaseManagerDirectory);
          dir.Delete(true);
          Initialize();
        }
      }
    }

    public AllFederationsSchemaViewModel()
    {
      Initialize();
    }

    public DirectoryInfo Initialize(string dbFilePath)
    {
      if (dbFilePath != null && dbFilePath.Length > 0)
      {
        m_session.BeginRead();
        FileInfo dbFile = new FileInfo(dbFilePath);
        if (dbFile.Exists)
        {
          UInt32 dbNum = 0;
          UInt32.TryParse(dbFile.Name.Substring(0, dbFile.Name.IndexOf('.')), out dbNum);
          DirectoryInfo directory = dbFile.Directory;
          if (directory.GetFiles("0.odb").Length > 0)
          {
            bool foundIt = false;
            foreach (var info in m_federationViews)
            {
              if (SessionBase.IsSameHost(info.Federationinfo.HostName, SessionBase.LocalHost) &&
                  info.Federationinfo.SystemDbsPath.ToLower() == directory.FullName.ToLower())
              {
                foundIt = true;
                info.IsExpanded = true;
                foreach (var child in info.Children)
                {
                  if (child.GetType() == typeof(DatabaseLocationViewModel))
                  {
                    child.IsExpanded = true;
                    foreach (var dbView in child.Children)
                    {
                      DatabaseViewModel dbViewModel = dbView as DatabaseViewModel;
                      if (dbViewModel != null && dbViewModel.DatabaseNumber == dbNum)
                        dbView.IsExpanded = true;
                    }
                  }
                }

              }
            }
            if (foundIt == false)
            {
              return directory;
            }
          }
        }
        if (m_session.InTransaction)
          m_session.Commit();
      }
      return null;
    }

    public List<FederationSchemaViewModel> Federations
    {
      get { return m_federationViews; }
    }

    public SessionBase ActiveSession
    {
      get
      {
        return m_session;
      }
    }
  }
}
