using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityGraph;

namespace DatabaseManager
{
  public class VelocityGraphViewModel : TreeViewItemViewModel
  {
    Graph _graph;

    public VelocityGraphViewModel(Graph graph)
      : base(null, true)
    {
      _graph = graph;
    }

    public Graph Graph
    {
      get
      {
        return _graph;
      }
    }

    public string GraphName
    {
      get
      {
        return _graph.ToString();
      }
    }

    protected override void LoadChildren()
    {
      //base.Children.Add(new ObjectViewModel(_graph, this, _graph.GetSession()));
      if (!_graph.GetSession().InTransaction)
        _graph.GetSession().BeginRead();
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        foreach (var vt in _graph.FindVertexTypes())
          base.Children.Add(new VertexTypeViewModel(vt, this, _graph.GetSession()));
        foreach (var et in _graph.FindEdgeTypes())
          base.Children.Add(new EdgeTypeViewModel(et, this, _graph.GetSession()));
      }
    }
  }
}
