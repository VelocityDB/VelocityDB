using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbBrowser.ViewModel
{
  public class ArrayViewModel : TreeViewItemViewModel
  {
    readonly string arrayAsString;

    public ArrayViewModel(Array a, FieldViewModel parentObject, bool isEncodedOidArray, Page page, SessionBase session)
      : base(parentObject, true)
    {
      arrayAsString = OptimizedPersistable.ArrayToString(a, isEncodedOidArray, page, "");
    }
    public string ArrayName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return arrayAsString; }
    }

    public string ArrayDisplay
    {
      get
      {
        //return _object.ToStringDetails(_schema);
        return arrayAsString;
      }
    }
  }
}
