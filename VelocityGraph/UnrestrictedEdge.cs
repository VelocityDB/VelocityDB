using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElementId = System.Int32;
using EdgeId = System.Int32;
using VertexId = System.Int32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using TypeId = System.Int32;
using VelocityDb;

namespace VelocityGraph
{
  internal class UnrestrictedEdge : OptimizedPersistable
  {
    public VertexType headVertexType;
    public VertexId headVertexId;
    public VertexType tailVertexType;
    public VertexId tailVertexId;
  }
}
