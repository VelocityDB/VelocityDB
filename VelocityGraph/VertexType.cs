using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VertexId = System.Int32;
using EdgeId = System.Int32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using TypeId = System.Int32;
using EdgeIdVertexId = System.UInt64;
using Frontenac.Blueprints;
using VelocityDb.Collection;

namespace VelocityGraph
{
  /// <summary>
  /// All vertices have a type that is identified as a VertexType. Each VertexType have a name, can be persisted and tracks all vertices of its type. The vertex type also knows about properties used by its type.
  /// </summary>
  [Serializable]
  public partial class VertexType : OptimizedPersistable
  {
    internal Graph graph;
    VertexType baseType;
    internal VelocityDbList<VertexType> subType;
    string typeName;
    TypeId typeId;
    VelocityDbList<Range<VertexId>> vertecis;
    internal BTreeMap<string, PropertyType> stringToPropertyType;
    BTreeSet<EdgeType> edgeTypes;
    BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>> tailToHeadEdges;
    BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>> headToTailEdges;

    internal VertexType(TypeId aTypeId, string aTypeName, VertexType baseType, Graph graph)
    {
      this.graph = graph;
      this.baseType = baseType;
      subType = new VelocityDbList<VertexType>();
      if (baseType != null)
        baseType.subType.Add(this);
      typeId = (TypeId)aTypeId;
      typeName = aTypeName;
      vertecis = new VelocityDbList<Range<VertexId>>();
      stringToPropertyType = new BTreeMap<string, PropertyType>(null, graph.Session);
      edgeTypes = new BTreeSet<EdgeType>(null, graph.Session);
      tailToHeadEdges = new BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>(null, graph.Session);
      headToTailEdges = new BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>(null, graph.Session);
    }

    /// <summary>
    /// All edge types connected with this vertex type.
    /// </summary>
    public BTreeSet<EdgeType> EdgeTypes
    {
      get
      {
        return edgeTypes;
      }
    }

    /// <summary>
    /// Gets a combined <see cref="Edge"/> id and <see cref="Vertex"/> id
    /// </summary>
    /// <param name="edge">Get edge id from this edge</param>
    /// <param name="vertexId">A <see cref="Vertex"/> id</param>
    /// <returns>the combined ids</returns>
    protected EdgeIdVertexId edgeVertexId(Edge edge, VertexId vertexId)
    {
      EdgeIdVertexId id = (EdgeIdVertexId)edge.EdgeId;
      id <<= 32;
      return id + (EdgeIdVertexId)vertexId;
    }

    /// <summary>
    /// Instantiates a Vertex if it exist
    /// </summary>
    /// <param name="vertexId">id of Vertex we are looking for</param>
    /// <param name="polymorphic">If true and id isn't found in this VertexType continue search into sub types</param>
    /// <param name="errorIfNotFound">Indicate what to do if Vertex does not exist</param>
    /// <returns>A Vertex or null</returns>
    public Vertex GetVertex(VertexId vertexId, bool polymorphic = false, bool errorIfNotFound = true)
    {
      if (vertecis.Count > 0)
      {
        Range<VertexId> range = new Range<VertexId>(vertexId, vertexId);
        bool isEqual;
        int pos = vertecis.BinarySearch(range, out isEqual);
        if (pos >= 0)
        {
          if (pos == vertecis.Count)
            --pos;
          range = vertecis[pos];
          if (range.Contains(vertexId))
            return new Vertex(graph, this, vertexId);
          if (pos > 0)
          {
            range = vertecis[--pos];
            if (range.Contains(vertexId))
              return new Vertex(graph, this, vertexId);
          }
        }
      }
      if (polymorphic)
      {
        foreach (VertexType vt in subType)
        {
          Vertex v = vt.GetVertex(vertexId, polymorphic, false);
          if (v != null)
            return v;
        }
      }
      if (errorIfNotFound)
        throw new VertexDoesNotExistException();
      return null;
    }

