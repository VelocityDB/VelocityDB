using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using DatabaseManager.Model;
using VelocityDb;
using VelocityDb.Session;

namespace DatabaseManager
{
  public class FederationViewModel : TreeViewItemViewModel
  {
    FederationInfo m_federationInfo;
    SessionBase m_session;
    bool _velocityGraphMode;

    public FederationViewModel(FederationInfo federationInfo) : base(null, true)
    {
      m_federationInfo = federationInfo;
      if (m_federationInfo.UsesServerClient || (SessionBase.IsSameHost(m_federationInfo.HostName, SessionBase.LocalHost) == false))
        m_session = new ServerClientSession(m_federationInfo.SystemDbsPath, m_federationInfo.HostName, m_federationInfo.WaitForMilliSeconds, m_federationInfo.UsePessimisticLocking == false);
      else
        m_session = new SessionNoServer(m_federationInfo.SystemDbsPath, m_federationInfo.WaitForMilliSeconds, m_federationInfo.UsePessimisticLocking == false);
      m_session.BeginRead();
    }

    public string FederationName
    {
      get
      {
        return "Host: \"" + m_federationInfo.HostName + "\" Path: \"" + m_federationInfo.SystemDbsPath + "\" " + m_federationInfo.Oid;
      }
    }

    public FederationInfo Federationinfo
    {
      get
      {
        return m_federationInfo;
      }
    }

    public SessionBase Session
    {
      get
      {
        return m_session;
      }
    }

    public bool VelocityGraphMode
    {
      get
      {
        return _velocityGraphMode;
      }
      set
      {
        _velocityGraphMode = value;
        base.Children.Clear();
        LoadChildren();
      }
    }

    protected override void LoadChildren()
    {
      if (VelocityGraphMode)
      {
        foreach (var graph in m_session.AllObjects<VelocityGraph.Graph>())
        {
          base.Children.Add(new VelocityGraphViewModel(graph));
        }
      }
      else
      {
        base.Children.Add(new ObjectViewModel(m_federationInfo, this, m_session));
        DatabaseLocations locations = m_session.DatabaseLocations;
        foreach (DatabaseLocation location in locations)
          base.Children.Add(new DatabaseLocationViewModel(location, Properties.Settings.Default.OrderDatabasesByName));
      }
    }
  }
}
