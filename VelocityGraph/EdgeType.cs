using System;
using System.Linq;
using System.Collections.Generic;
using VelocityDb;
using VelocityDb.Session;
using ElementId = System.Int32;
using EdgeId = System.Int32;
using VertexId = System.Int32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using TypeId = System.Int32;
using VelocityDb.Collection.BTree;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using VelocityDb.Collection;

namespace VelocityGraph
{
  /// <summary>
  /// All edges have a type that is identified as a EdgeType. Each EdgeType have a name, can be persisted and tracks all edges of its type. The edge type also knows about properties used by its type.
  /// </summary>
  [Serializable]
  public partial class EdgeType : OptimizedPersistable, IComparable<EdgeType>, IEqualityComparer<EdgeType>
  {
    internal Graph graph;
    EdgeType baseType;
    internal VelocityDbList<EdgeType> subType;
    string typeName;
    TypeId typeId;
    VelocityDbList<Range<EdgeId>> edgeRanges;
    internal BTreeMap<EdgeId, UnrestrictedEdge> unrestrictedEdges;
    internal BTreeMap<EdgeId, UInt64> restrictedEdges;
    internal BTreeMap<string, PropertyType> stringToPropertyType;
    bool birectional;
    VertexType headType;
    VertexType tailType;

    /// <summary>
    /// Creates a new edge type.
    /// </summary>
    /// <param name="aTypeId">The id to use for the new edge type</param>
    /// <param name="aTypeName">A type name to use</param>
    /// <param name="tailType">Restrict tail vertex to a certain vertex type</param>
    /// <param name="headType">Restrict head vertex to a certain vertex type</param>
    /// <param name="birectional">Is this edge type bidirectional (going both ways)</param>
    /// <param name="baseType">A base type can be specified</param>
    /// <param name="graph">The owning graph</param>
    public EdgeType(TypeId aTypeId, string aTypeName, VertexType tailType, VertexType headType, bool birectional, EdgeType baseType, Graph graph)
    {
      this.graph = graph;
      this.baseType = baseType;
      subType = new VelocityDbList<EdgeType>();
      if (baseType != null)
        baseType.subType.Add(this);
      this.birectional = birectional;
      typeId = aTypeId;
      typeName = aTypeName;
      this.tailType = tailType;
      this.headType = headType;
      if (Unrestricted)
        unrestrictedEdges = new BTreeMap<EdgeId, UnrestrictedEdge>(null, graph.Session);
      else
        restrictedEdges = new BTreeMap<EdgeId, ulong>(null, graph.Session);
      stringToPropertyType = new BTreeMap<string, PropertyType>(null, graph.Session);
      edgeRanges = new VelocityDbList<Range<EdgeId>>();
    }

    /// <summary>
    /// Compares two EdgeType objects by id
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>a negative number if less, 0 if equal or else a positive number</returns>
    public int CompareTo(EdgeType obj)
    {
      return typeId.CompareTo(obj.typeId);
    }

    /// <summary>
    /// Compares two edge types by id
    /// </summary>
    /// <param name="aId">edge type 1</param>
    /// <param name="bId">edge type 2</param>
    /// <returns>0 if edge types are equal, -1 if aId is less than bId; otherwise 1</returns>
    public static int Compare(EdgeType aId, EdgeType bId)
    {
      return aId.typeId.CompareTo(bId.typeId);
    }

    /// <summary>
    /// Compares two edge types by id
    /// </summary>
    /// <param name="x">edge type 1</param>
    /// <param name="y">edge type 2</param>
    /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(EdgeType x, EdgeType y)
    {
      return Compare(x, y) == 0;
    }

    /// <summary>
    /// Is Directed if edge type is not birectionalal
    /// </summary>
    public bool Directed
    {
      get
      {
        return birectional == false;
      }
    }
    /// <summary>
    /// Gets an edge given an edge id. Throws if no such edge exist.
    /// </summary>
    /// <param name="edgeId">The id of the edge</param>
    /// <returns>The edge with matching id if it exists</returns>
    public Edge GetEdge(EdgeId edgeId)
    {
      if (Unrestricted)
      {
        UnrestrictedEdge headTail;
        if (unrestrictedEdges.TryGetValue(edgeId, out headTail))
        {
          VertexType vt = headTail.headVertexType;
          Vertex head = vt.GetVertex(headTail.headVertexId);
          vt = headTail.tailVertexType;
          Vertex tail = vt.GetVertex(headTail.tailVertexId);
          return new Edge(graph, this, edgeId, head, tail);
        }
      }
      else
      {
        UInt64 vertexVertex;
        if (restrictedEdges.TryGetValue(edgeId, out vertexVertex))
        {
          VertexId headId = (VertexId)(vertexVertex >> 32);
          Vertex head = headType.GetVertex(headId);
          Vertex tail = tailType.GetVertex((VertexId)vertexVertex);
          return new Edge(graph, this, edgeId, head, tail);
        }
      }

      throw new EdgeDoesNotExistException();
    }

