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
    BTreeMap<ElementId, T> propertyValue;
    BTreeMap<T, BTreeSet<ElementId>> valueIndex;
    BTreeMap<T, ElementId> valueIndexUnique;

    internal PropertyTypeT(bool isVertexProp, TypeId typeId, PropertyId propertyId, string name, PropertyKind kind, Graph graph)
      : base(isVertexProp, typeId, propertyId, name, graph)
    {
      propertyValue = new BTreeMap<ElementId, T>(null, graph.Session);
      switch (kind)
      {
        case PropertyKind.Indexed:
          valueIndex = new BTreeMap<T, BTreeSet<ElementId>>(null, graph.Session);
          break;
        case PropertyKind.Unique:
          valueIndexUnique = new BTreeMap<T, ElementId>(null, graph.Session);
          break;
      }
    }

    bool GetPropertyValueT(ElementId oid, ref T pv)
    {
      if (propertyValue.TryGetValue(oid, out pv))
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
      return propertyValue.Contains(elementId);
    }     

    bool RemovePropertyValueT(ElementId oid, out T pv)
    {
      if (propertyValue.TryGetValue(oid, out pv))
      {
        propertyValue.Remove(oid);
        if (valueIndex != null)
        {
          if (valueIndex.TryGetKey(pv, ref pv))
          {
            BTreeSet<ElementId> oidArray = valueIndex[pv];
            if (oidArray.Count > 1)
              oidArray.Remove(oid);
            else
            {
              oidArray.Unpersist(Session);
              valueIndex.Remove(pv);
            }
          }
        }
        else if (valueIndexUnique != null)
          valueIndexUnique.Remove(pv);
        return true;
      }
      return false;
    }

    void SetPropertyValueX(ElementId element, T aValue)
    {
      Update();
      propertyValue[element] = aValue;
      if (valueIndex != null)
      {
        BTreeSet<ElementId> oidArray;
        if (!valueIndex.TryGetKey(aValue, ref aValue))
        {
          oidArray = new BTreeSet<ElementId>(null, graph.Session);
          oidArray.Add(element);
          valueIndex.AddFast(aValue, oidArray);
        }
        else
        {
          oidArray = valueIndex[aValue];
          oidArray.Add(element);
          valueIndex[aValue] = oidArray;
        }
      }
      else if (valueIndexUnique != null)
        valueIndexUnique.AddFast(aValue, element);
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
      if (valueIndexUnique == null || valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (valueIndex != null && valueIndex.TryGetValue(value, out elementIds))
          elementId = elementIds.First();
      }
      if (elementId == -1)
        return null;
      VertexType vertexType = graph.vertexType[TypeId];
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
      VertexType vertexType = graph.vertexType[TypeId];
      if (valueIndexUnique == null || valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (valueIndex != null && valueIndex.TryGetValue(value, out elementIds))
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
      if (valueIndexUnique == null || valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (valueIndex != null && valueIndex.TryGetValue(value, out elementIds))
          elementId = elementIds.First();
      }
      if (elementId == -1)
        return null;
      EdgeType edgeType = graph.edgeType[TypeId];
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
      EdgeType edgeType = graph.edgeType[TypeId];
      if (valueIndexUnique == null || valueIndexUnique.TryGetValue(value, out elementId) == false)
      {
        BTreeSet<ElementId> elementIds;
        if (valueIndex != null && valueIndex.TryGetValue(value, out elementIds))
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
      if (graph.VertexIdSetPerType && IsVertexProperty)
      {
        if (typeId != TypeId)
          throw new NotSupportedException("SetPropertyValue with a different VertexType/EdgeType than used by property is not supported when using VertexIdSetPerType, create Graph with option bool vertexIdSetPerVertexType set to false");
      }
      else if (typeId != TypeId)
      {
        if (IsVertexProperty)
        {
          VertexType vertexTypeIn = graph.GetVertexType(typeId);
          VertexType vertexType = graph.GetVertexType(TypeId);
          if (vertexType.subType.Contains(vertexTypeIn) == false)
            throw new UnexpectedException("Invalid VertexType used for setting property");
        }
        else
        {
          EdgeType edgeTypeIn = graph.GetEdgeType(typeId);
          EdgeType edgeType = graph.GetEdgeType(TypeId);
          if (edgeType.subType.Contains(edgeTypeIn) == false)
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
