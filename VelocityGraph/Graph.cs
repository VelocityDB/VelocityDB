using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using ElementId = System.Int32;
using VertexId = System.Int32;
using EdgeId = System.Int32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using VertexTypeId = System.Int32;
using TypeId = System.Int32;
using EdgeTypeId = System.Int32;
using EdgeIdVertexId = System.UInt64;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Blueprints.Util.IO.GraphSON;
using System.Globalization;
using VelocityDb.Collection;
using System.IO;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints.Util.IO.GraphJson;

namespace VelocityGraph
{
  /// <summary>
  /// Choose <see cref="VelocityDb.IOptimizedPersistable"/> instead of <see cref="object"/> for property types where you only will store objects of type IOptimizedPersistable as property value.
  /// </summary>
  public enum DataType
  {
    /// <summary>
    /// Correspond to <see cref="bool"/>
    /// </summary>
    Boolean,
    /// <summary>
    /// Correspond to <see cref="int"/>
    /// </summary>
    Integer,
    /// <summary>
    /// Correspond to <see cref="long"/>
    /// </summary>
    Long,
    /// <summary>
    /// Correspond to <see cref="double"/>
    /// </summary>
    Double,
    /// <summary>
    /// Correspond to <see cref="Single"/>
    /// </summary>
    Single,
    /// <summary>
    /// Correspond to <see cref="System.DateTime"/>
    /// </summary>
    DateTime,
    /// <summary>
    /// Correspond to <see cref="string"/>
    /// </summary>
    String,
    /// <summary>
    /// Correspond to <see cref="object"/>
    /// </summary>
    Object,
    /// <summary>
    /// Correspond to <see cref="VelocityDb.IOptimizedPersistable"/>
    /// </summary>
    IOptimizedPersistable
  }

  /// <summary>
  /// A property can be index or not or index can require each entry to be unique
  /// </summary>
  public enum PropertyKind
  {
    /// <summary>
    /// Add to index
    /// </summary>
    Indexed,
    /// <summary>
    /// Add to index and require value to be unique
    /// </summary>
    Unique,
    /// <summary>
    /// Don't index
    /// </summary>
    NotIndexed
  }
  /// <summary>
  /// Graph is the root object of a graph. Most graph api is on this class but useful api also exist on <see cref="VertexType"/> and <see cref="EdgeType"/>.
  /// </summary>
  [Serializable]
  public partial class Graph : OptimizedPersistable, IGraph
  {
    enum GraphFlags { VertexIdSetPerType = 1 };
    UInt32 flags;
    BTreeMap<string, VertexType> stringToVertexType;
    internal VertexType[] vertexType;
    BTreeMap<string, EdgeType> stringToEdgeType;
    internal EdgeType[] edgeType;
    int vertexTypeCt;
    int edgeTypeCt;
    internal PropertyType[] propertyType;
    VelocityDbList<Range<VertexId>> vertecis;
    internal BTreeMap<VertexId, VertexTypeId> vertexIdToVertexType;
    [NonSerialized]
    SessionBase session;
    static readonly Features features = new Features();

    static Graph()
    {
      features.SupportsDuplicateEdges = true;
      features.SupportsSelfLoops = true;
      features.SupportsSerializableObjectProperty = true;
      features.SupportsBooleanProperty = true;
      features.SupportsDoubleProperty = true;
      features.SupportsFloatProperty = true;
      features.SupportsIntegerProperty = true;
      features.SupportsPrimitiveArrayProperty = true;
      features.SupportsUniformListProperty = true;
      features.SupportsMixedListProperty = true;
      features.SupportsLongProperty = true;
      features.SupportsMapProperty = true;
      features.SupportsStringProperty = true;

      features.IgnoresSuppliedIds = false;
      features.IsPersistent = true;
      features.IsRdfModel = false;
      features.IsWrapper = false;

      features.SupportsIndices = false;
      features.SupportsKeyIndices = false;
      features.SupportsVertexKeyIndex = false;
      features.SupportsEdgeKeyIndex = false;
      features.SupportsVertexIndex = false;
      features.SupportsEdgeIndex = false;
      features.SupportsTransactions = true;
      features.SupportsVertexIteration = true;
      features.SupportsEdgeIteration = true;
      features.SupportsEdgeRetrieval = true;
      features.SupportsVertexProperties = true;
      features.SupportsEdgeProperties = true;
      features.SupportsThreadedTransactions = true;
    }

    /// <summary>
    /// Creates a new <see cref="Graph"/>
    /// </summary>
    /// <param name="session">The active session</param>
    /// <param name="vertexIdSetPerVertexType">Set to <see langword="false"/> if you want graph wide unique <see cref="Vertex"/> ids, by default <see langword="true"/> each <see cref="VertexType"/> maintains its own set of <see cref="Vertex"/> ids</param>
    public Graph(SessionBase session, bool vertexIdSetPerVertexType = true)
    {
      flags = 0;
      edgeTypeCt = 0;
      vertexTypeCt = 0;
      stringToVertexType = new BTreeMap<string, VertexType>(null, session);
      vertexType = new VertexType[0];
      stringToEdgeType = new BTreeMap<string, EdgeType>(null, session);
      if (vertexIdSetPerVertexType)
        flags += (UInt32) GraphFlags.VertexIdSetPerType;
      else
      {
        vertexIdToVertexType = new BTreeMap<EdgeTypeId, EdgeTypeId>(null, session);
        vertecis = new VelocityDbList<Range<VertexId>>();
      }
      edgeType = new EdgeType[0];
      propertyType = new PropertyType[0];
      this.session = session;
      NewVertexType("default");
      NewEdgeType("default", true); // not sure if we need "directed" or not as edge type parameter ???
    }

