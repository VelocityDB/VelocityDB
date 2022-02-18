using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeId = System.Int32;
using EdgeId = System.Int32;
using PropertyId = System.Int32;
using VelocityGraph.Frontenac.Blueprints;
using VelocityGraph.Frontenac.Blueprints.Util;
using VelocityGraph.Exceptions;

namespace VelocityGraph
{
  /// <summary>
  /// An Edge links two vertices. Along with its key/value properties, an edge has both a directionality and a label.
  /// The directionality determines which vertex is the tail vertex (out vertex) and which vertex is the head vertex (in vertex).
  /// The edge label determines the type of relationship that exists between the two vertices.
  /// Diagrammatically, outVertex ---label---> inVertex.
  /// </summary>
  public class Edge : Element, IEdge, IEqualityComparer<Edge>
  {
    EdgeType m_edgeType;
    Vertex m_tail;
    Vertex m_head;

    internal Edge(Graph g, EdgeType eType, EdgeId eId, Vertex h, Vertex t):base(eId, g)
    {
      m_edgeType = eType;
      m_tail = t;
      m_head = h;
    }
    /// <summary>
    /// Gets the edge id
    /// </summary>
    public EdgeId EdgeId
    {
      get
      {
        return m_id;
      }
    }
    /// <summary>
    /// Gets the edge type
    /// </summary>
    public EdgeType EdgeType
    {
      get
      {
        return m_edgeType;
      }
    }

    /// <summary>
    /// Determines whether the specified <see cref="Edge"/> is equal to the current one.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(Object other)
    {
      bool isEqual = ReferenceEquals(this, other);
      if (isEqual)
        return true;
      Edge otherEdge = other as Edge;
      if (otherEdge != null)
        return EdgeId == otherEdge.EdgeId && EdgeType == otherEdge.EdgeType;
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
        UInt64 fullId = (UInt64)m_edgeType.TypeId;
        fullId <<= 32;
        fullId += (UInt64)m_id;
        return fullId;
      }
    }

    /// <summary>
    /// Return the label associated with the edge.
    /// </summary>
    /// <returns>the edge label</returns>
    string IEdge.Label
    {
      get
      {
        return m_edgeType.TypeName;
      }
    }

    /// <summary>
    /// Gets the Value for the given Property id
    /// </summary>
    /// <param name="property">Property type identifier.</param>
    /// <returns>Property value as <see cref="IComparable"/></returns>
    public IComparable GetProperty(PropertyType property)
    {
      return m_edgeType.GetPropertyValue(EdgeId, property);
    }
    
    /// <summary>
    /// Return the object value associated with the provided string key.
    /// If no value exists for that key, return null.
    /// </summary>
    /// <param name="key">the key of the key/value property</param>
    /// <returns>the object value related to the string key</returns>
    public override object GetProperty(string key)
    {
      PropertyType pt = m_edgeType.FindProperty(key);
      if (pt == null)
        return null;
      return m_edgeType.GetPropertyValue(m_id, pt);
    }

    /// <summary>
    /// Return all the keys associated with the element.
    /// </summary>
    /// <returns>the set of all string keys associated with the element</returns>
    public override IEnumerable<string> GetPropertyKeys()
    {
      foreach (string key in m_edgeType.GetPropertyKeys())
      {
        if (GetProperty(key) != null)
          yield return key;
      }
    }

    /// <summary>
    /// Return the tail/out or head/in vertex.
    /// ArgumentException is thrown if a direction of both is provided
    /// </summary>
    /// <param name="direction">whether to return the tail/out or head/in vertex</param>
    /// <returns>the tail/out or head/in vertex</returns>
    IVertex IEdge.GetVertex(Direction direction)
    {
      if (direction == Direction.In)
        return m_head;
      if (direction == Direction.Out)
        return m_tail;
      throw new ArgumentException("A direction of BOTH is not supported");
    }

    /// <summary>
    /// Gets the tail vertex
    /// </summary>
    public Vertex Tail
    {
      get
      {
        return m_tail;        
      }
    }

