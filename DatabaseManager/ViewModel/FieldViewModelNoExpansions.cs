using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class FieldViewModelNoExpansions : TreeViewItemViewModel
  {
    readonly string fieldAsString;

    public FieldViewModelNoExpansions(IOptimizedPersistable parentObj, DataMember member, ObjectViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      fieldAsString = OptimizedPersistable.ToStringDetails(member, parentObj.WrappedObject, parentObj, parentObj.Page, true);
    }

    public string FieldName
    {
      get { return fieldAsString; }
    }

    public string FieldDisplay
    {
      get
      {
        return fieldAsString;
      }
    }
  }
}
