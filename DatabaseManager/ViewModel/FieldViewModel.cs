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

namespace DatabaseManager
{
  public class FieldViewModel : TreeViewItemViewModel
  {
    readonly string m_fieldAsString;
    readonly UInt64 m_parentId;
    readonly DataMember m_member;
    readonly SessionBase m_session;
    readonly bool m_isEncodedOidArray;
    public FieldViewModel(IOptimizedPersistable parentObj, DataMember member, ObjectViewModel parentObject, SessionBase session)
      : base(parentObject, true)
    {
      m_member = member;
      m_session = session;
      m_parentId = parentObj.Id; 
      object memberObj = member.GetMemberValue(parentObj.WrappedObject);
      m_isEncodedOidArray = (parentObj as BTreeNode) != null && memberObj != null && (memberObj as Array) != null && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
      m_isEncodedOidArray = m_isEncodedOidArray || parentObj.GetType().IsGenericType && parentObj.GetType().GetGenericTypeDefinition() == typeof(WeakReferenceList<>);
      m_fieldAsString = Utilities.ToStringDetails(member, parentObj.WrappedObject, parentObj, parentObj.Page, true);
    }

    public string FieldName
    {
      get { return m_fieldAsString; }
    }

    public string FieldDisplay
    {
      get
      {
        return m_fieldAsString;
      }
    }

    public Array GetFieldAsArray()
    {
      IOptimizedPersistable parentObj = (IOptimizedPersistable)m_session.Open(m_parentId, false, null, false, 0, Int32.MaxValue);
      object memberObj = m_member.GetMemberValue(parentObj.WrappedObject);
      return memberObj as Array;
    }

    protected override void LoadChildren()
    {
      IOptimizedPersistable parentObj = (IOptimizedPersistable) m_session.Open(m_parentId, false, null, false, 0, Int32.MaxValue);
      if (parentObj != null)
      {
        object memberObj = m_member.GetMemberValue(parentObj.WrappedObject);
        Array a = memberObj as Array;
        if (a != null)
        {
          Type elementType = a.GetType().GetElementType();
          TypeCode tCode = elementType.GetTypeCode();
          bool isValueType = elementType.GetTypeInfo().IsValueType;
          if ((isValueType || elementType.IsArray) && !m_isEncodedOidArray)
            base.Children.Add(new ArrayViewModelNoExpansions(a, this, m_isEncodedOidArray, parentObj.Page, m_session));
          else
          {
            int i = 0;
            foreach (object arrayObj in a)
            {
              if (arrayObj != null)
                base.Children.Add(new ObjectViewModel(arrayObj, this, i++, m_isEncodedOidArray, m_session));
            }
          }
        }
        else
        {
          IOptimizedPersistable p = memberObj as IOptimizedPersistable;
          if (p != null)
            base.Children.Add(new ObjectViewModel(p, this, m_session));
          else if (m_member.WeakIOptimizedPersistableReference)
          {
            WeakIOptimizedPersistableReferenceBase weakRef = memberObj as VelocityDb.WeakIOptimizedPersistableReferenceBase;
            p = m_session.Open(weakRef.Id, false, null, false, 0, Int32.MaxValue);
            base.Children.Add(new ObjectViewModel(p, this, m_session));
          }
          else
          {
            IList list = memberObj as IList;
            Type elementType = list.GetType();
            if (elementType.IsGenericType)
              elementType = elementType.GetGenericArguments()[0];
            else
              elementType = elementType.GetElementType();
            TypeCode tCode = elementType.GetTypeCode();
            bool isValueType = elementType.GetTypeInfo().IsValueType;
            if ((isValueType || elementType.IsArray) && !m_isEncodedOidArray)
              base.Children.Add(new ListViewModel(list, this, parentObj.Page));
            else
            {
              int i = 0;
              foreach (object arrayObj in list)
              {
                base.Children.Add(new ObjectViewModel(arrayObj, this, i++, m_isEncodedOidArray, m_session));
              }
            }
          }
        }
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
