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
  public class DataMemberViewModelNoExpansion : TreeViewItemViewModel
  {
    readonly string m_fieldAsString;
    readonly UInt64 m_parentId;
    readonly DataMember m_member;
    readonly SessionBase m_session;
    readonly bool m_isEncodedOidArray;
    public DataMemberViewModelNoExpansion(TypeVersion parentObj, DataMember member, TypeVersionViewModel parentObject, SessionBase session)
      : base(parentObject, true)
    {
      m_member = member;
      m_session = session;
      m_parentId = parentObj.Id;
      m_isEncodedOidArray = parentObj.Type.IsAssignableFrom(typeof(BTreeNode)) && parentObj.Type.IsArray && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
      m_isEncodedOidArray = m_isEncodedOidArray || parentObj.GetType().IsGenericType && parentObj.GetType().GetGenericTypeDefinition() == typeof(WeakReferenceList<>);
      m_fieldAsString = member.ToString();
    }
    public DataMemberViewModelNoExpansion(TypeVersion parentObj, DataMember member, DataMemberViewModel parentObject, SessionBase session)
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

    public UInt64 ParentId
    {
      get
      {
        return m_parentId;
      }
    }
  }
}
