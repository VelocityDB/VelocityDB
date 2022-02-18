using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection.BTree;
using VelocityDb.Exceptions;
using VelocityGraph.Exceptions;
using ElementId = System.Int32;
using PropertyId = System.Int32;
using TypeId = System.Int32;

namespace VelocityGraph
{
  /// <summary>
  /// Used with string property type values. Avoids storing duplicate string values.
  /// </summary>
  /// <typeparam name="T">always <see cref="string"/> for now</typeparam>
  [Serializable]
  public class PropertyTypeNoDuplicateValues<T> : PropertyTypeT<UInt64> where T : IComparable
  {
    BTreeMap<T, UInt64> m_valueToId;
    BTreeMap<UInt64, T> m_IdToValue;
    UInt64 m_nextId;

    internal PropertyTypeNoDuplicateValues(bool isVertexProp, TypeId typeId, PropertyId propertyId, string name, PropertyKind kind, Graph graph)
      : base(isVertexProp, typeId, propertyId, name, kind, graph)
    {
      m_valueToId = new BTreeMap<T, UInt64>(null, graph.GetSession());
      m_IdToValue = new BTreeMap<UInt64, T>(null, graph.GetSession());
      m_nextId = 0;
    }

    /// <inheritdoc />
    public override IComparable GetPropertyValue(ElementId element)
    {
      UInt64 id = 0;
      if (GetPropertyValueT(element, ref id))
        return m_IdToValue[id];
      return default(T);
    }

    /// <summary>
    /// Internally sets a property value
    /// </summary>
    /// <param name="element">element id</param>
    /// <param name="aValue">value</param>
    void SetPropertyValueX(ElementId element, T aValue)
    {
      UInt64 id;
      if (!m_valueToId.TryGetValue(aValue, out id))
      {
        Update();
        id = m_nextId++;
        m_IdToValue[id] = aValue;
        m_valueToId[aValue] = id;
      }
      base.SetPropertyValueX(element, id);
    }

    /// <inheritdoc />
    public override void SetPropertyValue(VertexType vt, ElementId element, TypeId typeId, IComparable value)
    {
      if (object.ReferenceEquals(value, null))
        throw new NullObjectException("A property value may not be null");
      if (MyGraph.VertexIdSetPerType && IsVertexProperty)
      {
        if (typeId != TypeId)
          throw new NotSupportedException("SetPropertyValue with a different VertexType/EdgeType than used by property is not supported when using VertexIdSetPerType, create Graph with option bool vertexIdSetPerVertexType set to false");
      }
      else if (typeId != TypeId)
      {
        if (IsVertexProperty)
        {
          VertexType vertexTypeIn = MyGraph.GetVertexType(typeId);
          VertexType vertexType = MyGraph.GetVertexType(TypeId);

          if (vertexType != vt && vertexType.SubTypes.Contains(vertexTypeIn) == false)
            throw new UnexpectedException("Invalid VertexType used for setting property");
        }
      }
      SetPropertyValueX(element, (T)value);
    }

    /// <inheritdoc />
    public override void SetPropertyValue(EdgeType et, ElementId element, TypeId typeId, IComparable value)
    {
      if (object.ReferenceEquals(value, null))
        throw new NullObjectException("A property value may not be null");
      SetPropertyValueX(element, (T)value);
    }

    /// <summary>
    /// Try to find a <see cref="Vertex"/> with a given property value.
    /// </summary>
    /// <param name="value">The property value to look for</param>
    /// <param name="polymorphic">If true, also look for property value matching vertices of property <see cref="VertexType"/> sub classes</param>
    /// <param name="errorIfNotFound">If true, signal an error if no matching <see cref="Vertex"/> found</param>
    /// <returns>A matching Vertex</returns>
    public Vertex GetPropertyVertex(T value, bool polymorphic = false, bool errorIfNotFound = true)
    {
      UInt64 id = 0;
      if (m_valueToId.TryGetValue(value, out id))
        return GetPropertyVertex(id, polymorphic, errorIfNotFound);
      return null;
    }

    /// <inheritdoc />
    public override Vertex GetPropertyVertex(IComparable value, bool polymorphic = false, bool errorIfNotFound = true)
    {
      if (IsVertexProperty == false)
        throw new InvalidTypeIdException();
      return GetPropertyVertex((T)value, polymorphic, errorIfNotFound);
    }

    /// <summary>
    /// Try to find all <see cref="Vertex"/> with a given property value.
    /// </summary>
    /// <param name="value">The property value to look for</param>
    /// <param name="polymorphic">If true, also look for property value matching vertices of property <see cref="VertexType"/> sub classes</param>
    /// <returns>Enumeration of matching vertices</returns>
    public IEnumerable<Vertex> GetPropertyVertices(T value, bool polymorphic = false)
    {
      UInt64 id = 0;
      if (m_valueToId.TryGetValue(value, out id))
        return GetPropertyVertices(id, polymorphic).ToArray();
      return Enumerable.Empty<Vertex>();
    }

    /// <inheritdoc />
    public override IEnumerable<Vertex> GetPropertyVertices(IComparable value, bool polymorphic = false)
    {
      if (IsVertexProperty == false)
        throw new InvalidTypeIdException();
      return GetPropertyVertices((T)value, polymorphic);
    }

    /// <summary>
    /// Try to find an <see cref="Edge"/> with a given property value.
    /// </summary>
    /// <param name="value">the property value to look for</param>
    /// <returns>An edge with a matching property value</returns>
    public Edge GetPropertyEdge(T value)
    {
      UInt64 id = 0;
      if (m_valueToId.TryGetValue(value, out id))
        return GetPropertyEdge(id);
      return null;
    }

    /// <inheritdoc />
    public IEnumerable<Edge> GetPropertyEdges(T value)
    {
      UInt64 id = 0;
      if (m_valueToId.TryGetValue(value, out id))
        return GetPropertyEdges(id);
      return Enumerable.Empty<Edge>();
    }

    /// <inheritdoc />
    public override IEnumerable<Edge> GetPropertyEdges(IComparable value)
    {
      if (IsVertexProperty)
        throw new InvalidTypeIdException();
      return GetPropertyEdges((T)value);
    }

    /// <inheritdoc />
    public override Edge GetPropertyEdge(IComparable value)
    {
      UInt64 id = 0;
      if (m_valueToId.TryGetValue((T) value, out id))
        return GetPropertyEdge(id);
      return null;
    }

    /// <inheritdoc />
    public override Type ValueType
    {
      get
      {
        return typeof(T);
      }
    }
  }
}
