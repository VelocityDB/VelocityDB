using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeId = System.Int32;
using VertexId = System.Int32;
using VertexTypeId = System.Int32;
using PropertyId = System.Int32;
using EdgeTypeId = System.Int32;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace VelocityGraph
{
  /// <summary>
  /// A vertex maintains pointers to both a set of incoming and outgoing edges.
  /// The outgoing edges are those edges for which the vertex is the tail.
  /// The incoming edges are those edges for which the vertex is the head.
  /// Diagrammatically, ---inEdges---> vertex ---outEdges--->
  /// </summary>
  public class Vertex : Element, IVertex
  {
    VertexType m_vertexType;
    /// <summary>
    /// Normally you should use <see cref="VelocityGraph.VertexType.GetVertex(VertexId,bool,bool)"/> but if you need a reference to a Vertex that has no yet been created, this constructor may be used (but know what you are doing!)
    /// </summary>
    /// <param name="g">the owning graph</param>
    /// <param name="eType">the type of the Vertex</param>
    /// <param name="eId">the Id of the Vertex</param>
    public Vertex(Graph g, VertexType eType, VertexId eId)
      : base(eId, g)
    {
      m_vertexType = eType;
    }

    /// <summary>
    /// Add an edge from this Vertex to inVertex of edge type looked up from label, if edge type does not yet exist it is created.
    /// </summary>
    /// <param name="label">The type of edge to create</param>
    /// <param name="inVertex">The head of the new edge</param>
    /// <returns>the new edge</returns>
    public IEdge AddEdge(string label, IVertex inVertex)
    {
      return AddEdge(null, label, inVertex);
    }

    /// <summary>
    /// Add an edge from this Vertex to inVertex of edge type looked up from label, if edge type does not yet exist it is created.
    /// </summary>
    /// <param name="edgeId">If not null, this must be a UInt32 to be used as edge id - NOTE: not yet implemented usage</param>
    /// <param name="label">The type of edge to create</param>
    /// <param name="inVertex">The head of the new edge</param>
    /// <returns>the new edge</returns>
    public IEdge AddEdge(object edgeId, string label, IVertex inVertex)
    { // TODO, use edgeId
      if (edgeId != null)
        throw new NotImplementedException("Custom edge id is not implemented");
      EdgeType edgeType = Graph.FindEdgeType(label);
      if (edgeType == null)
        edgeType = Graph.NewEdgeType(label, true);
      return edgeType.NewEdge(this, inVertex as Vertex);
    }

    /// <summary>
    /// Add an edge from this Vertex to inVertex of edge type.
    /// </summary>
    /// <param name="edgeType">The type of edge to add</param>
    /// <param name="head">The head of the new edge</param>
    /// <returns>the new edge</returns>
    public Edge AddEdge(EdgeType edgeType, Vertex head)
    {
      return edgeType.NewEdge(this, head);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Vertex"/> is equal to the current one.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(Object other)
    {
      bool isEqual = ReferenceEquals(this, other);
      if (isEqual)
        return true;
      Vertex otherVertex = other as Vertex;
      if (otherVertex != null)
        return VertexId == otherVertex.VertexId && VertexType == otherVertex.VertexType;
      return false;
    }

    /// <summary>
    /// An identifier that is unique to its inheriting class.
    /// All vertices of a graph must have unique identifiers.
    /// All edges of a graph must have unique identifiers.
    /// </summary>
    /// <returns>the identifier of the element</returns>
    public override object Id
    {
      get
      {
        UInt64 fullId = (UInt64)m_vertexType.TypeId;
        fullId <<= 32;
        fullId += (UInt64)m_id;
        return fullId;
      }
    }

    /// <summary>
    /// Gets the number of edges from or to this vertex and for the given edge type. 
    /// </summary>
    /// <param name="edgeType">an EdgeType</param>
    /// <param name="dir">direction, one of: Out, In, Both</param>
    /// <returns>The number of edges.</returns>
    public long GetNumberOfEdges(EdgeType edgeType, Direction dir)
    {
      return m_vertexType.GetNumberOfEdges(edgeType, this.VertexId, dir);
    }

    /// <summary>
    /// Gets the number of edges from or to this vertex for the given edge type and the given other Vertex. 
    /// </summary>
    /// <param name="edgeType">an EdgeType</param>
    /// <param name="headVertex">Vertex at other end of the edge</param>
    /// <param name="dir">direction, one of: Out, In, Both</param>
    /// <returns>The number of edges.</returns>
    public long GetNumberOfEdges(EdgeType edgeType, Vertex headVertex, Direction dir)
    {
      return m_vertexType.GetNumberOfEdges(edgeType, this.VertexId, headVertex.VertexId, dir);
    }

    /// <summary>
    /// Return the object value associated with the provided string key.
    /// If no value exists for that key, return null.
    /// </summary>
    /// <param name="key">the key of the key/value property</param>
    /// <returns>the object value related to the string key</returns>
    public override object GetProperty(string key)
    {
      PropertyType pt = m_vertexType.FindProperty(key);
      return m_vertexType.GetPropertyValue(m_id, pt);
    }

    /// <summary>
    /// Return all the keys associated with the vertex.
    /// </summary>
    /// <returns>the set of all string keys associated with the vertex</returns>
    public override IEnumerable<string> GetPropertyKeys()
    {
      foreach (string key in m_vertexType.GetPropertyKeys())
      {
        if (GetProperty(key) != null)
          yield return key;
      }
    }

    /// <summary>
    /// Gets the Value for the given Property id
    /// </summary>
    /// <param name="property">Property type identifier.</param>
    /// <returns>the property value</returns>
    public object GetProperty(PropertyType property)
    {
      return m_vertexType.GetPropertyValue(VertexId, property);
    }

    /// <summary>
    /// Selects all edges from or to this vertex and for the given edge type. 
    /// </summary>
    /// <param name="edgeType">the id of an EdgeType</param>
    /// <param name="dir">direction, one of: Out, In, Both</param>
    /// <returns>a set of Edge</returns>
    public IEnumerable<IEdge> GetEdges(EdgeType edgeType, Direction dir)
    {
      return m_vertexType.GetEdges(edgeType, this, dir);
    }

    /// <summary>
    /// Return the edges incident to the vertex according to the provided direction and edge labels.
    /// </summary>
    /// <param name="direction">the direction of the edges to retrieve</param>
    /// <param name="labels">the labels of the edges to retrieve</param>
    /// <returns>an IEnumerable of incident edges</returns>
    public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
    {
      switch (direction)
      {
        case Direction.Out:
          foreach (IEdge edge in GetOutEdges(labels))
            yield return edge;
          break;
        case Direction.In:
          foreach (IEdge edge in GetInEdges(labels))
            yield return edge;
          break;
        default:
          foreach (IEdge edge in GetInEdges(labels))
            yield return edge;
          foreach (IEdge edge in GetOutEdges(labels))
            yield return edge;
          break;
      };
    }

    IEnumerable<IEdge> GetInEdges(params string[] labels)
    {
      if (labels.Length == 0)
        foreach (IEdge edge in m_vertexType.GetEdges(this, Direction.In))
          yield return edge;
      else
      {
        foreach (string label in labels)
        {
          EdgeType edgeType = Graph.FindEdgeType(label);
          if (edgeType != null)
          {
            foreach (IEdge edge in m_vertexType.GetEdges(edgeType, this, Direction.In))
              yield return edge;
          }
        }
      }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return m_id.GetHashCode();
    }
    /// <summary>
    /// Enumerates all outgoing edges of selected edge types
    /// </summary>
    /// <param name="labels">Array of edge type names to find edges for or if empty, get all outgoing edges from this vertex type.</param>
    /// <returns>Enumeration of edges</returns>
    IEnumerable<IEdge> GetOutEdges(params string[] labels)
    {
      if (labels.Length == 0)
      {
        foreach (IEdge edge in m_vertexType.GetEdges(this, Direction.Out))
          yield return edge;
      }
      else
      {
        foreach (string label in labels)
        {
          EdgeType edgeType = Graph.FindEdgeType(label);
          foreach (IEdge edge in m_vertexType.GetEdges(edgeType, this, Direction.Out))
            yield return edge;
        }
      }
    }

    /// <summary>
    /// Return the vertices adjacent to the vertex according to the provided direction and edge labels.
    /// This method does not remove duplicate vertices (i.e. those vertices that are connected by more than one edge).
    /// </summary>
    /// <param name="direction">the direction of the edges of the adjacent vertices</param>
    /// <param name="labels">the labels of the edges of the adjacent vertices</param>
    /// <returns>an IEnumerable of adjacent vertices</returns>
    public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
    {
      if (labels.Length == 0)
      {
        HashSet<EdgeType> edgeTypes = new HashSet<EdgeType>();
        foreach (Edge edge in GetEdges(direction, labels))
          edgeTypes.Add(edge.EdgeType);
        foreach (EdgeType edgeType in edgeTypes)
          foreach (IVertex vertex in m_vertexType.GetVertices(edgeType, this, direction))
            yield return vertex;
      }
      else
      {
        foreach (string label in labels)
        {
          EdgeType edgeType = Graph.FindEdgeType(label);
          foreach (IVertex vertex in m_vertexType.GetVertices(edgeType, this, direction))
            yield return vertex;
        }
      }
    }

    /// <summary>
    /// Gets the unique id of a vertex
    /// </summary>
    public VertexId VertexId
    {
      get
      {
        return m_id;
      }
    }

    /// <summary>
    /// Gets the vertex type of this Vertex
    /// </summary>
    public VertexType VertexType
    {
      get
      {
        return m_vertexType;
      }
    }

    /// <summary>
    /// Selects all neighbor Vertices from or to this vertex and for the given edge type.
    /// </summary>
    /// <param name="etype">Edge type identifier.</param>
    /// <param name="dir">Direction to traverse edges</param>
    /// <returns>Dictionary of vertex key with edge path(s) to vertex</returns>
    public Dictionary<Vertex, HashSet<Edge>> Traverse(EdgeType etype, Direction dir)
    {
      return m_vertexType.Traverse(this, etype, dir);
    }

    /// <summary>
    /// Selects all neighbor Vertices from or to this vertex and for the given edge types.
    /// </summary>
    /// <param name="dir">Direction to traverse edges</param>
    /// <param name="edgeTypesToTraverse">the type of edges to follow, by default null which means follow all edge types</param>
    /// <returns></returns>
    public Dictionary<Vertex, HashSet<Edge>> Traverse(Direction dir, ISet<EdgeType> edgeTypesToTraverse = null)
    {
      return m_vertexType.Traverse(this, dir, edgeTypesToTraverse);
    }

    class PathInfo
    {
      Vertex m_node;
      List<Edge> m_edgePath;
      HashSet<Vertex> m_visited;
      public PathInfo(Vertex node, List<Edge> edgePath, HashSet<Vertex> visited)
      {
        m_node = node;
        m_edgePath = edgePath;
        m_visited = new HashSet<Vertex>();
        if (visited != null)
          m_visited.UnionWith(visited);
      }
      public Vertex Node
      {
        get
        {
          return m_node;
        }
      }

      public List<Edge> EdgePath
      {
        get
        {
          return m_edgePath;
        }
      }

      public HashSet<Vertex> Visited
      {
        get
        {
          return m_visited;
        }
      }
    }

    /// <summary>
    /// Traverses graph from this Vertex to a target Vertex using Breadth-first search like in Dijkstra's algorithm
    /// </summary>
    /// <param name="maxHops">maximum number of hops from this Vertex</param>
    /// <param name="all">find or not find all paths to goal Vertex</param>
    /// <param name="dir">Direction to traverse edges</param>
    /// <param name="toVertex">the goal Vertex. If null, finds all paths</param>
    /// <param name="edgeTypesToTraverse">the type of edges to follow, by default null which means follow all edge types</param>
    /// <param name="includedVertexTypes">the type of vertices's to follow, by default null which means follow all vertex types</param>
    /// <param name="excludedVertexTypes">the type of vertices's not to follow, by default null</param>
    /// <param name="includedVertices">one or more Vertex instances that MUST be in the path for the path to be traversed i.e. if a path does exist
    /// to the specified toVertex, but does not include all the instances in includedVertices set, the Traverse method will exclude that path</param>
    /// <param name="excludedVertices">one or more Vertex instances that MUST NOT be in the path for the path to be traversed i.e. if a path does exist
    /// to the specified toVertex, but does include any of the instances in includedVertices set, the Traverse method will exclude that path</param>
    /// <param name="includedEdges">one or more Edge instances that MUST be in the path for the path to be traversed i.e. if a path does exist
    /// to the specified toVertex, but does not include all the instances in includedEdges set, the Traverse method will exclude that path</param>
    /// <param name="excludedEdges">one or more Edge instances that MUST NOT be in the path for the path to be traversed i.e. if a path does exist
    /// to the specified toVertex, but does include any of the instances in includedEdges set, the Traverse method will exclude that path</param>
    /// <param name="includedVertexProperty">One or more Vertex property types that MUST be in the path for the path to be accepted i.e. if a path does exist
    /// to the specified toVertex, but does not include all of the Vertex properties in the set, the Traverse method will exclude that path</param>
    /// <param name="excludedVertexProperty">One or more Vertex property types that MUST NOT be in the path for the path to be accepted i.e. if a path does exist
    /// to the specified toVertex, but does include any of the Vertex properties in the set, the Traverse method will exclude that path</param>
    /// <param name="includedEdgeProperty">One or more Vertex property types that MUST be in the path for the path to be accepted i.e. if a path does exist
    /// to the specified toVertex, but does not include all of the Vertex properties in the set, the Traverse method will exclude that path</param>
    /// <param name="excludedEdgeProperty">One or more Edge property types that MUST NOT be in the path for the path to be accepted i.e. if a path does exist
    /// to the specified toVertex, but does include any of the Edge properties in the set, the Traverse method will exclude that path</param>
    /// <param name="validateVertex">A function that will be called before accepting a Vertex in path to toVertex. If function returns true then this vertex is accepted in path; otherwise vertex is rejected</param>
    /// <param name="validateEdge">A function that will be called before accepting an Edge in path to toVertex. If function returns true then this Edge is accepted in path; otherwise edge is rejected</param>
    /// <param name="validateEdges">A function that will be called before accepting a candidate Edges list in path to toVertex. If function returns true then this Edge list is accepted in path; otherwise edge list is rejected</param>
    /// <returns>List of paths to goal Vertex</returns>
    public List<List<Edge>> Traverse(int maxHops, bool all = true, Direction dir = Direction.Both, Vertex toVertex = null, ISet<EdgeType> edgeTypesToTraverse = null,
      ISet<VertexType> includedVertexTypes = null, ISet<VertexType> excludedVertexTypes = null, ISet<Vertex> includedVertices = null, ISet<Vertex> excludedVertices = null,
      ISet<Edge> includedEdges = null, ISet<Edge> excludedEdges = null, ISet<PropertyType> includedVertexProperty = null, ISet<PropertyType> excludedVertexProperty = null,
      ISet<PropertyType> includedEdgeProperty = null, ISet<PropertyType> excludedEdgeProperty = null, Func<Vertex, bool> validateVertex = null, Func<Edge, bool> validateEdge = null,
      Func<List<Edge>, bool> validateEdges = null)
    {
      Queue<PathInfo> q = new Queue<PathInfo>();
      HashSet<PropertyType> vertexPropertyTypesToFind = null;
      HashSet<PropertyType> edgePropertyTypesToFind = null;
      HashSet<Edge> edgeSet;
      List<Edge> path = new List<Edge>();
      List<List<Edge>> resultPaths = new List<List<Edge>>();
      if (excludedVertexProperty != null)
      {
        foreach (PropertyType pt in excludedVertexProperty)
        {
          if (pt.HasPropertyValue(VertexId))
            return resultPaths;
          if (pt.HasPropertyValue(toVertex.VertexId))
            return resultPaths;
        }
      }
      int includedVerticesSize;
      if (includedVertices != null)
      { // these will always be included so remove from list
        includedVertices.Remove(this);
        includedVertices.Remove(toVertex);
        includedVerticesSize = includedVertices.Count;
      }
      else
        includedVerticesSize = 0;
      int includedVertexPropertySize;
      if (includedVertexProperty != null)
      {
        vertexPropertyTypesToFind = new HashSet<PropertyType>(includedVertexProperty);
        foreach (PropertyType pt in vertexPropertyTypesToFind)
        {
          if (pt.HasPropertyValue(VertexId))
            vertexPropertyTypesToFind.Remove(pt);
          else if (pt.HasPropertyValue(toVertex.VertexId))
            vertexPropertyTypesToFind.Remove(pt);
        }
        includedVertexPropertySize = vertexPropertyTypesToFind.Count;
      }
      else
        includedVertexPropertySize = 0;
      int includedEdgePropertySize;
      if (includedEdgeProperty != null)
      {
        edgePropertyTypesToFind = new HashSet<PropertyType>(includedEdgeProperty);
        includedEdgePropertySize = edgePropertyTypesToFind.Count;
      }
      else
        includedEdgePropertySize = 0;
      PathInfo pathInfo = new PathInfo(this, path, null);
      if (excludedVertices != null)
        pathInfo.Visited.UnionWith(excludedVertices);
      pathInfo.Visited.Add(this);
      if (toVertex != null)
        pathInfo.Visited.Add(toVertex);
      q.Enqueue(pathInfo);
      while (q.Count > 0)
      {
        pathInfo = q.Dequeue();
        Dictionary<Vertex, HashSet<Edge>> friends = pathInfo.Node.Traverse(dir, edgeTypesToTraverse);
        if (toVertex != null && friends.TryGetValue(toVertex, out edgeSet))
        {
          foreach (Edge edge in edgeSet)
          {
            if ((excludedEdges == null || excludedEdges.Contains(edge) == false) && (validateEdge == null || validateEdge(edge)))
            {
              //Console.WriteLine(this + " and " + toVertex + " have a friendship link");
              List<Edge> edgePath = pathInfo.EdgePath;
              edgePath.Add(edge);
              if (validateEdges == null || validateEdges(edgePath))
              {
                bool foundVerticesToInclude = includedVerticesSize == 0;
                if (includedVerticesSize > 0)
                {
                  HashSet<Vertex> verticesToFind = new HashSet<Vertex>(includedVertices);
                  foreach (Edge edgeInPath in edgePath)
                  {
                    if (verticesToFind.Contains(edgeInPath.Tail))
                    {
                      verticesToFind.Remove(edgeInPath.Tail);
                      if (verticesToFind.Count == 0)
                        break;
                    }
                  }
                  foundVerticesToInclude = verticesToFind.Count == 0;
                }
                bool foundVertexPropertyTypesToInclude = includedVertexPropertySize == 0;
                if (includedVertexPropertySize > 0)
                {
                  foreach (Edge edgeInPath in edgePath)
                  {
                    foreach (PropertyType pt in vertexPropertyTypesToFind)
                    {
                      if (pt.HasPropertyValue(edgeInPath.Tail.VertexId))
                        vertexPropertyTypesToFind.Remove(pt);
                    }
                    if (vertexPropertyTypesToFind.Count == 0)
                      break;
                  }
                  foundVertexPropertyTypesToInclude = vertexPropertyTypesToFind.Count == 0;
                }
                bool foundEdgePropertyTypesToInclude = includedEdgePropertySize == 0;
                if (includedEdgePropertySize > 0)
                {
                  foreach (Edge edgeInPath in edgePath)
                  {
                    foreach (PropertyType pt in edgePropertyTypesToFind)
                    {
                      if (pt.HasPropertyValue(edgeInPath.EdgeId))
                        edgePropertyTypesToFind.Remove(pt);
                    }
                    if (edgePropertyTypesToFind.Count == 0)
                      break;
                  }
                  foundEdgePropertyTypesToInclude = edgePropertyTypesToFind.Count == 0;
                }
                if (foundVerticesToInclude && foundVertexPropertyTypesToInclude && foundEdgePropertyTypesToInclude)
                {
                  if (includedEdges == null || includedEdges.IsSubsetOf(edgePath))
                  {
                    resultPaths.Add(edgePath);
                    if (!all)
                      return resultPaths;
                  }
                }
              }
            }
          }
        }
        if (pathInfo.EdgePath.Count < maxHops || friends.Count == 0)
          foreach (KeyValuePair<Vertex, HashSet<Edge>> v in friends)
          {
            if (pathInfo.Visited.Contains(v.Key) == false)
              foreach (Edge edge in v.Value)
              {
                if (excludedEdges == null || excludedEdges.Contains(edge) == false)
                {
                  bool doExclude = false;
                  if (excludedVertexProperty != null)
                  {
                    foreach (PropertyType pt in excludedVertexProperty)
                    {
                      if (pt.HasPropertyValue(v.Key.VertexId))
                      {
                        pathInfo.Visited.Add(v.Key);
                        doExclude = true;
                        break;
                      }
                    }
                  }

                  if (excludedEdgeProperty != null)
                  {
                    foreach (PropertyType pt in excludedEdgeProperty)
                    {
                      if (pt.HasPropertyValue(edge.EdgeId))
                      {
                        doExclude = true;
                        break;
                      }
                    }
                  }

                  if (!doExclude)
                  {                   
                    path = new List<Edge>(pathInfo.EdgePath);
                    path.Add(edge);
                    PathInfo newPath = new PathInfo(v.Key, path, pathInfo.Visited);
                    newPath.Visited.Add(v.Key);
                    if (validateEdges == null || validateEdges(path))
                    {
                      bool vertexTypeIncluded = includedVertexTypes == null || includedVertexTypes.Contains(v.Key.VertexType);
                      bool vertexTypeExcluded = excludedVertexTypes != null && excludedVertexTypes.Contains(v.Key.VertexType);
                      bool validVertex = validateVertex == null || validateVertex(v.Key);
                      if (vertexTypeIncluded && validVertex && !vertexTypeExcluded)
                      {
                        q.Enqueue(newPath);
                        if (toVertex == null && newPath.EdgePath.Count <= maxHops)
                        {
                          resultPaths.Add(newPath.EdgePath);
                          if (!all)
                            return resultPaths;
                        }
                      }
                    }
                  }
                }
              }
          }
      }
      //if (all && resultPaths.Count == 0)
      //  Console.WriteLine(this + " and " + toVertex + " may not be connected by indirect frienship");
      return resultPaths;
    }


    /// <summary>
    /// Uses <see cref="DefaultVertexQuery"/>
    /// </summary>
    /// <returns>Query interface</returns>
    public IVertexQuery Query()
    { // TO DO - Optimize
      return new DefaultVertexQuery(this);
    }

    /// <inheritdoc />
    public override void Remove()
    {
      m_vertexType.RemoveVertex(this);
    }

    /// <summary>
    /// Un-assigns a key/value property from the vertex.
    /// The object value of the removed property is returned.
    /// </summary>
    /// <param name="key">the key of the property to remove from the vertex</param>
    /// <returns>the object value associated with that key prior to removal</returns>
    public override object RemoveProperty(string key)
    {
      PropertyType pt = m_vertexType.FindProperty(key);
      if (pt == null)
        return null;
      return pt.RemovePropertyValue(m_id);
    }

    /// <summary>
    /// Assign a key/value property to the vertex.
    /// If a value already exists for this key, then the previous key/value is overwritten. 
    /// </summary>
    /// <param name="property">The property type to set</param>
    /// <param name="v">the property value</param>
    public void SetProperty(PropertyType property, IComparable v)
    {
      m_vertexType.SetPropertyValue(VertexId, property, v);
    }

    /// <summary>
    /// Assign a key/value property to the vertex.
    /// If a value already exists for this key, then the previous key/value is overwritten.
    /// </summary>
    /// <param name="key">the string key of the property</param>
    /// <param name="value">the object value of the property</param>
    public override void SetProperty(string key, object value)
    {
      if (key == null || key.Length == 0)
        throw new ArgumentException("Property key may not be null or be an empty string");
      if (value == null)
        throw new ArgumentException("Property value may not be null");
      if (key.Equals(StringFactory.Id))
        throw ExceptionFactory.PropertyKeyIdIsReserved();
      PropertyType pt = m_vertexType.FindProperty(key);
      if (pt == null)
        pt = m_vertexType.MyGraph.NewVertexProperty(m_vertexType, key, DataType.Object, PropertyKind.Indexed);
      m_vertexType.SetPropertyValue(VertexId, pt, (IComparable)value);
    }

    /// <summary>
    /// Assign a key/value property to the vertex.
    /// If a value already exists for this key, then the previous key/value is overwritten.
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">the string key of the property</param>
    /// <param name="value">the object value o the property</param>
    internal override void SetProperty<T>(string key, T value)
    {
      PropertyType pt = m_vertexType.FindProperty(key);
      if (pt == null)
        pt = m_vertexType.MyGraph.NewVertexProperty(m_vertexType, key, DataType.Object, PropertyKind.Indexed);
      m_vertexType.SetPropertyValue(VertexId, pt, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return "Vertex: " + VertexId + " " + m_vertexType.TypeName;
    }
  }
}
