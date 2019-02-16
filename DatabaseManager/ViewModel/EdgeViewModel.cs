using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using VelocityGraph;

namespace DatabaseManager
{
  public class EdgeViewModel : TreeViewItemViewModel
  {
    readonly SessionBase m_session;
    Edge _edge;

    public EdgeViewModel(Edge edge, TreeViewItemViewModel parentPage, SessionBase session)
      : base(parentPage, true)
    {
      _edge = edge;
      m_session = session;
    }

    public string EdgeName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return _edge.ToString(); }
    }

    protected override void LoadChildren()
    {
      foreach (var property in _edge.GetPropertyKeys())
      {
        base.Children.Add(new EdgePropertyViewModel(property, _edge, this, m_session));
      }
      base.Children.Add(new VertexViewModel(_edge.Head, this, m_session));
    }
  }
}
