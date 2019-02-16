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
  public class VertexTypeViewModel : TreeViewItemViewModel
  {
    readonly VertexType _vertexType;
    readonly SessionBase m_session;

    public VertexTypeViewModel(VertexType vertexType, TreeViewItemViewModel parentPage, SessionBase session)
      : base(parentPage, true)
    {
      _vertexType = vertexType;
      m_session = session;
    }

    public string ObjectName
    {
      //get { return _object.ToStringDetails(_schema); }
      get { return _vertexType.ToString(); }
    }

    protected override void LoadChildren()
    {
      if (_vertexType != null)
      {
        m_session.LoadFields(_vertexType);
        foreach (var vertex in _vertexType.GetVertices())
        {
          base.Children.Add(new VertexViewModel(vertex, this, m_session));
        }
        foreach (var pt in _vertexType.GetPropertyTypes())
        {
          base.Children.Add(new PropertyTypeViewModel(pt, this, m_session));
        }
      }
    }
  }
}
