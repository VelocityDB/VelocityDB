using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using ElementId = System.Int32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using TypeId = System.Int32;
using VertexId = System.Int32;
using EdgeId = System.Int32;
using Frontenac.Blueprints;
using VelocityDb.Collection;
using VelocityDb;

namespace VelocityGraph
{
  /// <summary>
  /// Keeper of <see cref="Edge"/> and <see cref="Vertex"/> properties
  /// </summary>
  /// <typeparam name="T">The type of value used by the property type</typeparam>
  [Serializable]
  public class PropertyTypeT<T> : PropertyType where T : IComparable
  {
    BTreeMap<ElementId, T> m_propertyValue;
    BTreeMap<T, BTreeSet<ElementId>> m_valueIndex;
    BTreeMap<T, ElementId> m_valueIndexUnique;

    internal PropertyTypeT(bool isVertexProp, TypeId typeId, PropertyId propertyId, string name, PropertyKind kind, Graph graph)
      : base(isVertexProp, typeId, propertyId, name, graph)
    {
      m_propertyValue = new BTreeMap<ElementId, T>(null, graph.Session);
      switch (kind)
      {
        case PropertyKind.Indexed:
          m_valueIndex = new BTreeMap<T, BTreeSet<ElementId>>(null, graph.Session);
          break;
        case PropertyKind.Unique:
          m_valueIndexUnique = new BTreeMap<T, ElementId>(null, graph.Session);
          break;
      }
    }

    bool GetPropertyValueT(ElementId oid, ref T pv)
    {
      if (m_propertyValue.TryGetValue(oid, out pv))
        return true;
      return false;
    }    
    /// <summary>
    /// Does a certain element (Edge/Vertex) have a property value for this property type
    /// </summary>
    /// <param name="elementId">element id of element</param>
    /// <returns></returns>
    public override bool HasPropertyValue(ElementId elementId)
    {
      return m_propertyValue.Contains(elementId);
    }     

    bool RemovePropertyValueT(ElementId oid, out T pv)
    {
      if (m_propertyValue.TryGetValue(oid, out pv))
      {
        m_propertyValue.Remove(oid);
        if (m_valueIndex != null)
        {
          if (m_valueIndex.TryGetKey(pv, ref pv))
          {
            BTreeSet<ElementId> oidArray = m_valueIndex[pv];
            if (oidArray.Count > 1)
              oidArray.Remove(oid);
            else
            {
              oidArray.Unpersist(Session);
              m_valueIndex.Remove(pv);
            }
          }
        }
        else if (m_valueIndexUnique != null)
          m_valueIndexUnique.Remove(pv);
        return true;
      }
      return false;
    }

    void SetPropertyValueX(ElementId element, T aValue)
    {
      Update();
      m_propertyValue[element] = aValue;
      if (m_valueIndex != null)
      {
        BTreeSet<ElementId> oidArray;
        if (!m_valueIndex.TryGetKey(aValue, ref aValue))
        {
          oidArray = new BTreeSet<ElementId>(null, Session);
          oidArray.Add(element);
          m_valueIndex.AddFast(aValue, oidArray);
        }
        else
        {
          oidArray = m_valueIndex[aValue];
          oidArray.Add(element);
          m_valueIndex[aValue] = oidArray;
        }
      }
      else if (m_valueIndexUnique != null)
        m_valueIndexUnique.AddFast(aValue, element);
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
      VertexId elementId = -1;
      if (m_valueIndexUnique == null || m_valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (m_valueIndex != null && m_valueIndex.TryGetValue(value, out elementIds))
          elementId = elementIds.First();
      }
      if (elementId == -1)
        return null;
      VertexType vertexType = MyGraph.VertexTypes[TypeId];
      return vertexType.GetVertex(elementId, polymorphic, errorIfNotFound);
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
      VertexId elementId = -1;
      VertexType vertexType = MyGraph.VertexTypes[TypeId];
      if (m_valueIndexUnique == null || m_valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (m_valueIndex != null && m_valueIndex.TryGetValue(value, out elementIds))
          foreach (ElementId eId in elementIds)
          {
            Vertex vertex = vertexType.GetVertex(eId, polymorphic, false);
            if (vertex != null)
              yield return vertex;
          }
      }
      if (elementId != -1)
      {
        Vertex vertex = vertexType.GetVertex(elementId, polymorphic, false);
        if (vertex != null)
          yield return vertex;
      }
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
      EdgeId elementId = -1;
      if (m_valueIndexUnique == null || m_valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (m_valueIndex != null && m_valueIndex.TryGetValue(value, out elementIds))
          elementId = elementIds.First();
      }
      if (elementId == -1)
        return null;
      EdgeType edgeType = MyGraph.EdgeTypes[TypeId];
      return edgeType.GetEdge(elementId);
    }

   // public override IEnumerable<Edge> GetPropertyEdgesV<V>(V value, Graph g)
   // {
   //   return GetPropertyEdges(value, g);
   // }

     /// <inheritdoc />
    public IEnumerable<Edge> GetPropertyEdges(T value)
   {
      EdgeId elementId = -1;
      EdgeType edgeType = MyGraph.EdgeTypes[TypeId];
      if (m_valueIndexUnique == null || m_valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (m_valueIndex != null && m_valueIndex.TryGetValue(value, out elementIds))
          foreach (ElementId eId in elementIds)
            yield return edgeType.GetEdge(eId);
      }
      if (elementId != -1)     
        yield return edgeType.GetEdge(elementId);
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
      if (IsVertexProperty == false)
        throw new InvalidTypeIdException();
      return GetPropertyEdge((T)value);
    }

    /// <inheritdoc />
    public override IComparable GetPropertyValue(ElementId element)
    {
      T v = default(T);
      if (GetPropertyValueT(element, ref v))
        return v;
      return null;
    }

    /// <inheritdoc />
    public override IComparable RemovePropertyValue(ElementId element)
    {
      T pv;
      if (RemovePropertyValueT(element, out pv))
        return pv;
      return null;
    }


    /// <inheritdoc />
    public override void SetPropertyValue(ElementId element, TypeId typeId, IComparable value)
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
          if (vertexType.SubTypes.Contains(vertexTypeIn) == false)
            throw new UnexpectedException("Invalid VertexType used for setting property");
        }
        else
        {
          EdgeType edgeTypeIn = MyGraph.GetEdgeType(typeId);
          EdgeType edgeType = MyGraph.GetEdgeType(TypeId);
          if (edgeType.SubTypes.Contains(edgeTypeIn) == false)
            throw new UnexpectedException("Invalid EdgeType used for setting property");
        }
      }
      SetPropertyValueX(element, (T) value);
    }

    /// <summary>
    /// Get the value Type
    /// </summary>
    public override Type ValueType 
    {
      get
      {
        return typeof(T);
      }
    }
  }
}
