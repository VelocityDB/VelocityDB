using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbBrowser.ViewModel
{
  public class DatabaseViewModel : TreeViewItemViewModel
  {
    readonly Database _database;
    readonly SessionBase _session;

    public DatabaseViewModel(Database db, SessionBase session)
      : base(null, true)
    {
      _database = db;
      _session = session;
    }

    public string DatabaseName
    {
      get { return _database.ToString(); }
    }

    protected override void LoadChildren()
    {
      foreach (Page page in _database)
        base.Children.Add(new PageViewModel(page, this, _session));
    }
  }
}