    /// <summary>
    /// Enumerates all edges of this type
    /// </summary>
    /// <param name="polymorphic">If true, also include all edges of sub types</param>
    /// <returns>Enumeration of edges of this type</returns>
    public IEnumerable<Edge> GetEdges(bool polymorphic = false)
    {
      if (Unrestricted)
        foreach (var m in unrestrictedEdges)
        {
          VertexType vt1 = m.Value.headVertexType;
          Vertex head = vt1.GetVertex(m.Value.headVertexId);
          VertexType vt2 = m.Value.tailVertexType;
          Vertex tail = vt2.GetVertex(m.Value.tailVertexId);
          yield return GetEdge(m.Key, tail, head);
        }
      else
        foreach (var m in restrictedEdges)
        {
          VertexType vt1 = headType;
          VertexId vId = (VertexId) (m.Value >> 32);
          Vertex head = vt1.GetVertex(vId);
          VertexType vt2 = tailType;
          vId = (VertexId) m.Value;
          Vertex tail = vt2.GetVertex(vId);
          yield return GetEdge(m.Key, tail, head);
        }
      if (polymorphic)
      {
        foreach (EdgeType et in subType)
          foreach (Edge e in et.GetEdges(polymorphic))
            yield return e;
      }
    }

    /// <summary>
    /// Get a count of existing edges of this type
    /// </summary>
    /// <returns></returns>
    public long CountEdges()
    {
      long ct = 0;
      foreach (Range<EdgeId> range in edgeRanges)
        ct += range.Max - range.Min + 1;
      return ct;
    }

    /// <summary>
    /// Get an edge for a given id
    /// </summary>
    /// <param name="edgeId">Id of edge</param>
    /// <param name="tailVertex">the tail vertex of the edge</param>
    /// <param name="headVertex">the head vertex of the edge</param>
    /// <returns></returns>
    public Edge GetEdge(EdgeId edgeId, Vertex tailVertex, Vertex headVertex)
    {
      if (Unrestricted)
      {
        if (unrestrictedEdges.Contains(edgeId))
          return new Edge(graph, this, edgeId, headVertex, tailVertex);
      }
      else if (restrictedEdges.Contains(edgeId))
        return new Edge(graph, this, edgeId, headVertex, tailVertex);
      throw new EdgeDoesNotExistException();
    }

    /// <summary>
    /// Get a hash code for an edge tyepe based on type id
    /// </summary>
    /// <param name="t">edge type to get hash code for</param>
    /// <returns>Hash code of an edge type</returns>
    public int GetHashCode(EdgeType t)
    {
      return t.typeId.GetHashCode();
    }

    /// <summary>
    /// Return all the keys associated with the edge type.
    /// </summary>
    /// <returns>the set of all string keys associated with the edge type</returns>
    public IEnumerable<string> GetPropertyKeys()
    {
      foreach (var pair in stringToPropertyType)
      {
        yield return pair.Key;
      }
    }

    /// <summary>
    /// Gets the Head VertexType of the edge (might not be set)
    /// </summary>
    public VertexType HeadType
    {
      get
      {
        return headType;
      }
    }

    /// <summary>
    /// Creates a new Property. 
    /// </summary>
    /// <param name="name">Unique name for the new Property.</param>
    /// <param name="dt">Data type for the new Property.</param>
    /// <param name="kind">Property kind.</param>
    /// <returns>a Property.</returns>
    public PropertyType NewProperty(string name, DataType dt, PropertyKind kind)
    {
      PropertyType aType;
      if (stringToPropertyType.TryGetValue(name, out aType) == false)
      {
        graph.Update();
        int pos = -1;
        int i = 0;
        foreach (PropertyType pt in graph.propertyType)
          if (pt == null)
          {
            pos = i;
            break;
          }
          else
            ++i;
        if (pos < 0)
        {
          pos = graph.propertyType.Length;
          Array.Resize(ref graph.propertyType, pos + 1);
        }
        aType = graph.PropertyTypeFromDataType(false, dt, this.TypeId, pos, name, kind);
        graph.propertyType[pos] = aType;
        stringToPropertyType.AddFast(name, aType);
      }
      return aType;
    }

