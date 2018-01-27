using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class TypeVersionViewModelNoExpansion : TreeViewItemViewModel
  {
    readonly TypeVersion _typeVersion;
    readonly SessionBase m_session;

    public TypeVersionViewModelNoExpansion(TypeVersion typeVersion, TreeViewItemViewModel parentDatabase, SessionBase session)
      : base(parentDatabase, true)
    {
      _typeVersion = typeVersion;
      m_session = session;
    }

    public string TypeVersionName => _typeVersion.ToString();
  }
}
