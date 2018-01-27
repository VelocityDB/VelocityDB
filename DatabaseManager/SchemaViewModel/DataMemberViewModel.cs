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
  public class DataMemberViewModel : TreeViewItemViewModel
  {
    readonly string m_fieldAsString;
    readonly UInt64 m_parentId;
    readonly DataMember m_member;
    readonly SessionBase m_session;
    readonly bool m_isEncodedOidArray;
    public DataMemberViewModel(TypeVersion parentObj, DataMember member, TypeVersionViewModel parentObject, SessionBase session)
      : base(parentObject, true)
    {
      m_member = member;
      m_session = session;
      m_parentId = parentObj.Id;
      m_isEncodedOidArray = parentObj.Type.IsAssignableFrom(typeof(BTreeNode)) && parentObj.Type.IsArray && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
      m_isEncodedOidArray = m_isEncodedOidArray || parentObj.GetType().IsGenericType && parentObj.GetType().GetGenericTypeDefinition() == typeof(WeakReferenceList<>);
      m_fieldAsString = member.ToString();
    }
    public DataMemberViewModel(TypeVersion parentObj, DataMember member, DataMemberViewModel parentObject, SessionBase session)
  : base(parentObject, true)
    {
      m_member = member;
      m_session = session;
      m_parentId = parentObj.Id;
      m_isEncodedOidArray = parentObj.Type.IsAssignableFrom(typeof(BTreeNode)) && parentObj.Type.IsArray && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
      m_isEncodedOidArray = m_isEncodedOidArray || parentObj.GetType().IsGenericType && parentObj.GetType().GetGenericTypeDefinition() == typeof(WeakReferenceList<>);
      m_fieldAsString = member.ToString();
    }

    public string DataMemberName
    {
      get
      {
        return $"{m_member.FieldName} {m_member.FieldType.ToGenericTypeString()}";
      }
    }

    public Array GetFieldAsArray()
    {
      TypeVersion parentObj = m_session.Open<TypeVersion>(m_parentId);
      return null; // TO DO, FIX IT!
    }

    protected override void LoadChildren()
    {
      TypeVersion parentObj = (TypeVersion)m_session.Open(m_parentId, false, null, false, 0, Int32.MaxValue);
      if (parentObj != null)
      {
        VelocityDbType vdbType;
        Type t = m_member.FieldType;
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
        var lookupByType = m_session.OpenSchema(false).LookupByType;
        if (t != null && lookupByType.TryGetValue(t, out vdbType))
        {
          foreach (VelocityDbType vType in m_session.OpenSchema(false).TypesByName)
          {
            if (vType != null && vType.Type != null)
            {
              var type = vType.Type;
              if (type.IsSubclassOf(vdbType.Type) || type == t)
                for (int i = 0; i < vType.TypeVersion.Length; i++)
                  base.Children.Add(new TypeVersionViewModel(vType.TypeVersion[i], this, m_session));
            }
            else
              Trace.WriteLine("vType or vType.type is unexpectedly null for VelocityDbType: " + vType);
          }
        }
        else
          base.Children.Add(new NotInSchemaViewModel("Type not registered in database schema", this, m_session));
      }
    }

    public UInt64 ParentId
    {
      get
      {
        return m_parentId;
      }
    }
  }
}
