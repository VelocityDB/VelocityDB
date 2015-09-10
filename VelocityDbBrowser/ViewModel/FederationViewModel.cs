using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbBrowser.ViewModel
{
  public class FederationViewModel
  {
    readonly ReadOnlyCollection<DatabaseViewModel> _databases;

    public FederationViewModel(IList<Database> databases, SessionBase session)
    {
      _databases = new ReadOnlyCollection<DatabaseViewModel>(
        (from database in databases
         select new DatabaseViewModel(database, session))
         .ToList());
    }

    public ReadOnlyCollection<DatabaseViewModel> Databases
    {
      get { return _databases; }
    }
  }
}