    /// <summary>
    /// Creates a new Property. 
    /// </summary>
    /// <param name="name">Unique name for the new Property.</param>
    /// <param name="value">Object guiding the type of the property.</param>
    /// <param name="kind">Property kind.</param>
    /// <returns>a Property.</returns>
    internal PropertyType NewProperty(string name, object value, PropertyKind kind)
    {
      PropertyType aType;
      if (stringToPropertyType.TryGetValue(name, out aType) == false)
      {
        int pos = graph.propertyType.Length;
        graph.Update();
        Array.Resize(ref graph.propertyType, pos + 1);
        switch (Type.GetTypeCode(value.GetType()))
        {
          case TypeCode.Boolean:
            aType = new PropertyTypeT<bool>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.Int32:
            aType = new PropertyTypeT<int>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.Int64:
            aType = new PropertyTypeT<long>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.Single:
            aType = new PropertyTypeT<Single>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.Double:
            aType = new PropertyTypeT<double>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.DateTime:
            aType = new PropertyTypeT<DateTime>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.String:
            aType = new PropertyTypeT<string>(false, this.TypeId, pos, name, kind, graph);
            break;
          case TypeCode.Object:
            aType = new PropertyTypeT<IComparable>(false, this.TypeId, pos, name, kind, graph);
            break;
        }
        graph.propertyType[pos] = aType;
        stringToPropertyType.AddFast(name, aType);
      }
      return aType;
    }

    EdgeId NewEdgeId(Graph g)
    {
      Range<EdgeId> range;
      EdgeId eId = 1;
      switch (edgeRanges.Count)
      {
        case 0:
          range = new Range<EdgeId>(1, 1);
          edgeRanges.Add(range);
          break;
        case 1:
          range = edgeRanges.First();

          if (range.Min == 1)
          {
            eId = range.Max + 1;
            range = new Range<EdgeId>(1, eId);
          }
          else
          {
            eId = range.Min - 1;
            range = new Range<EdgeId>(eId, range.Max);
          }
          edgeRanges[0] = range;
          break;
        default:
          {
            range = edgeRanges.First();
            if (range.Min > 1)
            {
              eId = range.Min - 1;
              range = new Range<VertexId>(eId, range.Max);
            }
            else
            {
              Range<VertexId> nextRange = edgeRanges[1];
              if (range.Max + 2 == nextRange.Min)
              {
                edgeRanges.Remove(range);
                eId = range.Max + 1;
                range = new Range<VertexId>(range.Min, nextRange.Max);
              }
              else
              {
                range = new Range<VertexId>(range.Min, range.Max + 1);
                eId = range.Max + 1;
              }
              edgeRanges.Add(range);
            }
          }
          break;
      }
      return eId;
    }

    /// <summary>
    /// Create an edge between tail and head vertex
    /// </summary>
    /// <param name="tail">selected tail vertex</param>
    /// <param name="head">selected head vertex</param>
    /// <returns>a new edge</returns>
    public Edge NewEdge(Vertex tail, Vertex head)
    {
      if (tailType != null && tail.VertexType != tailType)
        throw new InvalidTailVertexTypeException();
      if (headType != null && head.VertexType != headType)
        throw new InvalidHeadVertexTypeException();
      EdgeId eId = NewEdgeId(graph);
      if (Unrestricted)
        unrestrictedEdges.AddFast(eId, new UnrestrictedEdge { headVertexType = head.VertexType, headVertexId = head.VertexId, tailVertexType = tail.VertexType, tailVertexId = tail.VertexId });
      else
      {
        UInt64 vertexVertex = (UInt64)head.VertexId;
        vertexVertex = vertexVertex << 32;
        vertexVertex += (UInt64)tail.VertexId;
        restrictedEdges.AddFast(eId, vertexVertex);
      }
      Edge edge = new Edge(graph, this, eId, head, tail);
      if (birectional)
      {
        tail.VertexType.NewTailToHeadEdge(this, edge, tail, head, Session);
        head.VertexType.NewHeadToTailEdge(this, edge, tail, head, Session);
      }
      else
        tail.VertexType.NewTailToHeadEdge(this, edge, tail, head, Session);
      return edge;
    }

    /// <summary>
    /// Removes this <see cref="EdgeType"/> from a graph. An exception is thrown if the <see cref="EdgeType"/> is in use.
    /// </summary>
    public void Remove()
    {
      if (GetEdges().ElementAtOrDefault(0) != null)
        throw new EdgeTypeInUseException();
      if (subType.Count > 0)
        throw new EdgeTypeInUseException();
      if (baseType != null)
        baseType.subType.Remove(this);
      Unpersist(Session);
    }

