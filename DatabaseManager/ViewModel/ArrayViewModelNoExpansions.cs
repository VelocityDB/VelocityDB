using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDBExtensions;

namespace DatabaseManager
{
  public class ArrayViewModelNoExpansions : TreeViewItemViewModel
  {
    readonly string m_arrayAsString;


    public ArrayViewModelNoExpansions(Array a, Type elementType, FieldViewModel parentObject, bool isEncodedOidArray, Page page, SessionBase session)
      : base(parentObject, true)
    {
      m_arrayAsString = Utilities.ArrayToString(a, isEncodedOidArray, page, elementType, "");
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
