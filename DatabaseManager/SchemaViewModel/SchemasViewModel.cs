using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class SchemasViewModel : TreeViewItemViewModel
  {
    SessionBase _session;
    bool _internalTypes;
    bool _hideWeakReferenceConnectionTypes;
    public SchemasViewModel(SessionBase session, bool internalTypes, FederationSchemaViewModel parentView)
      : base(parentView, true)
    {
      _session = session;
      _internalTypes = internalTypes;
    }

    public string SchemasName => _internalTypes ? "Internal built in types" : "User defined types";

    public bool HideWeakReferenceConnectionTypes
    {
      get
      {
        return _hideWeakReferenceConnectionTypes;
      }
      set
      {
        _hideWeakReferenceConnectionTypes = value;
        base.Children.Clear();
        LoadChildren();
      }
    }

    protected override void LoadChildren()
    {
      if (!_session.InTransaction)
        _session.BeginRead();
        var types = _session.OpenSchema(false).TypesByName.ToList();
        using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
        {
          foreach (var type in types)
            if (_internalTypes == _session.OpenSchema(false).IsExpandedInternalType(type.SlotNumber))
              base.Children.Add(new TypeViewModel(this, type));
        }
    }
  }
}