    /// <summary>
    /// Removing an edge from this edge type
    /// </summary>
    /// <param name="edge">an edge to remove</param>
    public void RemoveEdge(Edge edge)
    {
      if (Unrestricted)
      {
        UnrestrictedEdge unrestrictedEdge = unrestrictedEdges[edge.EdgeId];
        unrestrictedEdge.Unpersist(graph.Session);
        unrestrictedEdges.Remove(edge.EdgeId);
      }
      else
        restrictedEdges.Remove(edge.EdgeId);
      if (birectional)
      {
        edge.Tail.VertexType.RemoveTailToHeadEdge(edge);
        edge.Head.VertexType.RemoveHeadToTailEdge(edge);
      }
      else
        edge.Tail.VertexType.RemoveTailToHeadEdge(edge);
      foreach (string key in GetPropertyKeys())
        edge.RemoveProperty(key);
      Range<EdgeId> range = new Range<EdgeId>(edge.EdgeId, edge.EdgeId);
      bool isEqual;
      int pos = edgeRanges.BinarySearch(range, out isEqual);
      if (pos >= 0)
      {
        if (pos == edgeRanges.Count || (pos > 0 && edgeRanges[pos].Min > edge.EdgeId))
          --pos;
        range = edgeRanges[pos];
        if (range.Min == edge.EdgeId)
        {
          if (range.Max == edge.EdgeId)
            edgeRanges.RemoveAt(pos);
          else
            edgeRanges[pos] = new Range<EdgeId>(range.Min + 1, range.Max);
        }
        else if (range.Max == edge.EdgeId)
          edgeRanges[pos] = new Range<EdgeId>(range.Min, range.Max + 1);
        else
        {
          edgeRanges[pos] = new Range<EdgeId>(range.Min, edge.EdgeId - 1);
          edgeRanges.Insert(pos + 1, new Range<EdgeId>(edge.EdgeId + 1, range.Max));
        }
      }
    }

    /// <summary>
    /// Not yet implemented
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="tailAttr"></param>
    /// <param name="tailV"></param>
    /// <param name="headAttr"></param>
    /// <param name="headV"></param>
    /// <param name="session"></param>
    /// <returns></returns>
   public Edge NewEdgeX(PropertyType[] propertyType, PropertyType tailAttr, object tailV, PropertyType headAttr, object headV, SessionBase session)
   {
      throw new NotImplementedException("don't yet know what it is supposed to do");
   }

    /// <summary>
    /// Gets the id of the edge type
    /// </summary>
    public TypeId TypeId
    {
      get
      {
        return typeId;
      }
    }

    /// <summary>
    /// Gets the name of the this edge type
    /// </summary>
    public string TypeName
    {
      get
      {
        return typeName;
      }
    }

    /// <summary>
    /// Gets the property type given a property type name
    /// </summary>
    /// <param name="name">a property type name</param>
    /// <returns>a looked up property type or null if no such property type exist</returns>
    public PropertyType FindProperty(string name)
    {
      PropertyType anPropertyType;
      if (stringToPropertyType.TryGetValue(name, out anPropertyType))
      {
        return anPropertyType;
      }
      return null;
    }

    /// <summary>
    /// Gets a property value given an edge id and a property type
    /// </summary>
    /// <param name="elementId">an edge id</param>
    /// <param name="property">a property type</param>
    /// <returns>a property value</returns>
    public IComparable GetPropertyValue(ElementId elementId, PropertyType property)
    {
      return property.GetPropertyValue(elementId);
    }

    /// <summary>
    /// Sets an edge property given an edge id, property type and a value
    /// </summary>
    /// <param name="elementId">an edge id</param>
    /// <param name="property">a property type</param>
    /// <param name="v">a value</param>
    public void SetPropertyValue(ElementId elementId, PropertyType property, IComparable v)
    {
      property.SetPropertyValue(elementId, typeId, v);
    }

    /// <summary>
    /// Gets the session managing this object
    /// </summary>
    public override SessionBase Session
    {
      get
      {
        return (graph != null && graph.Session != null) ? graph.Session : base.Session;
      }
    }

    /// <summary>
    /// Gets the Tail VertexType of the edge (might not be set)
    /// </summary>
    public VertexType TailType
    {
      get
      {
        return tailType;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return "EdgeType: " + typeName;
    }

    /// <inheritdoc />
    public override void Unpersist(SessionBase session, bool disableFlush = true)
    {
      if (IsPersistent == false)
        return;
      subType.Unpersist(session, disableFlush);
      if (unrestrictedEdges != null)
        unrestrictedEdges.Unpersist(session, disableFlush);
      if (restrictedEdges != null)
        restrictedEdges.Unpersist(session, disableFlush);
      stringToPropertyType.Unpersist(session, disableFlush);
      edgeRanges.Unpersist(session, disableFlush);
      base.Unpersist(session, disableFlush);
      graph.RemoveEdgeTypeRef(this);
    }

    /// <summary>
    /// Is this edge type restricted to a certain head and tail vertex type?
    /// </summary>
    public bool Unrestricted
    {
      get
      {
        return headType == null;
      }
    }
  }
}
