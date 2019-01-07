using System;
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
    string m_objectAsString;

    public ObjectViewModel(IOptimizedPersistable obj, TreeViewItemViewModel parentPage, SessionBase session)
      : base(parentPage, true)
    {
      m_objectId = obj.Id;
      m_session = session;
      SetName(obj);
    }

    public ObjectViewModel(IOptimizedPersistable obj, FieldViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      m_objectId = obj.Id;
      m_session = session;
      if (obj.GetWrappedObject() != obj)
        m_objectAsString = obj.GetWrappedObject().ToString() + " " + new Oid(obj.Id);
      else
        m_objectAsString = obj.ToString();
    }

    public ObjectViewModel(object obj, FieldViewModel parentView, int arrayIndex, bool encodedOid, SessionBase session)
      : base(parentView, true)
    {
      m_session = session;
      if (encodedOid)
      {
        if (obj.GetType() == typeof(UInt64))
        {
          m_objectId = (UInt64)obj;
          m_objectAsString = $"[{arrayIndex}] {new Oid(m_objectId)}";
        }
        else
        {
          Oid oid = new Oid(parentView.ParentId);
          oid = new Oid(oid.Database, (UInt32)obj);
          m_objectId = oid.Id;
          m_objectAsString = $"[{arrayIndex}] {new OidShort(oid.IdShort)}";
        }
      }
      else
      {
        IOptimizedPersistable pObj = obj as IOptimizedPersistable;
        if (pObj == null & obj != null)
          session.GlobalObjWrapperGet(obj, out pObj);
        if (pObj != null)
          m_objectId = pObj.Id;
        m_session = session;
        if (pObj != null && pObj.GetWrappedObject() != obj)
          m_objectAsString = $"[{arrayIndex}] {pObj.GetWrappedObject()} {new Oid(pObj.Id)}";
        else if (obj != null)
          m_objectAsString = $"[{arrayIndex}] {obj}";
        else
          m_objectAsString = $"[{arrayIndex}] null";
      }
    }

    void SetName(IOptimizedPersistable obj)
    {
      try
      {
        if (obj.GetWrappedObject() != obj)
          m_objectAsString = obj.GetWrappedObject().ToString() + " " + new Oid(obj.Id);
        else
          m_objectAsString = obj.ToString();
      }
      catch (Exception)
      { // in case fields used in ToString() are not loaded
          m_objectAsString = obj.GetWrappedObject().GetType().ToGenericTypeString() + " " + Oid.AsString(obj.Id);
      }
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
      IOptimizedPersistable pObj = (IOptimizedPersistable)m_session.Open(m_objectId, false, null, false, 0, Int32.MaxValue);
      if (pObj != null)
      {
        if (member.Field != null && memberObj != null & (member.Field.FieldType.IsArray || member.HasId || listWithItems || member.WeakIOptimizedPersistableReference))
          base.Children.Add(new FieldViewModel(pObj, member, this, m_session));
        else
          base.Children.Add(new FieldViewModelNoExpansions(pObj, member, this, m_session));
      }
    }

    protected override void LoadChildren()
    {
      IOptimizedPersistable pObj = (IOptimizedPersistable)m_session.Open(m_objectId, false, null, false, 0, Int32.MaxValue);     
      if (pObj != null)
      {
        SetName(pObj);
        m_session.LoadFields(pObj);
        object o = pObj.GetWrappedObject();
        TypeVersion baseShape = pObj.GetTypeVersion().BaseShape;
        while (baseShape != null)
        {
          foreach (DataMember member in baseShape.DataMemberArray)
          {
            object memberObj = member.GetMemberValue(o);
            LoadChild(member, memberObj);
          }
          baseShape = baseShape.BaseShape;
        }
        foreach (DataMember member in pObj.GetTypeVersion().DataMemberArray)
        {
          object memberObj = member.GetMemberValue(o);
          LoadChild(member, memberObj);
        }
      }
    }
  }
}
