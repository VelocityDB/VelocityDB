using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class TypeViewModel : TreeViewItemViewModel
  {
    readonly VelocityDbType _type;
    readonly SchemasViewModel _schemasViewModel;
    public TypeViewModel(SchemasViewModel schemaViewModel, VelocityDbType type)
      : base(schemaViewModel, true)
    {
      _type = type;
      _schemasViewModel = schemaViewModel;
    }

    public string TypeName
    {
      get
      {
        return _type.Type.ToGenericTypeString();
      }
    }

    protected override void LoadChildren()
    {
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        if (_type != null)
          foreach (var typeVersion in _type.TypeVersions)
            base.Children.Add(new TypeVersionViewModel(typeVersion, this, _schemasViewModel));
      }
    }
  }
}
