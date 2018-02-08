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
    readonly SchemasViewModel _schemaViewModel;

    public TypeVersionViewModel(TypeVersion typeVersion, TreeViewItemViewModel parentDatabase, SchemasViewModel schemaViewModel)
      : base(parentDatabase, true)
    {
      _typeVersion = typeVersion;
      _schemaViewModel = schemaViewModel;
    }

    //public TypeVersionViewModel(TypeVersion typeVersion, TreeViewItemViewModel parentView, int arrayIndex, bool encodedOid, SessionBase session)
    //  : base(parentView, true)
    //{
    //  // TO DO
    //}

    public string TypeVersionName => _typeVersion.ToString();


    protected override void LoadChildren()
    {
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        if (_typeVersion.BaseShape != null)
          if (_typeVersion.BaseShape.Type == CommonTypes.s_typeOfOptimizedPersistable)
            base.Children.Add(new TypeVersionViewModelNoExpansion(_typeVersion.BaseShape, this, _typeVersion.Session));
        else
            base.Children.Add(new TypeVersionViewModel(_typeVersion.BaseShape, this, _schemaViewModel));
        foreach (var field in _typeVersion.DataMemberArray)
        {
          if (field.HasId || field.WeakIOptimizedPersistableReference || field.Field.FieldType.ExpandsToNonPrimitiveTypes())
            base.Children.Add(new DataMemberViewModel(_typeVersion, field, this, _typeVersion.Session, _schemaViewModel));
          else
            base.Children.Add(new DataMemberViewModelNoExpansion(_typeVersion, field, this, _typeVersion.Session));
        }
        List<Type> weakReferencedTypes;
        if (!_schemaViewModel.HideWeakReferenceConnectionTypes && VelocityDb.TypeInfo.Schema.WeakReferencedTypes.TryGetValue(_typeVersion.Type, out weakReferencedTypes))
        {
          foreach (Type type in weakReferencedTypes)
          {
            base.Children.Add(new WeakReferencedTypeViewModel(_typeVersion, type, this, _typeVersion.Session, _schemaViewModel));
          }
        }
      }
    }
  }
}