    internal VertexId AllocateVertexId(VertexId vId = 0, VelocityDbList<Range<VertexId>> vertecis = null)
    {
      if (vertecis == null)
      {
        if (this.vertecis == null)
          return vId;
        vertecis = this.vertecis;
      }
      Range<VertexId> range;
      if (vId != 0)
      {
        range = new Range<VertexId>(vId, vId);
        if (vertecis.Count == 0)
          vertecis.Add(range);
        else
        {
          bool isEqual;
          int pos = vertecis.BinarySearch(range, out isEqual);
          int originalPos = pos;
          if (isEqual)
            throw new VertexAllreadyExistException("Vertex with id " + vId + " allready exist");
          Range<VertexId> existingRange = vertecis[pos == vertecis.Count ? pos - 1 : pos];
          if (existingRange.Min == 0 && existingRange.Max == 0 || (pos > 0 && existingRange.Min > vId + 1))
            existingRange = vertecis[--pos];
          if (existingRange.Min - 1 == vId)
          {
            range = new Range<VertexId>(existingRange.Min - 1, existingRange.Max);
            if (pos > 0)
            {
              Range<VertexId> priorRange = vertecis[pos - 1];
              if (priorRange.Max + 1 == range.Min)
              {
                range = new Range<VertexId>(priorRange.Min, range.Max);
                vertecis[pos - 1] = range;
                vertecis.RemoveAt(pos);
              }
              else
                vertecis[pos] = range;
            }
            else
              vertecis[pos] = range;
          }
          else if (existingRange.Max + 1 == vId)
          {
            range = new Range<VertexId>(existingRange.Min, existingRange.Max + 1);
            if (vertecis.Count > pos)
            {
              Range<VertexId> nextRange = vertecis[pos + 1];
              if (nextRange.Min == range.Max + 1)
              {
                range = new Range<VertexId>(range.Min, nextRange.Max);
                vertecis.RemoveAt(pos);
                vertecis[pos] = range;
              }
              else
                vertecis[pos] = range;
            }
            else
              vertecis[pos == vertecis.Count ? pos - 1 : pos] = range;
          }
          else if (vId >= existingRange.Min && vId <= existingRange.Max)
          {
            throw new VertexAllreadyExistException("Vertex with id " + vId + " allready exist");
          }
          else
            vertecis.Insert(originalPos, range);
#if VERIFY
          int i = 0;
          Range<VertexId> p = default(Range<VertexId>);
          foreach (Range<VertexId> r in vertecis)
          {
            if (i++ > 0)
            {
              if (p.Min >= r.Min)
                throw new UnexpectedException("Wrong order");
              if (p.Max == r.Min + 1)
                throw new UnexpectedException("Failed to merge");
            }
            p = r;
          }
#endif
        }
      }
      else
      {
        vId = 1;
        switch (vertecis.Count)
        {
          case 0:
            range = new Range<VertexId>(1, 1);
            vertecis.Add(range);
            break;
          case 1:
            range = vertecis.First();

            if (range.Min == 1)
            {
              vId = range.Max + 1;
              range = new Range<VertexId>(1, vId);
            }
            else
            {
              vId = range.Min - 1;
              range = new Range<VertexId>(vId, range.Max);
            }
            vertecis[0] = range;
            break;
          default:
            {
              range = vertecis.First();
              if (range.Min > 1)
              {
                vId = range.Min - 1;
                range = new Range<VertexId>(vId, range.Max);
                vertecis[0] = range;
              }
              else
              {
                Range<VertexId> nextRange = vertecis[1];
                if (range.Max + 1 == nextRange.Min)
                {
                  vertecis.RemoveAt(1);
                  vId = nextRange.Min;
                  range = new Range<VertexId>(range.Min, nextRange.Max);
                  vertecis[0] = range;
                }
                else
                {
                  range = new Range<VertexId>(range.Min, range.Max + 1);
                  vId = range.Max;
                  vertecis[0] = range;
                }
              }
            }
            break;
        }
      }
      return vId;
    }

    /// <summary>
    /// Opens a graph
    /// </summary>
    /// <param name="session">session to be associated with the opened graph</param>
    /// <param name="graphInstance">if multiple graphs exist, choose which one to open</param>
    /// <returns>A graph</returns>
    public static Graph Open(SessionBase session, int graphInstance = 0)
    {
      UInt32 dbNum = session.DatabaseNumberOf(typeof(Graph));
      Database db = session.OpenDatabase(dbNum, false, false);
      if (db != null)
      {
        int ct = 0;
        foreach (Graph g in db.AllObjects<Graph>())
        {
          if (ct++ == graphInstance)
            return g;
        }
      }
      return null;
    }

    /// <summary>
    /// Add an edge to the graph. The added edges requires a recommended identifier, a tail vertex, an head vertex, and a label.
    /// Like adding a vertex, the provided object identifier may be ignored by the implementation.
    /// </summary>
    /// <param name="id">the recommended object identifier</param>
    /// <param name="outVertex">the vertex on the tail of the edge</param>
    /// <param name="inVertex">the vertex on the head of the edge</param>
    /// <param name="label">the label associated with the edge</param>
    /// <returns>the newly created edge</returns>
    public virtual IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
    {
      EdgeType et;
      if (label != null && label.Length > 0)
      {
        et = FindEdgeType(label);
        if (et == null)
          et = NewEdgeType(label, true);
      }
      else if (id is UInt64)
      {
        UInt64 fullId = (UInt64)id;
        EdgeTypeId edgeTypeId = (EdgeTypeId)(fullId >> 32);
        et = edgeType[edgeTypeId];
      }
      else
        et = edgeType[0];
      Vertex tail = outVertex as Vertex;
      Vertex head = inVertex as Vertex;
      return et.NewEdge(tail, head);
    }

    /// <summary>
    /// Create a new vertex (of VertexType "default"), add it to the graph, and return the newly created vertex.
    /// </summary>
    /// <param name="id">the recommended object identifier</param>
    /// <returns>the newly created vertex</returns>
    public virtual IVertex AddVertex(object id)
    {
      VertexType vt;
      VertexId vId = 0;
      if (id != null && id is UInt64)
      {
        UInt64 fullId = (UInt64)id;
        vId = (VertexId)id;
        VertexTypeId vertexTypeId = (VertexTypeId)(fullId >> 32);
        vt = vertexType[vertexTypeId];
      }
      else
        vt = vertexType[0];
      return vt.NewVertex(vId);
    }