    internal void NewTailToHeadEdge(EdgeType edgeType, Edge edge, Vertex tail, Vertex head, SessionBase session)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap;
      BTreeSet<EdgeIdVertexId> set;
      //lock (tailToHeadEdges)
      {
        if (!tailToHeadEdges.TryGetValue(edgeType, out map))
        {
          map = new BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>(null, session);
          innerMap = new BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>(null, session);
          set = new BTreeSet<EdgeIdVertexId>(null, session);
          //if (IsPersistent)
          //{
          //  session.Persist(set, 1000);
          //  session.Persist(innerMap, 1000);
          //  session.Persist(map, 1000);
          //}
          innerMap.AddFast(tail.VertexId, set);
          map.AddFast(head.VertexType, innerMap);
          tailToHeadEdges.AddFast(edgeType, map);
          edgeTypes.AddFast(edgeType);
        }
        else if (!map.TryGetValue(head.VertexType, out innerMap))
        {
          innerMap = new BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>(null, session);
          set = new BTreeSet<EdgeIdVertexId>(null, session);
          //if (IsPersistent)
          //{
          //  session.Persist(set, 1000);
          //  session.Persist(innerMap, 1000);
          //}
          innerMap.AddFast(tail.VertexId, set);
          map.AddFast(head.VertexType, innerMap);
        }
        else if (!innerMap.TryGetValue(tail.VertexId, out set))
        {
          set = new BTreeSet<EdgeIdVertexId>(null, session);
          //if (IsPersistent)
          //{
          //  session.Persist(set, 1000);
          //}
          innerMap.AddFast(tail.VertexId, set);
        }
        set.AddFast(edgeVertexId(edge, head.VertexId));
      }
    }

    /// <summary>
    /// Removes this <see cref="VertexType"/> from a graph. An exception is thrown if the <see cref="VertexType"/> is in use.
    /// </summary>
    public void Remove()
    {
      if (GetVertices().ElementAtOrDefault(0) != null)
        throw new VertexTypeInUseException();
      if (subType.Count > 0)
        throw new VertexTypeInUseException();
      if (baseType != null)
        baseType.subType.Remove(this);
      Unpersist(Session);
    }

