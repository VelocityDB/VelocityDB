using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDBExtensions;

namespace DatabaseManager
{
  public class FieldViewModelNoExpansions : TreeViewItemViewModel
  {
    readonly string fieldAsString;

    public FieldViewModelNoExpansions(IOptimizedPersistable parentObj, DataMember member, ObjectViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      fieldAsString = Utilities.ToStringDetails(member, parentObj.GetWrappedObject(), parentObj, parentObj.GetPage(), true);
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
