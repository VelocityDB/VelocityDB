using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDb.Collection.BTree;

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
      m_fieldAsString = OptimizedPersistable.ToStringDetails(member, parentObj.WrappedObject, parentObj, parentObj.Page, true);
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

    protected override void LoadChildren()
    {
      IOptimizedPersistable parentObj = (IOptimizedPersistable) m_session.Open(m_parentId);
      if (parentObj != null)
      {
        object memberObj = m_member.GetMemberValue(parentObj.WrappedObject);
        Array a = memberObj as Array;
        IOptimizedPersistable p = memberObj as IOptimizedPersistable;
        if (a != null)
          base.Children.Add(new ArrayViewModel(a, this, m_isEncodedOidArray, parentObj.Page, m_session));
        else if (p != null)
          base.Children.Add(new ObjectViewModel(p, this, m_session));
        else
          base.Children.Add(new ListViewModel(memberObj as IList, this, parentObj.Page));
      }
    }
  }
}