    /// <summary>
    /// Gets the head vertex
    /// </summary>
    public Vertex Head
    {
      get
      {
        return m_head;
      }
    }

    /// <summary>
    /// Gets the other end for the given edge.
    /// </summary>
    /// <param name="vertex">A vertex, it must be one of the ends of the edge.</param>
    /// <returns>The other end of the edge.</returns>
    public Vertex GetEdgePeer(Vertex vertex)
    {
      if (m_head == vertex)
        return m_tail;
      if (m_tail == vertex)
        return m_head;
      throw new ArgumentException("Vertex argument must be either Head or Tail Vertex");
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return m_id.GetHashCode();
    }

    /// <summary>
    /// Remove the edge from the graph.
    /// </summary>
    public override void Remove()
    {
      m_edgeType.RemoveEdge(this);
    }

    /// <summary>
    /// Un-assigns a key/value property from the edge.
    /// The object value of the removed property is returned.
    /// </summary>
    /// <param name="key">the key of the property to remove from the edge</param>
    /// <returns>the object value associated with that key prior to removal</returns>
    public override object RemoveProperty(string key)
    {
      PropertyType pt = m_edgeType.FindProperty(key);
      return pt.RemovePropertyValue(m_id);
    }
    /// <summary>
    /// Sets the value for a property
    /// </summary>
    /// <param name="property">The property to set</param>
    /// <param name="v">the value</param>
    public void SetProperty(PropertyType property, IComparable v)
    {
      if (m_edgeType != null)
        m_edgeType.SetPropertyValue(m_edgeType, EdgeId, property, v);
      else
        throw new InvalidTypeIdException();
    }    

    /// <summary>
    /// Assign a key/value property to the edge.
    /// If a value already exists for this key, then the previous key/value is overwritten.
    /// </summary>
    /// <param name="key">the string key of the property</param>
    /// <param name="value">the object value o the property</param>
    public override void SetProperty(string key, object value)
    {
      if (key == null || key.Length == 0)
        throw new ArgumentException("Property key may not be null or be an empty string");
      if (value == null)
        throw new ArgumentException("Property value may not be null");
      if (key.Equals(StringFactory.Id))
        throw ExceptionFactory.PropertyKeyIdIsReserved();
      PropertyType pt = m_edgeType.FindProperty(key, false);
      var bType = m_edgeType;
      if (pt == null)
      {
        bType = m_edgeType.m_baseType;
        while (bType != null && pt == null)
        {
          pt = bType.FindProperty(key, false);
          if (pt == null)
            bType = bType.m_baseType;
        }
        if (pt == null)
          pt = m_edgeType.MyGraph.NewEdgeProperty(m_edgeType, key, DataType.Object, PropertyKind.Indexed);
      }
      m_edgeType.SetPropertyValue(m_edgeType, EdgeId, pt, (IComparable) value);
    }

    /// <summary>
    /// Assign a key/value property to the edge.
    /// If a value already exists for this key, then the previous key/value is overwritten.
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">the string key of the property</param>
    /// <param name="value">the object value o the property</param>
    internal override void SetProperty<T>(string key, T value)
    {
      PropertyType pt = m_edgeType.FindProperty(key);
      if (pt == null)
      {
        pt = m_edgeType.MyGraph.NewEdgeProperty(m_edgeType, key, DataType.Object, PropertyKind.Indexed);
      }
      m_edgeType.SetPropertyValue(m_edgeType, EdgeId, pt, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return "Edge: " + EdgeId + " " + m_edgeType.TypeName;
    }

    /// <inheritdoc />
    public bool Equals(Edge x, Edge y)
    {
      bool isEqual = ReferenceEquals(x, y);
      if (isEqual)
        return true;
      if (y != null)
        return x.EdgeId == y.EdgeId && x.EdgeType == y.EdgeType;
      return false;
    }

    /// <inheritdoc />
    public int GetHashCode(Edge obj)
    {
      return obj.EdgeId;
    }
  }
}
