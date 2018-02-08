using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection;
using VelocityDBExtensions;
using System.Diagnostics;

namespace DatabaseManager
{
  public class WeakReferencedTypeViewModel : TreeViewItemViewModel
  {
    readonly UInt64 _parentId;
    readonly Type _weakReferencedType;
    readonly SessionBase _session;
    readonly bool _isEncodedOidArray;
    readonly SchemasViewModel _schemasViewModel;
    public WeakReferencedTypeViewModel(TypeVersion parentObj, Type weakReferencedType, TypeVersionViewModel parentObject, SessionBase session, SchemasViewModel schemasViewModel)
      : base(parentObject, true)
    {
      _weakReferencedType = weakReferencedType;
      _session = session;
      _parentId = parentObj.Id;
      _schemasViewModel = schemasViewModel;
      //_isEncodedOidArray = parentObj.Type.IsAssignableFrom(typeof(BTreeNode)) && parentObj.Type.IsArray && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
      _isEncodedOidArray = _isEncodedOidArray || parentObj.GetType().IsGenericType && parentObj.GetType().GetGenericTypeDefinition() == typeof(WeakReferenceList<>);
    }
  //  public WeakReferencedTypeViewModel(TypeVersion parentObj, Type weakReferencedType, DataMemberViewModel parentObject, SessionBase session, SchemasViewModel schemasViewModel)
  //: base(parentObject, true)
  //  {
  //    _weakReferencedType = weakReferencedType;
  //    _session = session;
  //    _parentId = parentObj.Id;
  //    //_isEncodedOidArray = parentObj.Type.IsAssignableFrom(typeof(BTreeNode)) && parentObj.Type.IsArray && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
  //    _isEncodedOidArray = _isEncodedOidArray || parentObj.GetType().IsGenericType && parentObj.GetType().GetGenericTypeDefinition() == typeof(WeakReferenceList<>);
  //  }

    public string Description
    {
      get
      {
        return $"Weak Reference to {_weakReferencedType}";
      }
    }

    public Array GetFieldAsArray()
    {
      TypeVersion parentObj = _session.Open<TypeVersion>(_parentId);
      return null; // TO DO, FIX IT!
    }

    protected override void LoadChildren()
    {
      TypeVersion parentObj = (TypeVersion)_session.Open(_parentId, false, null, false, 0, Int32.MaxValue);
      if (parentObj != null)
      {
        VelocityDbType vdbType;
        Type t = _weakReferencedType;
        while (t.IsArray)
          t = t.GetElementType();
        TypeCode tCode = t.GetTypeCode();
        bool isValueType = t.IsValueType;
        bool isNullableElement = t.IsGenericType && t.GetGenericTypeDefinition() == CommonTypes.s_typeOfNullable;
        t = isNullableElement ? t.GetGenericArguments()[0] : t.IsEnum ? Enum.GetUnderlyingType(t) : t;
        if (t.IsArray)
          t = t.GetElementType();
        if (t.IsGenericType)
        {
          Type genericTypeDef = t.GetGenericTypeDefinition();
          Type key = t.GetGenericArguments()[0];
          if (genericTypeDef == CommonTypes.s_typeOfList)
            t = key;
        }
        var lookupByType = _session.OpenSchema(false).LookupByType;
        if (t != null && lookupByType.TryGetValue(t, out vdbType))
        {
          foreach (VelocityDbType vType in _session.OpenSchema(false).TypesByName)
          {
            if (vType != null && vType.Type != null)
            {
              var type = vType.Type;
              if (type.IsSubclassOf(vdbType.Type) || type == t)
                for (int i = 0; i < vType.TypeVersion.Length; i++)
                  base.Children.Add(new TypeVersionViewModel(vType.TypeVersion[i], this, _schemasViewModel));
            }
            else
              Trace.WriteLine("vType or vType.type is unexpectedly null for VelocityDbType: " + vType);
          }
        }
        else
          base.Children.Add(new NotInSchemaViewModel("Type not registered in database schema", this, _session));
      }
    }

    public UInt64 ParentId
    {
      get
      {
        return _parentId;
      }
    }
  }
}
