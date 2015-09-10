using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbBrowser.ViewModel
{
 public class ObjectViewModel : TreeViewItemViewModel
  {
    readonly IOptimizedPersistable _object;
    readonly SessionBase _session;
    readonly string objectAsString;

    public ObjectViewModel(IOptimizedPersistable obj, PageViewModel parentPage, SessionBase session)
      : base(parentPage, true)
    {
     _object = obj;
     _session = session;
     if (obj.WrappedObject != obj)
       objectAsString = obj.WrappedObject.ToString() + " " + new Oid(obj.Id);
     else
       objectAsString = obj.ToString();
    }
    public ObjectViewModel(IOptimizedPersistable obj, FieldViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      _object = obj;
      _session = session;
      if (obj.WrappedObject != obj)
        objectAsString = obj.WrappedObject.ToString() + " " + new Oid(obj.Id);
      else
        objectAsString = obj.ToString();
    }
    public string ObjectName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return objectAsString; }
    }

    public string ObjectDisplay
    {
      get 
      {
        //return _object.ToStringDetails(_schema);
        return objectAsString;
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
        if (member.Field != null && memberObj != null & (member.Field.FieldType.IsArray || member.HasId || listWithItems))
          base.Children.Add(new FieldViewModel(_object, member, this, _session));
        else
          base.Children.Add(new FieldViewModelNoExpansions(_object, member, this, _session));
    }

    protected override void LoadChildren()
    {
      _object.LoadFields();
      object o = _object.WrappedObject;
      TypeVersion baseShape = _object.Shape.BaseShape;
      while (baseShape != null)
      {
        foreach (DataMember member in baseShape.DataMemberArray)
        {
          object memberObj = member.GetMemberValue(o);
          LoadChild(member, memberObj);
        }
        baseShape = baseShape.BaseShape;
      }
      foreach (DataMember member in _object.Shape.DataMemberArray)
      {
        object memberObj = member.GetMemberValue(o);
        LoadChild(member, memberObj);
      }
    }
  }
}
