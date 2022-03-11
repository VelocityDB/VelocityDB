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
using VelocityGraph.Frontenac.Blueprints;
using VelocityGraph.Frontenac.Blueprints.Util;
using VelocityDb.Collection;
using VelocityDb.Exceptions;
using VelocityGraph.Exceptions;

namespace VelocityGraph
{
  /// <summary>
  /// All edges have a type that is identified as a EdgeType. Each EdgeType have a name, can be persisted and tracks all edges of its type. The edge type also knows about properties used by its type.
  /// </summary>
  [Serializable]
  public partial class EdgeType : OptimizedPersistable, IComparable<EdgeType>, IEqualityComparer<EdgeType>
  {
    WeakIOptimizedPersistableReference<Graph> m_graph;
    internal EdgeType m_baseType;
    List<EdgeType> m_subTypes;
    string m_typeName;
    TypeId m_typeId;
    WeakIOptimizedPersistableReference<VelocityDbList<Range<EdgeId>>> m_edgeRanges;
    internal BTreeMap<EdgeId, UnrestrictedEdge> m_unrestrictedEdges;
    internal BTreeMap<EdgeId, UInt64> m_restrictedEdges;
    internal BTreeMap<string, PropertyType> m_stringToPropertyType;
    bool m_birectional;
    WeakIOptimizedPersistableReference<VertexType> m_headType;
    WeakIOptimizedPersistableReference<VertexType> m_tailType;

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
      m_graph = new WeakIOptimizedPersistableReference<Graph>(graph);
      m_baseType = baseType;
      m_subTypes = new List<EdgeType>();
      if (baseType != null)
      {
        baseType.Update();
        baseType.m_subTypes.Add(this);
      }
      m_birectional = birectional;
      m_typeId = aTypeId;
      m_typeName = aTypeName;
      if (tailType != null)
      {
        m_tailType = new WeakIOptimizedPersistableReference<VertexType>(tailType);
        if (headType == null)
          throw new Exception("Unsupported, tail type specified but not head type");
      }
      if (headType != null)
      {
        m_headType = new WeakIOptimizedPersistableReference<VertexType>(headType);
        if (tailType == null)
          throw new Exception("Unsupported, head type specified but not tail type");
      }
      if (Unrestricted)
        m_unrestrictedEdges = new BTreeMap<EdgeId, UnrestrictedEdge>(null, graph.GetSession());
      else
        m_restrictedEdges = new BTreeMap<EdgeId, ulong>(null, graph.GetSession());
      m_stringToPropertyType = new BTreeMap<string, PropertyType>(null, graph.GetSession());
      var edgeRanges = new VelocityDbList<Range<EdgeId>>();
      graph.GetSession().Persist(edgeRanges);
      m_edgeRanges = new WeakIOptimizedPersistableReference<VelocityDbList<Range<PropertyId>>>(edgeRanges);
      graph.GetSession().Persist(this);
    }

    /// <summary>
    /// Compares two EdgeType objects by id
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>a negative number if less, 0 if equal or else a positive number</returns>
    public int CompareTo(EdgeType obj)
    {
      return m_typeId.CompareTo(obj.m_typeId);
    }

    /// <summary>
    /// Compares two edge types by id
    /// </summary>
    /// <param name="aId">edge type 1</param>
    /// <param name="bId">edge type 2</param>
    /// <returns>0 if edge types are equal, -1 if aId is less than bId; otherwise 1</returns>
    public static int Compare(EdgeType aId, EdgeType bId)
    {
      return aId.m_typeId.CompareTo(bId.m_typeId);
    }

