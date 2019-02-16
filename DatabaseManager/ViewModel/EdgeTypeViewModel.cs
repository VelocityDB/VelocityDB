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
  public class EdgeTypeViewModel : TreeViewItemViewModel
  {
    readonly EdgeType _edgeType;
    readonly SessionBase m_session;

    public EdgeTypeViewModel(EdgeType edgeType, TreeViewItemViewModel parentPage, SessionBase session)
      : base(parentPage, true)
    {
      _edgeType = edgeType;
      m_session = session;
    }

    public string ObjectName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return _edgeType.ToString(); }
    }

    protected override void LoadChildren()
    {
      if (_edgeType != null)
      {
        m_session.LoadFields(_edgeType);
        foreach (var edge in _edgeType.GetEdges())
        {
          base.Children.Add(new EdgeViewModel(edge, this, m_session));
        }
        foreach (var et in _edgeType.GetPropertyTypes())
        {
          base.Children.Add(new PropertyTypeViewModel(et, this, m_session));
        }
      }
    }
  }
}
