using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace DatabaseManager
{
  public class SchemaListViewModel : TreeViewItemViewModel
  {
    readonly string listAsString;

    public SchemaListViewModel(IList list, TreeViewItemViewModel parentObject, Page page)
      : base(parentObject, true)
    {
      listAsString = OptimizedPersistable.ListToString(list, page);
    }
    public string ListName
    {
      get { return listAsString; }
    }

    public string ListDisplay
    {
      get
      {
        return listAsString;
      }
    }
  }
}