    VelocityDbList<Range<EdgeId>> EdgeRanges
    {
      get
      {
        return m_edgeRanges.GetTarget(false, Session);
      }
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
        return m_birectional == false;
      }
    }
    /// <summary>
    /// Gets an edge given an edge id. Throws if no such edge exist.
    /// </summary>
    /// <param name="edgeId">The id of the edge</param>
    /// <param name="polymorphic">If true and id isn't found in this EdgeType continue search into sub types</param>
    /// <param name="errorIfNotFound">Indicate what to do if <see cref="Edge"/> does not exist</param>
    /// <returns>The edge with matching id if it exists</returns>
    public Edge GetEdge(EdgeId edgeId, bool polymorphic = false, bool errorIfNotFound = true)
    {
      if (Unrestricted)
      {
        UnrestrictedEdge headTail;
        if (m_unrestrictedEdges.TryGetValue(edgeId, out headTail))
        {
          VertexType vt = headTail.m_headVertexType;
          Vertex head = vt.GetVertex(headTail.m_headVertexId);
          vt = headTail.m_tailVertexType;
          Vertex tail = vt.GetVertex(headTail.m_tailVertexId);
          return new Edge(MyGraph, this, edgeId, head, tail);
        }
      }
      else
      {
        UInt64 vertexVertex;
        if (m_restrictedEdges.TryGetValue(edgeId, out vertexVertex))
        {
          VertexId headId = (VertexId)(vertexVertex >> 32);
          Vertex head = HeadType.GetVertex(headId);
          Vertex tail = TailType.GetVertex((VertexId)vertexVertex);
          return new Edge(MyGraph, this, edgeId, head, tail);
        }
      }
      if (polymorphic)
      {
        foreach (var et in m_subTypes)
        {
          var e = et.GetEdge(edgeId, polymorphic, false);
          if (e != null)
            return e;
        }
      }
      if (errorIfNotFound)
        throw new EdgeDoesNotExistException();
      return null;
    }

    /// <summary>
    /// Enumerates all edges of this type
    /// </summary>
    /// <param name="polymorphic">If true, also include all edges of sub types</param>
    /// <returns>Enumeration of edges of this type</returns>
    public IEnumerable<Edge> GetEdges(bool polymorphic = false)
    {
      if (Unrestricted)
        foreach (var m in m_unrestrictedEdges)
        {
          VertexType vt1 = m.Value.m_headVertexType;
          Vertex head = vt1.GetVertex(m.Value.m_headVertexId);
          VertexType vt2 = m.Value.m_tailVertexType;
          Vertex tail = vt2.GetVertex(m.Value.m_tailVertexId);
          yield return GetEdge(m.Key, tail, head);
        }
      else
        foreach (var m in m_restrictedEdges)
        {
          VertexType vt1 = HeadType;
          VertexId vId = (VertexId) (m.Value >> 32);
          Vertex head = vt1.GetVertex(vId);
          VertexType vt2 = TailType;
          vId = (VertexId) m.Value;
          Vertex tail = vt2.GetVertex(vId);
          yield return GetEdge(m.Key, tail, head);
        }
      if (polymorphic)
      {
        foreach (EdgeType et in m_subTypes)
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
      foreach (Range<EdgeId> range in EdgeRanges)
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
        if (m_unrestrictedEdges.Contains(edgeId))
          return new Edge(MyGraph, this, edgeId, headVertex, tailVertex);
      }
      else if (m_restrictedEdges.Contains(edgeId))
        return new Edge(MyGraph, this, edgeId, headVertex, tailVertex);
      throw new EdgeDoesNotExistException();
    }

    /// <summary>
    /// Get a hash code for an edge tyepe based on type id
    /// </summary>
    /// <param name="t">edge type to get hash code for</param>
    /// <returns>Hash code of an edge type</returns>
    public int GetHashCode(EdgeType t)
    {
      return t.m_typeId.GetHashCode();
    }

    /// <summary>
    /// Return all the keys associated with the edge type.
    /// </summary>
    /// <returns>the set of all string keys associated with the edge type</returns>
    public IEnumerable<string> GetPropertyKeys()
    {
      foreach (var pair in m_stringToPropertyType)
        yield return pair.Key;
      if (m_baseType != null)
        foreach (var k in m_baseType.GetPropertyKeys())
          yield return k;
    }

    /// <summary>
    /// Gets the Head VertexType of the edge (might not be set)
    /// </summary>
    public VertexType HeadType
    {
      get
      {
        return m_headType !=null ? m_headType.GetTarget(false, Session) : null;
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
      if (m_stringToPropertyType.TryGetValue(name, out aType) == false)
      {
        var propertyTypes = MyGraph.PropertyTypes;
        int pos = -1;
        int i = 0;
        foreach (PropertyType pt in propertyTypes)
          if (pt == null)
          {
            pos = i;
            break;
          }
          else
            ++i;
        if (pos < 0)
          pos = propertyTypes.Count;
        aType = MyGraph.PropertyTypeFromDataType(false, dt, this.TypeId, pos, name, kind);
        Session.Persist(aType);
        propertyTypes[pos] = aType;
        m_stringToPropertyType.AddFast(name, aType);
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
      if (m_stringToPropertyType.TryGetValue(name, out aType) == false)
      {
        int pos = MyGraph.PropertyTypes.Count;
        switch (Type.GetTypeCode(value.GetType()))
        {
          case TypeCode.Boolean:
            aType = new PropertyTypeT<bool>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.Int32:
            aType = new PropertyTypeT<int>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.Int64:
            aType = new PropertyTypeT<long>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.Single:
            aType = new PropertyTypeT<Single>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.Double:
            aType = new PropertyTypeT<double>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.DateTime:
            aType = new PropertyTypeT<DateTime>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.String:
            aType = new PropertyTypeNoDuplicateValues<string>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
          case TypeCode.Object:
            aType = new PropertyTypeT<IComparable>(false, this.TypeId, pos, name, kind, MyGraph);
            break;
        }
        MyGraph.PropertyTypes[pos] = aType;
        m_stringToPropertyType.AddFast(name, aType);
      }
      return aType;
    }

    EdgeId NewEdgeId(Graph g)
    {
      Range<EdgeId> range;
      EdgeId eId = 1;
      switch (EdgeRanges.Count)
      {
        case 0:
          range = new Range<EdgeId>(1, 1);
          EdgeRanges.Add(range);
          break;
        case 1:
          range = EdgeRanges.First();

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
          EdgeRanges[0] = range;
          break;
        default:
          {
            range = EdgeRanges.First();
            if (range.Min > 1)
            {
              eId = range.Min - 1;
              range = new Range<EdgeId>(eId, range.Max);
              EdgeRanges[0] = range;
            }
            else
            {
              Range<VertexId> nextRange = EdgeRanges[1];
              if (range.Max + 2 == nextRange.Min)
              {
                EdgeRanges.RemoveAt(1);
                eId = range.Max + 1;
                range = new Range<EdgeId>(range.Min, nextRange.Max);
                EdgeRanges[0] = range;
              }
              else
              {
                eId = range.Max + 1;
                range = new Range<EdgeId>(range.Min, eId);
                EdgeRanges[0] = range;
              }             
            }
          }
          break;
      }
      return eId;
    }

    /// <summary>
    /// <see cref="Graph"/> for which this <see cref="EdgeType"/> belongs to
    /// </summary>
    public Graph MyGraph
    {
      get
      {
        return m_graph.GetTarget(false, Session);
      }
    }

    /// <summary>
    /// Create an edge between tail and head vertex
    /// </summary>
    /// <param name="tail">selected tail vertex</param>
    /// <param name="head">selected head vertex</param>
    /// <returns>a new edge</returns>
    public Edge NewEdge(Vertex tail, Vertex head)
    {
      if (m_tailType != null && tail.VertexType != TailType)
        throw new InvalidTailVertexTypeException();
      if (m_headType != null && head.VertexType != HeadType)
        throw new InvalidHeadVertexTypeException();
      EdgeId eId = NewEdgeId(MyGraph);
      if (Unrestricted)
        m_unrestrictedEdges.AddFast(eId, new UnrestrictedEdge { m_headVertexType = head.VertexType, m_headVertexId = head.VertexId, m_tailVertexType = tail.VertexType, m_tailVertexId = tail.VertexId });
      else
      {
        UInt64 vertexVertex = (UInt64)head.VertexId;
        vertexVertex = vertexVertex << 32;
        vertexVertex += (UInt64)tail.VertexId;
        m_restrictedEdges.AddFast(eId, vertexVertex);
      }
      Edge edge = new Edge(MyGraph, this, eId, head, tail);
      if (m_birectional)
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
      if (m_subTypes.Count > 0)
        throw new EdgeTypeInUseException();
      if (m_baseType != null)
      {
        m_baseType.Update();
        m_baseType.m_subTypes.Remove(this);
      }
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
        UnrestrictedEdge unrestrictedEdge = m_unrestrictedEdges[edge.EdgeId];
        unrestrictedEdge.Unpersist(Session);
        m_unrestrictedEdges.Remove(edge.EdgeId);
      }
      else
        m_restrictedEdges.Remove(edge.EdgeId);
      if (m_birectional)
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
      var edgeRanges = EdgeRanges;
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
    public Edge NewEdgeX(WeakReferenceList<PropertyType> propertyType, PropertyType tailAttr, object tailV, PropertyType headAttr, object headV, SessionBase session)
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
        return m_typeId;
      }
    }

    /// <summary>
    /// Gets the name of the this edge type
    /// </summary>
    public string TypeName
    {
      get
      {
        return m_typeName;
      }
    }

    /// <summary>
    /// Gets the property type given a property type name
    /// </summary>
    /// <param name="name">a property type name</param>
    /// <returns>a looked up property type or null if no such property type exist</returns>
    public PropertyType FindProperty(string name, bool doBaseType = true)
    {
      PropertyType anPropertyType;
      if (m_stringToPropertyType.TryGetValue(name, out anPropertyType))
        return anPropertyType;
      if (doBaseType && m_baseType != null)
        return m_baseType.FindProperty(name);
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
    /// Return all the property types associated with edge type.
    /// </summary>
    /// <returns>the set of property types associated with the edge type</returns>
    public IEnumerable<PropertyType> GetPropertyTypes()
    {
      foreach (var pair in m_stringToPropertyType)
        yield return pair.Value;
      if (m_baseType != null)
        foreach (var v in m_baseType.GetPropertyTypes())
          yield return v;
    }

    /// <summary>
    /// Sets an edge property given an edge id, property type and a value
    /// </summary>
    /// <param name="elementId">an edge id</param>
    /// <param name="property">a property type</param>
    /// <param name="v">a value</param>
    public void SetPropertyValue(EdgeType et, ElementId elementId, PropertyType property, IComparable v)
    {
      property.SetPropertyValue(et, elementId, m_typeId, v);
    }
    public void SetPropertyValue(VertexType vt, ElementId elementId, PropertyType property, IComparable v) { }

    /// <summary>
    /// Sub types of this <see cref="EdgeType"/> 
    /// </summary>
    public List<EdgeType> SubTypes
    {
      get
      {
        return m_subTypes;
      }
    }
    /// <summary>
    /// Gets the Tail VertexType of the edge (might not be set)
    /// </summary>
    public VertexType TailType
    {
      get
      {
        return m_tailType != null ? m_tailType.GetTarget(false, Session) : null;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return "EdgeType: " + m_typeName;
    }

    /// <inheritdoc />
    public override void Unpersist(SessionBase session)
    {
      if (IsPersistent == false)
        return;
      if (m_unrestrictedEdges != null)
        m_unrestrictedEdges.Unpersist(session);
      if (m_restrictedEdges != null)
        m_restrictedEdges.Unpersist(session);
      m_stringToPropertyType.Unpersist(session);
      EdgeRanges.Unpersist(session);
      base.Unpersist(session);
      MyGraph.RemoveEdgeTypeRef(this);
    }

    /// <summary>
    /// Is this edge type restricted to a certain head and tail vertex type?
    /// </summary>
    public bool Unrestricted
    {
      get
      {
        return m_headType == null;
      }
    }
  }
}
