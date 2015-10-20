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
    readonly DatabaseLocation m_databaseLocation;

    public DatabaseLocationViewModel(DatabaseLocation location)
      : base(null, true)
    {
      m_databaseLocation = location;
    }

    public string LocationName
    {
      get
      {
        return "Host: \"" + m_databaseLocation.HostName + "\" Path: \"" + m_databaseLocation.DirectoryPath + "\" " + m_databaseLocation.Oid;
      }
    }

    protected override void LoadChildren()
    {
      base.Children.Add(new ObjectViewModel(m_databaseLocation, this, m_databaseLocation.Session));
      SortedSet<Database> dbSet = new SortedSet<Database>(m_databaseLocation.Session.OpenLocationDatabases(m_databaseLocation, false));
      foreach (Database database in dbSet)
        base.Children.Add(new DatabaseViewModel(this, database));
    }
  }
}
