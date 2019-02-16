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
  public class VertexPropertyViewModel : TreeViewItemViewModel
  {
    readonly string _property;
    readonly Vertex _vertex;


    public VertexPropertyViewModel(string property, Vertex vertex, VertexViewModel parentView, SessionBase session)
      : base(parentView, true)
    {
      _property = property;
      _vertex = vertex;
    }

    public string ObjectName
    {
      get { return $"{_property}: {_vertex.GetProperty(_property)}"; }
    }
  }
}