    /// <summary>
    /// Return the edge referenced by the provided object identifier.
    /// If no edge is referenced by that identifier, then return null.
    /// </summary>
    /// <param name="id">the identifier of the edge to retrieved from the graph</param>
    /// <returns>the edge referenced by the provided identifier or null when no such edge exists</returns>
    public IEdge GetEdge(object id)
    {
      if (id == null)
        throw new ArgumentException("id may not be null, it should be a UInt64");
      if (id is UInt64)
      {
        UInt64 fullId = (UInt64)id;
        EdgeTypeId edgeTypeId = (EdgeTypeId)(fullId >> 32);
        EdgeType et = edgeType[edgeTypeId];
        EdgeId edgeId = (EdgeId)fullId;
        Edge edge = et.GetEdge(edgeId);
        return edge;
      }
      return null;
    }

    /// <summary>
    /// Get a <see cref="EdgeType"/> associated with an <see cref="Edge"/> id
    /// </summary>
    /// <param name="id">Id to look for</param>
    /// <returns>the associated edge type</returns>
    public EdgeType GetEdgeType(EdgeId id)
    {
      if (id < edgeType.Length)
      {
        EdgeType anEdgeType = edgeType[id];
        return anEdgeType;
      }
      throw new EdgeTypeDoesNotExistException();
    }

    /// <summary>
    /// Return an iterable to all the edges in the graph.
    /// </summary>
    /// <returns>an iterable reference to all edges in the graph</returns>
    public IEnumerable<IEdge> GetEdges()
    {
      foreach (EdgeType et in edgeType)
        foreach (IEdge edge in et.GetEdges())
          yield return edge;
    }

    /// <summary>
    /// Return an iterable to all the edges in the graph that have a particular key/value property.
    /// </summary>
    /// <param name="key">the key of the edge</param>
    /// <param name="value">the value of the edge</param>
    /// <returns>an iterable of edges with provided key and value</returns>
    public virtual IEnumerable<IEdge> GetEdges(string key, object value)
    {
      switch (Type.GetTypeCode(value.GetType()))
      {
        case TypeCode.Boolean:
          return GetEdges<bool>(key, (bool)value);
        case TypeCode.Single:
          return GetEdges<float>(key, (float)value);
        case TypeCode.Double:
          return GetEdges<double>(key, (double)value);
        default:
          return GetEdges<IComparable>(key, (IComparable)value);
      };
    }

    /// <summary>
    /// Return an iterable to all the edges in the graph that have a particular key/value property.
    /// </summary>
    /// <param name="key">the key of the edge</param>
    /// <param name="value">the value of the edge</param>
    /// <returns>an iterable of edges with provided key and value</returns>
    public virtual IEnumerable<IEdge> GetEdges<T>(string key, T value) where T : IComparable
    {
      foreach (EdgeType et in edgeType)
      {
        PropertyType pt = et.FindProperty(key);
        if (pt != null)
          foreach (IEdge edge in pt.GetPropertyEdges(value))
            yield return edge;
      }
    }

    /// <summary>
    /// Returns the features of the underlying graph.
    /// </summary>
    public virtual Features Features
    {
      get
      {
        return features;
      }
    }

    /// <inheritdoc />
    public override SessionBase Session
    {
      get
      {
        if (session != null)
          return session;
        return base.Session;
      }
    }

    /// <summary>
    /// Removes all edges and vertices from the graph.
    /// </summary>
    public void Clear()
    {
      foreach (Edge edge in GetEdges())
      {
        edge.Remove();
      }
      foreach (Vertex vertex in GetVertices())
      {
        vertex.Remove();
      }
    }


   /* public static Dictionary<Vertex, HashSet<Edge>> CombineIntersection(Dictionary<Vertex, HashSet<Edge>> objs1, Dictionary<Vertex, HashSet<Edge>> objs2)
    {
      Dictionary<Vertex, HashSet<Edge>> clone = new Dictionary<Vertex, HashSet<Edge>>();
      foreach (KeyValuePair<Vertex, HashSet<Edge>> p in objs1)
      {
        HashSet<Edge> edges;
        if (objs2.TryGetValue(p.Key, out edges))
        {
          HashSet<Edge> edgesAll = new HashSet<Edge>(edges);
          edgesAll.UnionWith(p.Value);
          clone.Add(p.Key, edgesAll);
        }
      }
      return clone;
    }*/

    /// <summary>
    /// Combines to sets of edges by intersection
    /// </summary>
    /// <param name="objs1">first set of edges</param>
    /// <param name="objs2">second set of edges</param>
    /// <returns>Intersection of sets</returns>
    public static HashSet<Edge> CombineIntersection(HashSet<Edge> objs1, HashSet<Edge> objs2)
    {
      HashSet<Edge> clone = new HashSet<Edge>(objs1);
      clone.IntersectWith(objs2);
      return clone;
    }

    /// <summary>
    /// Creates a new node type.
    /// </summary>
    /// <param name="name">Unique name for the new vertex type.</param>
    /// <param name="baseType">Base VertexType for the new VertexType.</param>
    /// <returns>Unique graph type identifier.</returns>
    public VertexType NewVertexType(string name, VertexType baseType = null)
    {
      VertexType aType;
      if (stringToVertexType.TryGetValue(name, out aType) == false)
      {
        int pos = -1;
        for (int i = 0; i < vertexType.Length; i++)
        {
          if (vertexType[i] == null)
          {
            pos = i;
            break;
          }
        }
        Update();
        if (pos < 0)
        {
          pos = vertexTypeCt;
          Array.Resize(ref vertexType, ++vertexTypeCt);
        }
        aType = new VertexType(pos, name, baseType, this);
        vertexType[pos] = aType;
        stringToVertexType.AddFast(name, aType);
      }
      return aType;
    }

    /// <summary>
    /// Creates a new edge type.
    /// </summary>
    /// <param name="name">Unique name for the new edge type.</param>
    /// <param name="biderectional">If true, this creates a biderectional edge type, otherwise this creates a unidirectional edge type.</param>
    /// <param name="baseType">Base EdgeType for the new EdgeType.</param>
    /// <returns>Unique edge type.</returns>
    /// <returns>a new edge type</returns>
    public EdgeType NewEdgeType(string name, bool biderectional, EdgeType baseType = null)
    {
      EdgeType aType;
      if (stringToEdgeType.TryGetValue(name, out aType) == false)
      {
        int pos = -1;
        for (int i = 0; i < edgeType.Length; i++)
        {
          if (edgeType[i] == null)
          {
            pos = i;
            break;
          }
        }
        Update();
        if (pos < 0)
        {
          pos = edgeTypeCt;
          Array.Resize(ref edgeType, ++edgeTypeCt);
        }
        aType = new EdgeType(pos, name, null, null, biderectional, baseType, this);
        edgeType[pos] = aType;
        stringToEdgeType.AddFast(name, aType);
      }
      return aType;
    }

