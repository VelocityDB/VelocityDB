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
using VelocityGraph;
using Frontenac.Blueprints;

namespace DatabaseManager
{
  public class VertexViewModel : TreeViewItemViewModel
  {
    readonly Vertex _vertex;
    readonly SessionBase m_session;

    public VertexViewModel(Vertex vertex, TreeViewItemViewModel parentObject, SessionBase session)
      : base(parentObject, true)
    {
      m_session = session;
      _vertex = vertex;
    }

    public string VertexName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return _vertex.ToString(); }
    }

    protected override void LoadChildren()
    {
      foreach (var property in _vertex.GetPropertyKeys())
      {
        base.Children.Add(new VertexPropertyViewModel(property, _vertex,  this, m_session));
      }
      foreach (var edge in _vertex.GetEdges(Direction.Both))
      {
        base.Children.Add(new EdgeViewModel((Edge) edge, this, m_session));
      }
    }
  }
}