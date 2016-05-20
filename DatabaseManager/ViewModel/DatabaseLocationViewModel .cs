using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace DatabaseManager
{
  public class DatabaseLocationViewModel : TreeViewItemViewModel
  {
    DatabaseLocation m_databaseLocation;
    bool m_orderDatabasesByName;

    public DatabaseLocationViewModel(DatabaseLocation location, bool orderDatabasesByName)
      : base(null, true)
    {
      m_databaseLocation = location;
      m_orderDatabasesByName = orderDatabasesByName;
    }

    public DatabaseLocation DatabaseLocation
    {
      get
      {
        return m_databaseLocation;
      }
    }
    public IList<DatabaseLocation> DatabaseLocations
    {
      get
      {
        return m_databaseLocation.Session.DatabaseLocations.ToList();
      }
    }

    public string LocationName
    {
      get
      {
        return "Host: \"" + m_databaseLocation.HostName + "\" Path: \"" + m_databaseLocation.DirectoryPath + "\" " + m_databaseLocation.Oid;
      }
    }

    public bool IsBackupLocation
    {
      get
      {
        return m_databaseLocation.IsBackupLocation;
      }
    }

    public bool OrderDatabasesByName
    {
      get
      {
        return m_orderDatabasesByName;
      }
      set
      {
        m_orderDatabasesByName = value;
        base.Children.Clear();
        LoadChildren();
      }
    }

    protected override void LoadChildren()
    {
      base.Children.Add(new ObjectViewModel(m_databaseLocation, this, m_databaseLocation.Session));
      IOrderedEnumerable<Database> dbs;
      if (!m_databaseLocation.Session.InTransaction)
        m_databaseLocation.Session.BeginRead();
      if (m_orderDatabasesByName)
        dbs = m_databaseLocation.Session.OpenLocationDatabases(m_databaseLocation, false).OrderBy(db => db.Name);
      else
        dbs = m_databaseLocation.Session.OpenLocationDatabases(m_databaseLocation, false).OrderBy(db => db.Id);
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        foreach (Database database in dbs)
          base.Children.Add(new DatabaseViewModel(this, database));
      }
    }
  }
}
