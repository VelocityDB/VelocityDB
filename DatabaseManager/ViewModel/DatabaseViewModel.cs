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
          return db.ToString().Remove(0, 10);
        return "Database failed to open: " + m_dbid;
      }
    }

    public UInt32 DatabaseNumber
    {
      get
      {
        return m_dbid;
      }
    }

    protected override void LoadChildren()
    {
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        Database db = m_session.OpenDatabase(m_dbid, false, false);
        if (db != null)
          foreach (var page in db.Pages(true))
            base.Children.Add(new PageViewModel(page, this, m_session));
      }
    }
  }
}
