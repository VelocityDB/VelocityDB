using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace DatabaseManager
{
  public class DatabaseViewModel : TreeViewItemViewModel
  {
    readonly SessionBase m_session;
    readonly UInt32 m_dbid;

    public DatabaseViewModel(DatabaseLocationViewModel databaseLocationView, Database database)
      : base(databaseLocationView, true)
    {
      m_session = database.Session;
      m_dbid = database.DatabaseNumber;
    }

    public string DatabaseName
    {
      get
      {
        Database db = m_session.OpenDatabase(m_dbid, false, false);
        if (db != null)
          return db.ToString();
        return "Database failed to open: " + m_dbid;
      }
    }

    protected override void LoadChildren()
    {
      Database db = m_session.OpenDatabase(m_dbid, false, false);
      if (db != null)
        foreach (Page page in db)
          base.Children.Add(new PageViewModel(page, this, m_session));
    }
  }
}