    internal void RemoveTailToHeadEdge(Edge edge)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap;
      BTreeSet<EdgeIdVertexId> set;
      if (tailToHeadEdges.TryGetValue(edge.EdgeType, out map))
        if (map.TryGetValue(edge.Head.VertexType, out innerMap))
          if (innerMap.TryGetValue(edge.Tail.VertexId, out set))
            set.Remove(edgeVertexId(edge, edge.Head.VertexId));
    }

    internal void RemoveHeadToTailEdge(Edge edge)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      BTreeMap<EdgeId, BTreeSet<EdgeIdVertexId>> innerMap;
      BTreeSet<EdgeIdVertexId> set;
      if (headToTailEdges.TryGetValue(edge.EdgeType, out map))
        if (map.TryGetValue(edge.Tail.VertexType, out innerMap))
          if (innerMap.TryGetValue(edge.Head.VertexId, out set))
            set.Remove(edgeVertexId(edge, edge.Tail.VertexId));
    }

    internal void NewHeadToTailEdge(EdgeType edgeType, Edge edge, Vertex tail, Vertex head, SessionBase session)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      BTreeMap<EdgeId, BTreeSet<EdgeIdVertexId>> innerMap;
      BTreeSet<EdgeIdVertexId> set;
      //lock (headToTailEdges)
      {
        if (!headToTailEdges.TryGetValue(edgeType, out map))
        {
          map = new BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>(null, session);
          innerMap = new BTreeMap<EdgeId, BTreeSet<EdgeIdVertexId>>(null, session);
          set = new BTreeSet<EdgeIdVertexId>(null, session);
          innerMap.AddFast(head.VertexId, set);
          map.AddFast(tail.VertexType, innerMap);
          headToTailEdges.AddFast(edgeType, map);
          edgeTypes.AddFast(edgeType);
        }
        else if (!map.TryGetValue(tail.VertexType, out innerMap))
        {
          innerMap = new BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>(null, session);
          set = new BTreeSet<EdgeIdVertexId>(null, session);
          innerMap.AddFast(head.VertexId, set);
          map.AddFast(tail.VertexType, innerMap);
        }
        else if (!innerMap.TryGetValue(head.VertexId, out set))
        {
          set = new BTreeSet<EdgeIdVertexId>(null, session);
          innerMap.AddFast(head.VertexId, set);
        }
        set.AddFast(edgeVertexId(edge, tail.VertexId));
      }
    }

    /// <summary>
    /// Creates a new Vertex
    /// </summary>
    /// <param name="vId">Optionally provide the Vertex id to use.</param>
    /// <returns>The newly created <see cref="Vertex"/></returns>
    public Vertex NewVertex(VertexId vId = 0)
    {
      vId = graph.AllocateVertexId(vId);
      vId = graph.AllocateVertexId(vId, vertecis);
      if (graph.vertexIdToVertexType != null)
        graph.vertexIdToVertexType.AddFast(vId, typeId);
      return new Vertex(graph, this, vId);
    }

    internal void AddToResult(Dictionary<Vertex, HashSet<Edge>> result, Vertex key, Edge value)
    {
      HashSet<Edge> hashSet;
      if (result.TryGetValue(key, out hashSet))
        hashSet.Add(value);
      else
      {
        hashSet = new HashSet<Edge>();
        hashSet.Add(value);
        result.Add(key, hashSet);
      }
    }

    /// <summary>
    /// Selects all neighbor Vertices from or to this vertex and for the given edge type.
    /// </summary>
    /// <param name="vertex1">the start Vertex</param>
    /// <param name="etype">the type of edges to follow</param>
    /// <param name="dir">Direction.</param>
    /// <returns>Dictionary of vertex key with edge path(s) to vertex</returns>
    internal Dictionary<Vertex, HashSet<Edge>> Traverse(Vertex vertex1, EdgeType etype, Direction dir)
    {
      Dictionary<Vertex, HashSet<Edge>> result = new Dictionary<Vertex, HashSet<Edge>>(10);
      if (etype.Directed)
      {
        if (etype.Unrestricted)
        {
          foreach (var pair in etype.unrestrictedEdges)
          {
            UnrestrictedEdge edgeStruct = pair.Value;
            bool headSameVertex = vertex1.VertexType == edgeStruct.headVertexType && vertex1.VertexId == edgeStruct.headVertexId;
            bool tailSameVertex = vertex1.VertexType == edgeStruct.tailVertexType && vertex1.VertexId == edgeStruct.tailVertexId;
            Vertex other;
            if (headSameVertex)
            {
              VertexType vt = edgeStruct.tailVertexType;
              other = vt.GetVertex(edgeStruct.tailVertexId);
            }
            else
            {
              if (tailSameVertex == false)
                continue;
              VertexType vt = edgeStruct.headVertexType;
              other = vt.GetVertex(edgeStruct.headVertexId);
            }
#if EdgeDebug
          Edge edge = etype.GetEdge(graph, pair.Key, other, vertex1);
#else
            Edge edge = new Edge(graph, etype, pair.Key, vertex1, other);
#endif
            AddToResult(result, other, edge);
          }
        }
        else
          foreach (var pair in etype.restrictedEdges)
          {
            UInt64 vertexVertex = pair.Value;
            VertexId headVertexId = (VertexId) (vertexVertex >> 32);
            VertexId tailVertexId = (Int32) (UInt32)vertexVertex;
            bool headSameVertex = vertex1.VertexType == etype.HeadType && vertex1.VertexId == headVertexId;
            bool tailSameVertex = vertex1.VertexType == etype.TailType && vertex1.VertexId == tailVertexId;
            Vertex other;
            if (headSameVertex)
            {
              VertexType vt = etype.TailType;
              other = vt.GetVertex(tailVertexId);
            }
            else
            {
              if (tailSameVertex == false)
                continue;
              VertexType vt = etype.HeadType;
              other = vt.GetVertex(headVertexId);
            }
#if EdgeDebug
          Edge edge = etype.GetEdge(g, pair.Key, other, vertex1);
#else
            Edge edge = new Edge(graph, etype, pair.Key, vertex1, other);
#endif
            AddToResult(result, other, edge);
          }
      }
      else
      {
        BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
        BTreeSet<EdgeIdVertexId> set;
        switch (dir)
        {
          case Direction.Out:
            if (tailToHeadEdges.TryGetValue(etype, out map))
            {
              foreach (KeyValuePair<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> pair in map)
              {
                BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap = pair.Value;
                if (innerMap.TryGetValue(vertex1.VertexId, out set))
                {
                  foreach (EdgeIdVertexId id in set)
                  {
                    Vertex vertex2 = new Vertex(graph, pair.Key, (VertexId)id);
                    EdgeId eId = (EdgeId)(id >> 32);
                    Edge edge = etype.GetEdge(eId, vertex1, vertex2);
                    AddToResult(result, vertex2, edge);
                  }
                }
              }
            }
            break;
          case Direction.In:
            if (headToTailEdges.TryGetValue(etype, out map))
            {
              foreach (KeyValuePair<VertexType, BTreeMap<EdgeId, BTreeSet<EdgeIdVertexId>>> pair in map)
              {
                BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap = pair.Value;
                if (innerMap.TryGetValue(vertex1.VertexId, out set))
                {
                  foreach (EdgeIdVertexId id in set)
                  {
                    Vertex vertex2 = new Vertex(graph, pair.Key, (VertexId)id);
                    EdgeId eId = (EdgeId)(id >> 32);
#if EdgeDebug
                    Edge edge = etype.GetEdge(g, eId, vertex1, vertex2);
#else
                    Edge edge = new Edge(graph, etype, eId, vertex1, vertex2);
#endif
                    AddToResult(result, vertex2, edge);
                  }
                }
              }
            }
            break;
          case Direction.Both:
            if (tailToHeadEdges.TryGetValue(etype, out map))
            {
              foreach (KeyValuePair<VertexType, BTreeMap<EdgeId, BTreeSet<EdgeIdVertexId>>> pair in map)
              {
                BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap = pair.Value;
                if (innerMap.TryGetValue(vertex1.VertexId, out set))
                {
                  foreach (EdgeIdVertexId id in set)
                  {
                    Vertex vertex2 = new Vertex(graph, pair.Key, (VertexId)id);
                    EdgeId eId = (EdgeId)(id >> 32);
#if EdgeDebug
                    Edge edge = etype.GetEdge(g, eId, vertex1, vertex2);
#else
                    Edge edge = new Edge(graph, etype, eId, vertex1, vertex2);
#endif
                    AddToResult(result, vertex2, edge);
                  }
                }
              }
            }
            if (headToTailEdges.TryGetValue(etype, out map))
            {
              foreach (KeyValuePair<VertexType, BTreeMap<EdgeId, BTreeSet<EdgeIdVertexId>>> pair in map)
              {
                BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap = pair.Value;
                if (innerMap.TryGetValue(vertex1.VertexId, out set))
                {
                  foreach (EdgeIdVertexId id in set)
                  {
                    Vertex vertex2 = new Vertex(graph, pair.Key, (VertexId)id);
                    EdgeId eId = (EdgeId)(id >> 32);
#if EdgeDebug
                    Edge edge = etype.GetEdge(g, eId, vertex1, vertex2);
#else
                    Edge edge = new Edge(graph, etype, eId, vertex1, vertex2);
#endif
                    AddToResult(result, vertex2, edge);
                  }
                }
              }
            }
            break;
        }
      }
      return result;
    }

    /// <summary>
    /// Creates a new Property. 
    /// </summary>
    /// <param name="name">Unique name for the new Property.</param>
    /// <param name="dt">Data type for the new Property.</param>
    /// <param name="kind">Property kind.</param>
    /// <returns>Unique Property identifier.</returns>
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
        aType = graph.PropertyTypeFromDataType(true, dt, typeId, pos, name, kind);
        graph.propertyType[pos] = aType;
        stringToPropertyType.AddFast(name, aType);
      }
      return aType;
    }

    /// <summary>
    /// Get the unique id of the vertex type.
    /// </summary>
    public TypeId TypeId
    {
      get
      {
        return typeId;
      }
    }

    /// <summary>
    /// Gets the associated <see cref="PropertyType"/> given a propert type name or null if such property type does't exist.
    /// </summary>
    /// <param name="name">A property type name</param>
    /// <returns>The property type or null</returns>
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
    /// Return all the keys associated with the vertex type.
    /// </summary>
    /// <returns>the set of all string keys associated with the vertex type</returns>
    public IEnumerable<string> GetPropertyKeys()
    {
      foreach (var pair in stringToPropertyType)
      {
        yield return pair.Key;
      }
    }

    /// <summary>
    /// Return all the property types associated with vertex type.
    /// </summary>
    /// <returns>the set of property types associated with the vertex type</returns>
    public IEnumerable<PropertyType> GetPropertyTypes()
    {
      foreach (var pair in stringToPropertyType)
      {
        yield return pair.Value;
      }
    }

    /// <summary>
    /// Enumerates edges connected wih this vertex type
    /// </summary>
    /// <param name="etype">A type of edge type to look for</param>
    /// <param name="dir">Direction of edge</param>
    /// <returns></returns>
    public IEnumerable<IEdge> GetEdges(EdgeType etype, Direction dir)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      switch (dir)
      {
        case Direction.Out:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                Vertex vertex1 = GetVertex(p2.Key);
                foreach (UInt64 l in p2.Value)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = etype.GetEdge(eId, vertex1, vertex2);
                  yield return edge;
                }
              }
          break;
        case Direction.In:
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                Vertex vertex1 = GetVertex(p2.Key);
                foreach (UInt64 l in p2.Value)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = etype.GetEdge(eId, vertex2, vertex1);
                  yield return edge;
                }
              }
          break;
        case Direction.Both:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                Vertex vertex1 = GetVertex(p2.Key);
                foreach (UInt64 l in p2.Value)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = etype.GetEdge(eId, vertex1, vertex2);
                  yield return edge;
                }
              };
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                Vertex vertex1 = GetVertex(p2.Key);
                foreach (UInt64 l in p2.Value)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = etype.GetEdge(eId, vertex2, vertex1);
                  yield return edge;
                }
              }
          break;
      }
    }

    /// <summary>
    /// Get all edges found from a given <see cref="Vertex"/>
    /// </summary>
    /// <param name="vertex1">A <see cref="Vertex"/> id</param>
    /// <param name="dir">follow edges in this direction</param>
    /// <returns>an enumeration of edges</returns>
    public IEnumerable<IEdge> GetEdges(Vertex vertex1, Direction dir)
    {
      switch (dir)
      {
        case Direction.Out:
          foreach (var p0 in tailToHeadEdges)
            foreach (var p1 in p0.Value)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = p0.Key.GetEdge(eId, vertex1, vertex2);
                  yield return edge;
                }
              }
            }
          break;
        case Direction.In:
          foreach (var p0 in headToTailEdges)
            foreach (var p1 in p0.Value)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = p0.Key.GetEdge(eId, vertex2, vertex1);
                  yield return edge;
                }
              }
            }
          break;
        case Direction.Both:
          foreach (var p0 in tailToHeadEdges)
            foreach (var p1 in p0.Value)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = p0.Key.GetEdge(eId, vertex1, vertex2);
                  yield return edge;
                }
              }
            }
          foreach (var p0 in headToTailEdges)
            foreach (var p1 in p0.Value)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  EdgeId eId = (int)(l >> 32);
                  Edge edge = p0.Key.GetEdge(eId, vertex2, vertex1);
                  yield return edge;
                }
              }
            }
          break;
      }
    }

    /// <summary>
    /// Get all edges found between two vertices
    /// </summary>
    /// <param name="edgeType">Restrict to this type of edge</param>
    /// <param name="vertex1">Start <see cref="Vertex"/></param>
    /// <param name="dir">Follow edges in this direction</param>
    /// <param name="vertex2">End <see cref="Vertex"/></param>
    /// <returns>An enumeration of edges</returns>
    public IEnumerable<IEdge> GetEdges(EdgeType edgeType, Vertex vertex1, Direction dir, Vertex vertex2 = null)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      switch (dir)
      {
        case Direction.Out:
          if (tailToHeadEdges.TryGetValue(edgeType, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  if (vertex2 == null || vertex2.VertexId == vId)
                  {
#if VertexDebug
                    Vertex head = p1.Key.GetVertex(vId);
#else
                    Vertex head = new Vertex(graph, p1.Key, vId);
#endif
                    EdgeId eId = (int)(l >> 32);
#if EdgeDebug
                    Edge edge = edgeType.GetEdge(g, eId, head, vertex1);
#else
                    Edge edge = new Edge(graph, edgeType, eId, head, vertex1);
#endif
                    yield return edge;
                  }
                }
              }
            }
          break;
        case Direction.In:
          if (headToTailEdges.TryGetValue(edgeType, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  if (vertex2 == null || vertex2.VertexId == vId)
                  {
                    Vertex tailVertex = p1.Key.GetVertex(vId);
                    EdgeId eId = (int)(l >> 32);
                    Edge edge = edgeType.GetEdge(eId, tailVertex, vertex1);
                    yield return edge;
                  }
                }
              }
            }
          break;
        case Direction.Both:
          if (tailToHeadEdges.TryGetValue(edgeType, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  if (vertex2 == null || vertex2.VertexId == vId)
                  {
                    Vertex head = p1.Key.GetVertex(vId);
                    EdgeId eId = (int)(l >> 32);
                    Edge edge = edgeType.GetEdge(eId, vertex1, head);
                    yield return edge;
                  }
                }
              }
            }
          if (headToTailEdges.TryGetValue(edgeType, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertex1.VertexId, out edgeVertexSet))
              {
                foreach (UInt64 l in edgeVertexSet)
                {
                  VertexId vId = (int)l;
                  if (vertex2 == null || vertex2.VertexId == vId)
                  {
                    Vertex tail = p1.Key.GetVertex(vId);
                    EdgeId eId = (int)(l >> 32);
                    Edge edge = edgeType.GetEdge(eId, tail, vertex1);
                    yield return edge;
                  }
                }
              }
            }
          break;
      }
    }

    /// <summary>
    /// Get the number of edges of a certain type that can be found and an edge direction
    /// </summary>
    /// <param name="etype">Use this type of edge</param>
    /// <param name="dir">Edge direction to follow</param>
    /// <returns>Number of edges found</returns>
    public long GetNumberOfEdges(EdgeType etype, Direction dir)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      long numberOfEdges = 0;
      switch (dir)
      {
        case Direction.Out:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                numberOfEdges += p2.Value.Count;
              }
          break;
        case Direction.In:
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                numberOfEdges += p2.Value.Count;
              }
          break;
        case Direction.Both:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                numberOfEdges += p2.Value.Count;
              }
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                numberOfEdges += p2.Value.Count;
              }
          break;
      }
      return numberOfEdges;
    }

    /// <summary>
    /// Get the top vertices with the most number of edges of the given edge type
    /// </summary>
    /// <param name="etype">The edge type to look for</param>
    /// <param name="howMany">How many top ones to collect</param>
    /// <param name="dir">What end of edges to look at</param>
    /// <returns>Array of Vertices with the most edges of the given edge type</returns>
    public Vertex[] GetTopNumberOfEdges(EdgeType etype, int howMany, Direction dir)
    {
      Vertex[] top = new Vertex[howMany];
      int[] topCt = new int[howMany];
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      switch (dir)
      {
        case Direction.Out:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                if (topCt[0] == 0 || p2.Value.Count > 0)
                {
                  int pos = Array.BinarySearch(topCt, p2.Value.Count);
                  if (pos < 0)
                    pos = ~pos;
                  if (pos > 0)
                  {
                    --pos;
                    Array.Copy(topCt, 1, topCt, 0, pos);
                    Array.Copy(top, 1, top, 0, pos);
                  }
                  if (topCt[0] < p2.Value.Count)
                  {
                    topCt[pos] = p2.Value.Count;
                    top[pos] = new Vertex(graph, this, p2.Key);
                  }
                }
              }
          break;
        case Direction.In:
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                if (topCt[0] == 0 || p2.Value.Count > 0)
                {
                  int pos = Array.BinarySearch(topCt, p2.Value.Count);
                  if (pos < 0)
                    pos = ~pos;
                  if (pos > 0)
                  {
                    --pos;
                    Array.Copy(topCt, 1, topCt, 0, pos);
                    Array.Copy(top, 1, top, 0, pos);
                  }
                  if (topCt[0] < p2.Value.Count)
                  {
                    topCt[pos] = p2.Value.Count;
                    top[pos] = new Vertex(graph, p1.Key, p2.Key);
                  }
                }
              }
          break;
        case Direction.Both:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                if (topCt[0] == 0 || p2.Value.Count > 0)
                {
                  int pos = Array.BinarySearch(topCt, p2.Value.Count);
                  if (pos < 0)
                    pos = ~pos;
                  if (pos > 0)
                  {
                    --pos;
                    Array.Copy(topCt, 1, topCt, 0, pos);
                    Array.Copy(top, 1, top, 0, pos);
                  }
                  if (topCt[0] < p2.Value.Count)
                  {
                    topCt[pos] = p2.Value.Count;
                    top[pos] = new Vertex(graph, p1.Key, p2.Key);
                  }
                }
              }
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
              foreach (var p2 in p1.Value)
              {
                if (topCt[0] == 0 || p2.Value.Count > 0)
                {
                  int pos = Array.BinarySearch(topCt, p2.Value.Count);
                  if (pos < 0)
                    pos = ~pos;
                  if (pos > 0)
                  {
                    --pos;
                    Array.Copy(topCt, 1, topCt, 0, pos);
                    Array.Copy(top, 1, top, 0, pos);
                  }
                  if (topCt[0] < p2.Value.Count)
                  {
                    topCt[pos] = p2.Value.Count;
                    top[pos] = new Vertex(graph, p1.Key, p2.Key);
                  }
                }
              }
          break;
      }
      return top;
    }

    /// <summary>
    /// Get the number of edges of a certain type that can be found associated with a vertex id and an edge direction
    /// </summary>
    /// <param name="etype">Type of edges to look for</param>
    /// <param name="vertexId">Id of a <see cref="Vertex"/></param>
    /// <param name="dir">Edge direction to use</param>
    /// <returns>The number of edges found</returns>
    public long GetNumberOfEdges(EdgeType etype, VertexId vertexId, Direction dir)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      long numberOfEdges = 0;
      switch (dir)
      {
        case Direction.Out:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertexId, out edgeVertexSet))
                numberOfEdges += edgeVertexSet.Count;
            }
          break;
        case Direction.In:
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertexId, out edgeVertexSet))
                numberOfEdges += edgeVertexSet.Count;
            }
          break;
        case Direction.Both:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertexId, out edgeVertexSet))
                numberOfEdges += edgeVertexSet.Count;
            }
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              BTreeSet<EdgeIdVertexId> edgeVertexSet;
              if (p1.Value.TryGetValue(vertexId, out edgeVertexSet))
                numberOfEdges += edgeVertexSet.Count;
            }
          break;
      }
      return numberOfEdges;
    }

    /// <summary>
    /// Enumerates all vertices for the given type
    /// </summary>
    /// <param name="polymorphic">If true, also include all vertices of sub types of this VertexType</param>
    /// <returns>Enumeration of vertices</returns>
    public IEnumerable<Vertex> GetVertices(bool polymorphic = false)
    {
      foreach (Range<VertexId> range in vertecis)
        foreach (VertexId vId in Enumerable.Range((int)range.Min, (int)range.Max - range.Min + 1))
          yield return new Vertex(graph, this, vId);
      if (polymorphic)
      {
        foreach (VertexType vt in subType)
          foreach (Vertex v in vt.GetVertices(polymorphic))
            yield return v;
      }
    }

    /// <summary>
    /// Count of vertcies
    /// </summary>
    /// <returns>the number of verices that exist for this type in this <see cref="Graph"/></returns>
    public long CountVertices()
    {
      long ct = 0;
      foreach (Range<VertexId> range in vertecis)
        ct += range.Max - range.Min + 1;
      return ct;
    }

    /// <summary>
    /// Get existing <see cref="Vertex"/> ids for this type
    /// </summary>
    /// <returns>An enumeration of vertex ids</returns>
    public IEnumerable<VertexId> GetVerticeIds()
    {
      foreach (Range<VertexId> range in vertecis)
        foreach (int vId in Enumerable.Range((int)range.Min, (int)range.Max - range.Min + 1))
          yield return (VertexId) vId;
    }

    /// <summary>
    /// Get an enumeration of existing vertices of this type found by following an edge type
    /// </summary>
    /// <param name="etype">Edge type to follow</param>
    /// <param name="vertex1">Vertex to start from</param>
    /// <param name="dir">Edge direction to follow</param>
    /// <returns>an enumeration of vertices</returns>
    public IEnumerable<IVertex> GetVertices(EdgeType etype, Vertex vertex1, Direction dir)
    {
      BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>> map;
      BTreeSet<EdgeIdVertexId> set;
      switch (dir)
      {
        case Direction.Out:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              if (p1.Value.TryGetValue(vertex1.VertexId, out set))
              {
                foreach (long l in set)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  yield return vertex2;
                }
              }
            }
          break;
        case Direction.In:
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              if (p1.Value.TryGetValue(vertex1.VertexId, out set))
              {
                foreach (long l in set)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  yield return vertex2;
                }
              }
            }
          break;
        case Direction.Both:
          if (tailToHeadEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              if (p1.Value.TryGetValue(vertex1.VertexId, out set))
              {
                foreach (long l in set)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  yield return vertex2;
                }
              }
            }
          if (headToTailEdges.TryGetValue(etype, out map))
            foreach (var p1 in map)
            {
              if (p1.Value.TryGetValue(vertex1.VertexId, out set))
              {
                foreach (long l in set)
                {
                  VertexId vId = (int)l;
                  Vertex vertex2 = p1.Key.GetVertex(vId);
                  yield return vertex2;
                }
              }
            }
          break;
      }
    }

    /// <summary>
    /// Removes a vertex from this type and graph
    /// </summary>
    /// <param name="vertex">The vertex to remove</param>
    public void RemoveVertex(Vertex vertex)
    {
      BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>> innerMap;
      BTreeSet<EdgeIdVertexId> edgeVertexSet;
      foreach (var m in headToTailEdges)
      {
        if (m.Key.TailType != null)
        {
          List<Edge> edgesToRemove = new List<Edge>();
          if (m.Value.TryGetValue(m.Key.TailType, out innerMap))
            if (innerMap.TryGetValue(vertex.VertexId, out edgeVertexSet))
            {
              foreach (UInt64 l in edgeVertexSet)
              {
                VertexId vId = (int)l;
                Vertex vertex2 = m.Key.TailType.GetVertex(vId);
                EdgeId eId = (int)(l >> 32);
                Edge edge = m.Key.GetEdge(eId, vertex2, vertex);
                edgesToRemove.Add(edge);
              }
              foreach (Edge edge in edgesToRemove)
                m.Key.RemoveEdge(edge);
            }
        }
        else
          throw new NotImplementedException();
      }

      foreach (var m in headToTailEdges)
      {
        if (m.Key.TailType != null)
        {
          if (m.Value.TryGetValue(m.Key.TailType, out innerMap))
            if (innerMap.TryGetValue(vertex.VertexId, out edgeVertexSet))
            {
              innerMap.Remove(vertex.VertexId);
              edgeVertexSet.Unpersist(Session);
            }
        }
        else
          throw new NotImplementedException();
      }

      foreach (var m in tailToHeadEdges)
      {
        if (m.Key.HeadType != null)
        {
          List<Edge> edgesToRemove = new List<Edge>();
          if (m.Value.TryGetValue(m.Key.HeadType, out innerMap))
            if (innerMap.TryGetValue(vertex.VertexId, out edgeVertexSet))
            {
              foreach (UInt64 l in edgeVertexSet)
              {
                VertexId vId = (int)l;
                Vertex vertex2 = m.Key.HeadType.GetVertex(vId);
                EdgeId eId = (int)(l >> 32);
                Edge edge = m.Key.GetEdge(eId, vertex, vertex2);
                edgesToRemove.Add(edge);
              }
              foreach (Edge edge in edgesToRemove)
                m.Key.RemoveEdge(edge);
            }
        }
        else
          throw new NotImplementedException();
      }

      foreach (var m in tailToHeadEdges)
      {
        if (m.Key.HeadType != null)
        {
          if (m.Value.TryGetValue(m.Key.HeadType, out innerMap))
            if (innerMap.TryGetValue(vertex.VertexId, out edgeVertexSet))
            {
              innerMap.Remove(vertex.VertexId);
              edgeVertexSet.Unpersist(Session);
            }
        }
        else
          throw new NotImplementedException();
      }

      foreach (PropertyType pt in GetPropertyTypes())
        pt.RemovePropertyValue(vertex.VertexId);

      graph.DeAllocateVertexId(vertex.VertexId);
      graph.DeAllocateVertexId(vertex.VertexId, vertecis);
    }

    /// <summary>
    /// Get the property value for a <see cref="Vertex"/>
    /// </summary>
    /// <param name="vertexId">Id of <see cref="Vertex"/></param>
    /// <param name="propertyType">Type of property</param>
    /// <returns></returns>
    public object GetPropertyValue(VertexId vertexId, PropertyType propertyType)
    {
      return propertyType.GetPropertyValue(vertexId);
    }

    /// <summary>
    /// Sets a property value
    /// </summary>
    /// <param name="vertexId">Id of <see cref="Vertex"/> for which to set property value</param>
    /// <param name="propertyType">The type of property to set</param>
    /// <param name="v">the value to set the property to</param>
    public void SetPropertyValue(VertexId vertexId, PropertyType propertyType, IComparable v)
    {
      propertyType.SetPropertyValue(vertexId, typeId, v);
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
    /// Gets the name of this <see cref="VertexType"/> 
    /// </summary>
    public string TypeName
    {
      get
      {
        return typeName;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return "VertexType: " + typeName;
    }

    /// <inheritdoc />
    public override void Unpersist(SessionBase session, bool disableFlush = true)
    {
      if (IsPersistent == false)
        return;
      subType.Unpersist(session, disableFlush);
      vertecis.Unpersist(session, disableFlush);
      stringToPropertyType.Unpersist(session, disableFlush);
      edgeTypes.Unpersist(session, disableFlush);
      tailToHeadEdges.Unpersist(session, disableFlush);
      headToTailEdges.Unpersist(session, disableFlush);
      base.Unpersist(session, disableFlush);
      graph.RemoveVertexTypeRef(this);
    }
  }
}
