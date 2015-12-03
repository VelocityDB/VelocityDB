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
    public VertexType m_headVertexType;
    public VertexId m_headVertexId;
    public VertexType m_tailVertexType;
    public VertexId m_tailVertexId;
  }
}
