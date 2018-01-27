using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class TypeVersionViewModel : TreeViewItemViewModel
  {
    readonly TypeVersion _typeVersion;
    readonly SessionBase m_session;

    public TypeVersionViewModel(TypeVersion typeVersion, TreeViewItemViewModel parentDatabase, SessionBase session)
      : base(parentDatabase, true)
    {
      _typeVersion = typeVersion;
      m_session = session;
    }

    public TypeVersionViewModel(TypeVersion typeVersion, TreeViewItemViewModel parentView, int arrayIndex, bool encodedOid, SessionBase session)
      : base(parentView, true)
    {
      // TO DO
    }

    public string TypeVersionName => _typeVersion.ToString();


    protected override void LoadChildren()
    {
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        if (_typeVersion.BaseShape != null)
          if (_typeVersion.BaseShape.Type == CommonTypes.s_typeOfOptimizedPersistable)
            base.Children.Add(new TypeVersionViewModelNoExpansion(_typeVersion.BaseShape, this, m_session));
        else
            base.Children.Add(new TypeVersionViewModel(_typeVersion.BaseShape, this, m_session));
        foreach (var field in _typeVersion.DataMemberArray)
        {
          if (field.HasId || field.WeakIOptimizedPersistableReference || field.Field.FieldType.ExpandsToNonPrimitiveTypes())
            base.Children.Add(new DataMemberViewModel(_typeVersion, field, this, m_session));
          else
            base.Children.Add(new DataMemberViewModelNoExpansion(_typeVersion, field, this, m_session));
        }
      }
    }
  }
}
