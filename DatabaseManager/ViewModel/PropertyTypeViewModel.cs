using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityDBExtensions;
using VelocityGraph;

namespace DatabaseManager
{
  public class PropertyTypeViewModel : TreeViewItemViewModel
  {
    readonly PropertyType _propertyType;

    public PropertyTypeViewModel(PropertyType propertyType, TreeViewItemViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      _propertyType = propertyType;
    }

    public string ObjectName
    {
      get
      {
        return $" Property type Id: {_propertyType.PropertyId} Name: {_propertyType.Name} Value Type: {_propertyType.ValueType}";
      }
    }
  }
}