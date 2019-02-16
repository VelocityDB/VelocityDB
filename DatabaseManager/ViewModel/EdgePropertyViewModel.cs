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
  public class EdgePropertyViewModel : TreeViewItemViewModel
  {
    readonly string _property;
    readonly Edge _edge;


    public EdgePropertyViewModel(string property, Edge edge, EdgeViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      _property = property;
      _edge = edge;
    }

    public string ObjectName
    {
      get { return $"{_property}: {_edge.GetProperty(_property)}"; }
    }
  }
}