    internal void RemoveEdgeTypeRef(EdgeType et)
    {
      Update();
      stringToEdgeType.Remove(et.TypeName);
      edgeType[et.TypeId] = null;
    }

    internal void RemoveVertexTypeRef(VertexType vt)
    {
      Update();
      stringToVertexType.Remove(vt.TypeName);
      vertexType[vt.TypeId] = null;
    }

    /// <summary>
    /// Creates a new edge type.
    /// </summary>
    /// <param name="name">Unique name for the new edge type.</param>
    /// <param name="biderectional">If true, this creates a biderectional edge type, otherwise this creates a unidirectional edge type.</param>
    /// <returns>Unique edge type.</returns>
    /// <param name="tailType">a fixed tail VertexType</param>
    /// <param name="headType">a fixed head VertexType</param>
    /// <param name="baseType">if specified make the new <see cref="EdgeType"/> a subclass of this <see cref="EdgeType"/></param>
    /// <returns>a new edge type</returns>
    public EdgeType NewEdgeType(string name, bool biderectional, VertexType tailType, VertexType headType, EdgeType baseType = null)
    {
      EdgeType aType;
      if (stringToEdgeType.TryGetValue(name, out aType) == false)
      {
        int pos = -1;
        for (int i = 0; i < edgeType.Length; i++)
        {
          if (edgeType[i] == null)
          {
            pos = i;
            break;
          }
        }
        Update();
        if (pos < 0)
        {
          pos = edgeTypeCt;
          Array.Resize(ref edgeType, ++edgeTypeCt);
        }
        aType = new EdgeType(pos, name, tailType, headType, biderectional, baseType, this);
        edgeType[pos] = aType;
        stringToEdgeType.AddFast(name, aType);
      }
      return aType;
    }

    /// <summary>
    /// Creates a new node instance.
    /// </summary>
    /// <param name="vertexType">Node type identifier.</param>
    /// <returns>Unique OID of the new node instance.</returns>
    public Vertex NewVertex(VertexType vertexType)
    {
      return vertexType.NewVertex();
    }

    /// <summary>
    /// Creates a new Property. 
    /// </summary>
    /// <param name="vertexType">Node or edge type identifier.</param>
    /// <param name="name">Unique name for the new Property.</param>
    /// <param name="dt">Data type for the new Property.</param>
    /// <param name="kind">Property kind.</param>
    /// <returns>Unique Property identifier.</returns>
    public PropertyType NewVertexProperty(VertexType vertexType, string name, DataType dt, PropertyKind kind)
    {
      return vertexType.NewProperty(name, dt, kind);
    }

    /// <summary>
    /// Creates a new Property. 
    /// </summary>
    /// <param name="edgeType">Node or edge type identifier.</param>
    /// <param name="name">Unique name for the new Property.</param>
    /// <param name="dt">Data type for the new Property.</param>
    /// <param name="kind">Property kind.</param>
    /// <returns>Unique Property identifier.</returns>
    public PropertyType NewEdgeProperty(EdgeType edgeType, string name, DataType dt, PropertyKind kind)
    {
      return edgeType.NewProperty(name, dt, kind);
    }

    /// <summary>
    /// Creates a new edge instance.
    /// </summary>
    /// <param name="edgeType">Edge type identifier.</param>
    /// <param name="tail">Source OID.</param>
    /// <param name="head">Target OID. </param>
    /// <returns>Unique OID of the new edge instance.</returns>
    public Edge NewEdge(EdgeType edgeType, Vertex tail, Vertex head)
    {
      return edgeType.NewEdge(tail, head);
    }

    /// <summary>
    /// Creates a new edge instance.
    /// The tail of the edge will be any node having the given tailV Value for the given tailAttr Property identifier,
    /// and the head of the edge will be any node having the given headV Value for the given headAttr Property identifier. 
    /// </summary>
    /// <param name="edgeType">Node or edge type identifier.</param>
    /// <param name="tailAttr">Property identifier.</param>
    /// <param name="tailV">Tail value</param>
    /// <param name="headAttr">Property identifier.</param>
    /// <param name="headV">Head value</param>
    /// <returns>Unique edge instance.</returns>
    public IEdge NewEdge(EdgeType edgeType, PropertyType tailAttr, object tailV, PropertyType headAttr, object headV)
    {
      return edgeType.NewEdgeX(propertyType, tailAttr, tailV, headAttr, headV, Session);
    }

