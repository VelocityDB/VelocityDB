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
    readonly Database m_database;

    public DatabaseViewModel(DatabaseLocationViewModel databaseLocationView, Database database)
      : base(databaseLocationView, true)
    {
      m_database = database;
    }

    public string DatabaseName
    {
      get
      {
        return m_database.ToString();
      }
    }

    protected override void LoadChildren()
    {
      foreach (Page page in m_database)
        base.Children.Add(new PageViewModel(page, this, m_database.Session));
    }
  }
}
