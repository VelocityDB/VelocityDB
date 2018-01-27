using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class NotInSchemaViewModel : TreeViewItemViewModel
  {
    readonly string _message;
    readonly SessionBase m_session;

    public NotInSchemaViewModel(string message, TreeViewItemViewModel parentDatabase, SessionBase session)
      : base(parentDatabase, true)
    {
      _message = message;
      m_session = session;
    }

    public string Message => _message;
  }
}
