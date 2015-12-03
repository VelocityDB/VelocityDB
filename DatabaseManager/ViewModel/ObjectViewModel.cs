﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
 public class ObjectViewModel : TreeViewItemViewModel
  {
    readonly UInt64 m_objectId;
    readonly SessionBase m_session;
    readonly string m_objectAsString;

    public ObjectViewModel(IOptimizedPersistable obj, TreeViewItemViewModel parentPage, SessionBase session)
      : base(parentPage, true)
    {
     m_objectId = obj.Id;
     m_session = session;
     if (obj.WrappedObject != obj)
       m_objectAsString = obj.WrappedObject.ToString() + " " + new Oid(obj.Id);
     else
       m_objectAsString = obj.ToString();
    }
    public ObjectViewModel(IOptimizedPersistable obj, FieldViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      m_objectId = obj.Id;
      m_session = session;
      if (obj.WrappedObject != obj)
        m_objectAsString = obj.WrappedObject.ToString() + " " + new Oid(obj.Id);
      else
        m_objectAsString = obj.ToString();
    }
    public string ObjectName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return m_objectAsString; }
    }

    public string ObjectDisplay
    {
      get 
      {
        //return _object.ToStringDetails(_schema);
        return m_objectAsString;
      }
    }

    void LoadChild(DataMember member, object memberObj)
    {
        bool listWithItems = false;
        if (member.Field != null && member.Field.FieldType.IsGenericType && member.Field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
          IList list = (IList)memberObj;
          listWithItems = list != null && list.Count > 0;
        }
        IOptimizedPersistable pObj = (IOptimizedPersistable) m_session.Open(m_objectId);
        if (pObj != null)
        {
          if (member.Field != null && memberObj != null & (member.Field.FieldType.IsArray || member.HasId || listWithItems))
            base.Children.Add(new FieldViewModel(pObj, member, this, m_session));
          else
            base.Children.Add(new FieldViewModelNoExpansions(pObj, member, this, m_session));
        }
    }

    protected override void LoadChildren()
    {
      IOptimizedPersistable pObj = (IOptimizedPersistable)m_session.Open(m_objectId);
      if (pObj != null)
      {
        pObj.LoadFields();
        object o = pObj.WrappedObject;
        TypeVersion baseShape = pObj.Shape.BaseShape;
        while (baseShape != null)
        {
          foreach (DataMember member in baseShape.DataMemberArray)
          {
            object memberObj = member.GetMemberValue(o);
            LoadChild(member, memberObj);
          }
          baseShape = baseShape.BaseShape;
        }
        foreach (DataMember member in pObj.Shape.DataMemberArray)
        {
          object memberObj = member.GetMemberValue(o);
          LoadChild(member, memberObj);
        }
      }
    }
  }
}