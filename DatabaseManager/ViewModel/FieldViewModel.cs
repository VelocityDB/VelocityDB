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
    readonly string fieldAsString;
    readonly object memberObj;
    readonly Page page;
    readonly SessionBase _session;
    readonly bool isEncodedOidArray;
    public FieldViewModel(IOptimizedPersistable parentObj, DataMember member, ObjectViewModel parentObject, SessionBase session)
      : base(parentObject, true)
    {
      _session = session;
      page = parentObj.Page;
      memberObj = member.GetMemberValue(parentObj.WrappedObject);
      isEncodedOidArray = (parentObj as BTreeNode) != null && memberObj != null && (memberObj as Array) != null && (member.Field.Name == "keysArray" || member.Field.Name == "valuesArray");
      fieldAsString = OptimizedPersistable.ToStringDetails(member, parentObj.WrappedObject, parentObj, parentObj.Page, true);
    }

    public string FieldName
    {
      get { return fieldAsString; }
    }

    public string FieldDisplay
    {
      get
      {
        return fieldAsString;
      }
    }

    protected override void LoadChildren()
    {
      Array a = memberObj as Array;
      IOptimizedPersistable p = memberObj as IOptimizedPersistable;
      if (a != null)
        base.Children.Add(new ArrayViewModel(a, this, isEncodedOidArray, page, _session));
      else if (p != null)
        base.Children.Add(new ObjectViewModel(p, this, _session));
      else
        base.Children.Add(new ListViewModel(memberObj as IList, this, page));
    }
  }
}
