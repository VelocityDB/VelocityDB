using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace DatabaseManager
{
  public class ArrayViewModelNoExpansions : TreeViewItemViewModel
  {
    readonly string m_arrayAsString;


    public ArrayViewModelNoExpansions(Array a, FieldViewModel parentObject, bool isEncodedOidArray, Page page, SessionBase session)
      : base(parentObject, true)
    {
      m_arrayAsString = OptimizedPersistable.ArrayToString(a, isEncodedOidArray, page, "");
    }

    public string ArrayName
    {
      get { return m_arrayAsString; }
    }

    public string ArrayDisplay
    {
      get
      {
        return m_arrayAsString;
      }
    }
  }
}