    /// <summary>
    /// Selects all neighbor Vertices from or to each of the node OID in the given collection and for the given edge type.
    /// </summary>
    /// <param name="vertices">Vertex collection.</param>
    /// <param name="etype">Edge type identifier.</param>
    /// <param name="dir">Direction.</param>
    /// <returns>Dictionary of vertex keys with edges path to vertex</returns>
    public Dictionary<Vertex, HashSet<Edge>> Traverse(Vertex[] vertices, EdgeType etype, Direction dir)
    {
      Dictionary<Vertex, HashSet<Edge>> result = new Dictionary<Vertex, HashSet<Edge>>();
      foreach (Vertex vertex in vertices)
      {
        Dictionary<Vertex, HashSet<Edge>> t = vertex.Traverse(etype, dir);
        foreach (KeyValuePair<Vertex, HashSet<Edge>> p2 in t)
        {
          HashSet<Edge> edges;
          if (result.TryGetValue(p2.Key, out edges))
            foreach (Edge edge in p2.Value)
              edges.Add(edge);
          else
          {
            edges = new HashSet<Edge>();
            foreach (Edge edge in p2.Value)
              edges.Add(edge);
            result[p2.Key] = edges;
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Finds vertex having the given value for the given property. 
    /// </summary>
    /// <param name="property"></param>
    /// <param name="v">a value to look for</param>
    /// <param name="errorIfNotFound">if <c>true</c>, throw an exception if an error is found</param>
    /// <returns>the vertex matching</returns>
    public Vertex FindVertex(PropertyType property, IComparable v, bool errorIfNotFound = true)
    {
      return property.GetPropertyVertex(v, errorIfNotFound);
    }

    /// <summary>
    /// Get a count of all vertices in this graph
    /// </summary>
    /// <returns>count of vertices</returns>
    public long CountVertices()
    {
      long ct = 0;
      foreach (VertexType vt in vertexType)
        ct += vt.CountVertices();
      return ct;
    }

    /// <summary>
    /// Returns a value indicating whether a Vertex exists for the specified vertex id.
    /// </summary>
    /// <param name="vertexId">A Vertex id</param>
    /// <returns>Returns <see langword="true"/> if a Vertex exist with specified <paramref name="vertexId"/> is found in this <see cref="Graph"/>; otherwise, <see langword="false"/>.</returns>
    public bool ContainsVertex(VertexId vertexId)
    {
      if (VertexIdSetPerType)
        throw new NotSupportedException("ContainsVertex by VertexId on graph level is not supported when using VertexIdSetPerType");
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
            return true;
          if (pos > 0)
          {
            range = vertecis[--pos];
            if (range.Contains(vertexId))
              return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Returns a count of all edges in this graph
    /// </summary>
    /// <returns>the total count of edges in this graph</returns>
    public long CountEdges()
    {
      long ct = 0;
      foreach (EdgeType et in edgeType)
        ct += et.CountEdges();
      return ct;
    }

    /// <summary>
    /// A shutdown function is required to properly close the graph.
    /// This is important for implementations that utilize disk-based serializations.
    /// </summary>
    public void Shutdown()
    {
      if (Session.InTransaction)
        Session.Commit();
    }

    /// <summary>
    /// Finds the type id associated with a particular edge type. Lookup by name.
    /// </summary>
    /// <param name="name">The name of the edge/node type being looked up</param>
    /// <returns>A node/edge type id or -1 if not found.</returns>
    public EdgeType FindEdgeType(string name)
    {
      EdgeType eType;
      //if (stringToRestrictedEdgeType.TryGetValue(name, out eType))
      //  return eType.TypeId;
      if (stringToEdgeType.TryGetValue(name, out eType))
        return eType;
      return null;
    }

    /// <summary>
    /// Finds the type id associated with a particular vertexe type. Lookup by name.
    /// </summary>
    /// <param name="name">The name of the edge/node type being looked up</param>
    /// <returns>A node/edge type id or -1 if not found.</returns>
    public VertexType FindVertexType(string name)
    {
      VertexType nType;
      if (stringToVertexType.TryGetValue(name, out nType))
        return nType;
      return null;
    }

    /// <summary>
    /// Place this type of of object on its own page
    /// </summary>
    /// <returns>
    /// The default maximum number of objects per page
    /// </returns>
    public override UInt16 ObjectsPerPage
    {
      get
      {
        return 1;
      }
    }

    /// <summary>
    /// Finds a <see cref="PropertyType"/> of <see cref="VertexType"/>.
    /// </summary>
    /// <param name="vertexType">vertex type with property type</param>
    /// <param name="name">a name of a property</param>
    /// <returns></returns>
    public PropertyType FindVertexProperty(VertexType vertexType, string name)
    {
      return vertexType.FindProperty(name);
    }

    /// <summary>
    /// Generate a query object that can be used to fine tune which edges/vertices are retrieved from the graph.
    /// </summary>
    /// <returns>a graph query object with methods for constraining which data is pulled from the underlying graph</returns>
    public IQuery Query()
    {
      return new DefaultGraphQuery(this);
    }

    /// <summary>
    /// Remove the provided edge from the graph.
    /// </summary>
    /// <param name="edge">the edge to remove from the graph</param>
    public void RemoveEdge(IEdge edge)
    {
      Edge e = edge as Edge;
      e.EdgeType.RemoveEdge(e);
    }

    /// <summary>
    /// Removes a property type except if any vertex or edge is using it
    /// </summary>
    /// <param name="pt">property type to remove</param>
    public void RemovePropertyType(PropertyType pt)
    {
      if (pt.IsVertexProperty)
      {
        VertexType vt = vertexType[pt.TypeId];
        foreach (Vertex vertex in vt.GetVertices())
        {
          IComparable v = pt.GetPropertyValue(vertex.VertexId);
          if (v != null)
            throw new PropertyTypeInUseException();
        }
      }
      else
      {
        EdgeType et = edgeType[pt.TypeId];
        foreach (Edge edge in et.GetEdges())
        {
          IComparable v = pt.GetPropertyValue(edge.EdgeId);
          if (v != null)
            throw new PropertyTypeInUseException();
        }
      }
      if (pt.IsVertexProperty)
      {
        VertexType vt = vertexType[pt.TypeId];
        vt.stringToPropertyType.Remove(pt.Name);
      }
      else
      {
        EdgeType et = edgeType[pt.TypeId];
        et.stringToPropertyType.Remove(pt.Name);
      }
      Update();
      propertyType[pt.PropertyId] = null;
      pt.Unpersist(Session, true);
    }

    /// <summary>
    /// Remove the provided vertex from the graph.
    /// Upon removing the vertex, all the edges by which the vertex is connected must be removed as well.
    /// </summary>
    /// <param name="vertex">the vertex to remove from the graph</param>
    public void RemoveVertex(IVertex vertex)
    {
      Vertex v = vertex as Vertex;
      v.VertexType.RemoveVertex(v);
    }

    /// <summary>
    /// Removes a <see cref="VertexType"/> from this graph. An exception is thrown if the <see cref="VertexType"/> is in use.
    /// </summary>
    /// <param name="type">a <see cref="VertexType"/> instance</param>
    public void RemoveVertexType(VertexType type)
    {
      type.Remove();
    }

    /// <summary>
    /// Enumerates all elements satisfying the given condition for the given property and value
    /// </summary>
    /// <param name="property">Property we are looking for</param>
    /// <param name="condition">A filter function, applied with in graph value as 1st parameter and value as second parameter. </param>
    /// <param name="value">Filtering value</param>
    /// <returns>Enum of IElement</returns>
    public IEnumerable<IElement> Select(PropertyType property, Func<IComparable, IComparable, Boolean> condition, IComparable value)
    {
      if (property.IsVertexProperty)
      {
        VertexType vt = vertexType[property.TypeId];
        foreach (Vertex vertex in vt.GetVertices())
        {
          IComparable v = property.GetPropertyValue(vertex.VertexId);
          if (condition(v, value))
            yield return vertex;
        }
      }
      else
      {
        EdgeType et = edgeType[property.TypeId];
        foreach (Edge edge in et.GetEdges())
        {
          IComparable v = property.GetPropertyValue(edge.EdgeId);
          if (condition(v, value))
            yield return edge;
        }
      }
    }

    /// <summary>
    /// numerates all elements satisfying the given condition for the given property and value range
    /// </summary>
    /// <param name="property">Property we are looking for</param>
    /// <param name="condition"> filter function, applied with in graph value as 1st parameter, lower as 2nd and higher as 3rd parameter.</param>
    /// <param name="lower">lower value in filtering</param>
    /// <param name="higher">higher value in filtering</param>
    /// <returns></returns>
    public IEnumerable<IElement> Select(PropertyType property, Func<IComparable, IComparable, IComparable, Boolean> condition, IComparable lower, IComparable higher)
    {
      if (property.IsVertexProperty)
      {
        VertexType vt = vertexType[property.TypeId];
        foreach (Vertex vertex in vt.GetVertices())
        {
          IComparable v = property.GetPropertyValue(vertex.VertexId);
          if (condition(v, lower, higher))
            yield return vertex;
        }
      }
      else
      {
        EdgeType et = edgeType[property.TypeId];
        foreach (Edge edge in et.GetEdges())
        {
          IComparable v = property.GetPropertyValue(edge.EdgeId);
          if (condition(v, lower, higher))
            yield return edge;
        }
      }
    }

    /// <summary>
    /// Return the vertex referenced by the provided object identifier.
    /// If no vertex is referenced by that identifier, then return null.
    /// </summary>
    /// <param name="id">the identifier of the vertex to retrieved from the graph, must be a UInt64</param>
    /// <returns>the vertex referenced by the provided identifier or null when no such vertex exists</returns>
    public IVertex GetVertex(object id)
    {
      if (id == null)
        throw new ArgumentException("id may not be null, it should be a UInt64");
      if (id is UInt64)
      {
        UInt64 fullId = (UInt64)id;
        VertexTypeId vertexTypeId = (VertexTypeId)(fullId >> 32);
        VertexType vt = vertexType[vertexTypeId];
        VertexId vertexId = (VertexId)fullId;
        Vertex vertex = vt.GetVertex(vertexId);
        return vertex;
      }
      if (id is string)
      {
        UInt64 fullId;
        if (UInt64.TryParse(id as string, out fullId))
        {
          VertexTypeId vertexTypeId = (VertexTypeId)(fullId >> 32);
          VertexType vt = vertexType[vertexTypeId];
          VertexId vertexId = (VertexId)fullId;
          Vertex vertex = vt.GetVertex(vertexId);
          return vertex;
        }
      }
      return null;
    }

    /// <summary>
    /// Look up a <see cref="Vertex"/>
    /// </summary>
    /// <param name="id">Id of <see cref="Vertex"/> to lookup</param>
    /// <returns>A vertex with the matching id</returns>
    public Vertex GetVertex(VertexId id)
    {
      if (VertexIdSetPerType)
        throw new NotSupportedException("GetVertex by VertexId on graph level is not supported when using VertexIdSetPerType");
      VertexTypeId tId;
      if (vertexIdToVertexType.TryGetValue(id, out tId))
      {
        VertexType vt = vertexType[tId];
        return vt.GetVertex(id, false);
      }
      throw new VertexDoesNotExistException();
    }

    /// <summary>
    /// Given a <see cref="Vertex"/> id, returns the corresponding  <see cref="Vertex"/>.
    /// </summary>
    /// <param name="id">Id of a <see cref="Vertex"/></param>
    /// <returns>A <see cref="VertexType"/> with id. Throws <see cref="VertexTypeDoesNotExistException"/> if <see cref="Vertex"/> does not exist</returns>
    public VertexType GetVertexType(VertexId id)
    {
      if (VertexIdSetPerType)
        throw new NotSupportedException("GetVertexType by VertexId on graph level is not supported when using VertexIdSetPerType");
      VertexTypeId tId;
      if (vertexIdToVertexType.TryGetValue(id, out tId))
        return vertexType[tId];
      throw new VertexTypeDoesNotExistException();
    }

    /// <summary>
    /// Return an iterable to all the vertices in the graph.
    /// </summary>
    /// <returns>an iterable reference to all vertices in the graph</returns>
    public IEnumerable<IVertex> GetVertices()
    {
      foreach (VertexType vt in vertexType)
        foreach (IVertex vertex in vt.GetVertices(false))
          yield return vertex;
    }

    /// <summary>
    /// Return an iterable to all the vertices in the graph that have a particular key/value property.
    /// </summary>
    /// <param name="key">the key of vertex</param>
    /// <param name="value">the value of the vertex</param>
    /// <returns>an iterable of vertices with provided key and value</returns>
    public IEnumerable<IVertex> GetVertices(string key, object value)
    {
      foreach (VertexType vt in vertexType)
      {
        PropertyType pt = vt.FindProperty(key);
        foreach (Vertex vertex in pt.GetPropertyVertices((IComparable)value))
          yield return vertex;
      }
    }

    /// <summary>
    /// Enumerates all the edges of the given type between two given nodes (tail and head).
    /// </summary>
    /// <param name="etype">Type of Edge</param>
    /// <param name="tail">Outgoing Vertex</param>
    /// <param name="head">Incoming Vertex</param>
    /// <returns>Enumeration of Edge</returns>
    public IEnumerable<IEdge> Edges(EdgeType etype, Vertex tail, Vertex head)
    {
      VertexType vertexType = tail.VertexType;
      return vertexType.GetEdges(etype, tail, Direction.Out, head);
    }

    /// <summary>
    ///     Write the data in a Graph to a GraphSon JSON OutputStream.
    /// </summary>
    /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
    /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
    /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
    /// <param name="mode">determines the format of the GraphSON</param>
    public void ExportToGraphSon(Stream jsonOutputStream, IEnumerable<string> vertexPropertyKeys = null, IEnumerable<string> edgePropertyKeys = null, GraphSonMode mode = GraphSonMode.NORMAL)
    {
      GraphSonWriter graphJson = new GraphSonWriter(this);
      graphJson.OutputGraph(jsonOutputStream, vertexPropertyKeys, edgePropertyKeys, mode);
    }

    /// <summary>
    ///     Write the data in a Graph to a GraphSon JSON OutputStream.
    /// </summary>
    /// <param name="filename">the JSON file to write the Graph data to</param>
    /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
    /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
    /// <param name="mode">determines the format of the GraphSON</param>
    public void ExportToGraphSon(string filename, IEnumerable<string> vertexPropertyKeys = null, IEnumerable<string> edgePropertyKeys = null, GraphSonMode mode = GraphSonMode.NORMAL)
    {
      GraphSonWriter graphJson = new GraphSonWriter(this);
      graphJson.OutputGraph(filename, vertexPropertyKeys, edgePropertyKeys, mode);
    }

    /// <summary>
    /// Input the GraphSon JSON stream data into the graph.
    /// In practice, usually the provided graph is empty.
    /// </summary>
    /// <param name="filename">name of a file of JSON data</param>
    public void ImportGraphSon(string filename)
    {
      Contract.Requires(!string.IsNullOrWhiteSpace(filename));
      GraphSonReader.InputGraph(this, filename);
    }

    /// <summary>
    /// Input the GraphSon JSON stream data into the graph.
    /// In practice, usually the provided graph is empty.
    /// </summary>
    /// <param name="jsonInputStream">an Stream of JSON data</param>
    /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions</param>
    public void ImportGraphSon(Stream jsonInputStream, int bufferSize)
    {
      Contract.Requires(jsonInputStream != null);
      Contract.Requires(bufferSize > 0);
      GraphSonReader.InputGraph(this, jsonInputStream, bufferSize, null, null);
    }

    /// <summary>
    ///     Write the data in a Graph to a GraphJson JSON OutputStream.
    /// </summary>
    /// <param name="filename">the JSON file to write the Graph data to</param>
    public void ExportToGraphJson(string filename)
    {
      GraphJsonWriter.OutputGraph(this, filename, GraphJsonSettings.Default);
    }

    /// <summary>
    ///     Write the data in a Graph to a GraphJson JSON OutputStream.
    /// </summary>
    /// <param name="filename">the JSON file to write the Graph data to</param>
    /// <param name="settings">Contains field names that the writer will use to map to BluePrints</param>
    public void ExportToGraphJson(string filename, GraphJsonSettings settings)
    {
      GraphJsonWriter.OutputGraph(this, filename, settings);
    }

    /// <summary>
    ///     Write the data in a Graph to a GraphJson JSON OutputStream.
    /// </summary>
    /// <param name="jsonOutputStream">the OutputStream to write to</param>
    public void ExportToGraphJson(Stream jsonOutputStream)
    {
      GraphJsonWriter.OutputGraph(this, jsonOutputStream, GraphJsonSettings.Default);
    }

    /// <summary>
    ///     Write the data in a Graph to a GraphJson JSON OutputStream.
    /// </summary>
    /// <param name="jsonOutputStream">the OutputStream to write to</param>
    /// <param name="settings">Contains field names that the writer will use to map to BluePrints</param>
    public void ExportToGraphJson(Stream jsonOutputStream, GraphJsonSettings settings)
    {
      GraphJsonWriter.OutputGraph(this, jsonOutputStream, settings);
    }

    /// <summary>
    /// Input the GraphJson JSON stream data into the graph.
    /// In practice, usually the provided graph is empty.
    /// </summary>
    /// <param name="filename">name of a file of JSON data</param>
    public void ImportGraphJson(string filename)
    {
      Contract.Requires(!string.IsNullOrWhiteSpace(filename));
      GraphJsonReader.InputGraph(this, filename);
    }

    /// <summary>
    /// Input the GrapJson JSON stream data into the graph.
    /// In practice, usually the provided graph is empty.
    /// </summary>
    /// <param name="grapJsonInputStream">an Stream of GraphJson JSON data</param>
    /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions</param>
    public void ImportGraphJson(Stream grapJsonInputStream, int bufferSize = 1000)
    {
      Contract.Requires(grapJsonInputStream != null);
      Contract.Requires(bufferSize > 0);
      GraphJsonReader.InputGraph(this, grapJsonInputStream, bufferSize);
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return StringFactory.GraphString(this, string.Concat("vertices:", CountVertices().ToString(CultureInfo.InvariantCulture), " edges:", CountEdges().ToString(CultureInfo.InvariantCulture)));
    }

    /// <summary>
    /// Get an array of all vertex types in this graph
    /// </summary>
    /// <returns>an array of vertex types</returns>
    public VertexType[] FindVertexTypes()
    {
      return vertexType.Where(vt => vt != null).ToArray(); // null if VertexType was unpersisted;
    }

    /// <summary>
    /// Get an array of all edge types in this graph
    /// </summary>
    /// <returns>an array of edge types</returns>
    public EdgeType[] FindEdgeTypes()
    {
      return edgeType.Where(et => et != null).ToArray(); // null if EdgeType was unpersisted
    }

    /// <summary>
    /// Enumerates all the values for the given vertex/edge property
    /// </summary>
    /// <param name="property">Edge or Vertex property</param>
    /// <returns>Enumeration of property values</returns>
    public IEnumerable<IComparable> GetValues(PropertyType property)
    {
      if (property.IsVertexProperty)
      {
        foreach (VertexType vt in vertexType)
          foreach (Vertex vertex in vt.GetVertices())
          {
            IComparable v = property.GetPropertyValue(vertex.VertexId);
            if (v != null)
              yield return v;
          }
      }
      else
      {
        foreach (EdgeType et in edgeType)
          foreach (Edge edge in et.GetEdges())
          {
            IComparable v = property.GetPropertyValue(edge.EdgeId);
            if (v != null)
              yield return v;
          }
      }
    }

    internal PropertyType PropertyTypeFromDataType(bool isVertexProperty, DataType dt, TypeId typeId, int pos, string name, PropertyKind kind)
    {
      switch (dt)
      {
        case DataType.Boolean:
          return new PropertyTypeT<bool>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.Integer:
          return new PropertyTypeT<int>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.Long:
          return new PropertyTypeT<long>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.Double:
          return new PropertyTypeT<double>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.DateTime:
          return new PropertyTypeT<DateTime>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.String:
          return new PropertyTypeT<string>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.Object:
          return new PropertyTypeT<IComparable>(isVertexProperty, typeId, pos, name, kind, this);
        case DataType.IOptimizedPersistable:
          return new PropertyTypeT<IComparable>(isVertexProperty, typeId, pos, name, kind, this);
      }
      throw new UnexpectedException();
    }

    internal void DeAllocateVertexId(VertexId vId, VelocityDbList<Range<VertexId>> vertecis = null)
    {
      if (vertecis == null)
      {
        if (this.vertecis == null)
          return;
        vertecis = this.vertecis;
      }
      Range<VertexId> range = new Range<VertexId>(vId, vId);
      bool isEqual;
      int pos = vertecis.BinarySearch(range, out isEqual);
      if (pos >= 0)
      {
        if (pos == vertecis.Count || (pos > 0 && vertecis[pos].Min > vId))
          --pos;
        range = vertecis[pos];
        if (range.Min == vId)
        {
          if (range.Max == vId)
            vertecis.RemoveAt(pos);
          else
            vertecis[pos] = new Range<VertexId>(range.Min + 1, range.Max);
        }
        else if (range.Max == vId)
          vertecis[pos] = new Range<VertexId>(range.Min, range.Max + 1);
        else
        {
          vertecis[pos] = new Range<VertexId>(range.Min, vId - 1);
          vertecis.Insert(pos + 1, new Range<VertexId>(vId + 1, range.Max));
        }
      }
    }

    internal static void DeployInternalTypes(SessionBase session, string outputDirectory)
    {
      if (!Directory.Exists(outputDirectory))
        Directory.CreateDirectory(outputDirectory);
      session.DeployGenerateReaderWriter(typeof(Graph), outputDirectory);
      session.DeployGenerateReaderWriter(typeof(PropertyType), outputDirectory);
      session.DeployGenerateReaderWriter(typeof(VertexType), outputDirectory);
      session.DeployGenerateReaderWriter(typeof(EdgeType), outputDirectory);
      //session.DeployGenerateReaderWriter(typeof(BTreeBase<VertexId, VertexId>), outputDirectory);
      //session.DeployGenerateReaderWriter(typeof(BTreeSet<VertexId>), outputDirectory);
    }

    /// <inheritdoc />
    public override UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
      if (IsPersistent)
        return Id;
      session.RegisterClass(typeof(Graph));
      session.RegisterClass(typeof(BTreeMap<EdgeTypeId, EdgeTypeId>));
      session.RegisterClass(typeof(PropertyType));
      session.RegisterClass(typeof(VertexType));
      session.RegisterClass(typeof(VelocityDbList<VertexType>));
      session.RegisterClass(typeof(EdgeType));
      session.RegisterClass(typeof(UnrestrictedEdge));
      session.RegisterClass(typeof(VelocityDbList<Range<ElementId>>));
      session.RegisterClass(typeof(VelocityDbList<EdgeType>));
      session.RegisterClass(typeof(Range<VertexId>));
      session.RegisterClass(typeof(BTreeSet<Range<VertexId>>));
      session.RegisterClass(typeof(BTreeSet<EdgeType>));
      session.RegisterClass(typeof(BTreeSet<EdgeIdVertexId>));
      session.RegisterClass(typeof(BTreeMap<EdgeId, ulong>));
      session.RegisterClass(typeof(BTreeMap<EdgeId, UnrestrictedEdge>));
      session.RegisterClass(typeof(BTreeMap<string, PropertyType>));
      session.RegisterClass(typeof(BTreeMap<string, EdgeType>));
      session.RegisterClass(typeof(BTreeMap<string, VertexType>));
      session.RegisterClass(typeof(BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>));
      session.RegisterClass(typeof(BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>));
      session.RegisterClass(typeof(BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>));
      session.RegisterClass(typeof(BTreeMap<string, BTreeSet<ElementId>>));
      session.RegisterClass(typeof(BTreeMap<int, BTreeSet<ElementId>>));
      session.RegisterClass(typeof(BTreeMap<Int64, BTreeSet<ElementId>>));
      session.RegisterClass(typeof(PropertyTypeT<bool>));
      session.RegisterClass(typeof(PropertyTypeT<int>));
      session.RegisterClass(typeof(PropertyTypeT<long>));
      session.RegisterClass(typeof(PropertyTypeT<double>));
      session.RegisterClass(typeof(PropertyTypeT<DateTime>));
      session.RegisterClass(typeof(PropertyTypeT<string>));
      session.RegisterClass(typeof(PropertyTypeT<IComparable>));
      session.RegisterClass(typeof(AutoPlacement));
      return base.Persist(place, session, persistRefs, disableFlush, toPersist);
    }

    /// <inheritdoc />
    public override void Unpersist(SessionBase session, bool disableFlush = true)
    {
      if (IsPersistent == false)
        return;
      if (vertecis != null)
        vertecis.Unpersist(session, disableFlush);
      stringToVertexType.Unpersist(session, disableFlush);
      stringToEdgeType.Unpersist(session, disableFlush);
      if (vertexIdToVertexType != null)
        vertexIdToVertexType.Unpersist(session, disableFlush);
      base.Unpersist(session, disableFlush);
    }

    /// <summary>
    /// <c>true</c> if using a vertex id set per type. Decided by constructor parameter when creating a <see cref="Graph"/>
    /// </summary>
    public bool VertexIdSetPerType
    {
      get
      {
        return (flags & (UInt32) GraphFlags.VertexIdSetPerType) != 0;
      }
    }
  }
}
