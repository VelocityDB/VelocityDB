using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using ElementId = System.Int32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using TypeId = System.Int32;
using VelocityGraph.Frontenac.Blueprints;

namespace VelocityGraph
{
  /// <summary>
  /// Keeper of <see cref="Edge"/> and <see cref="Vertex"/> properties
  /// </summary>
  [Serializable]
  abstract public partial class PropertyType : OptimizedPersistable
  {
    WeakIOptimizedPersistableReference<Graph> m_graph;
    string m_propertyName;
    TypeId m_typeId;
    PropertyId m_propertyId;
    bool m_isVertexProperty;

    internal PropertyType(bool isVertexProp, TypeId typeId, PropertyId propertyId, string name, Graph graph)
    {
      m_typeId = typeId;
      m_isVertexProperty = isVertexProp;
      m_propertyId = propertyId;
      m_propertyName = name;
      m_graph = new WeakIOptimizedPersistableReference<Graph>(graph);
    }

    public Graph MyGraph
    {
      get
      {
        return m_graph.GetTarget(false, Session);
      }
    }
    /// <summary>
    /// Get id of this property type
    /// </summary>
    public PropertyId PropertyId
    {
      get
      {
        return m_propertyId;
      }
    }    
    
    /// <summary>
    /// Id of a <see cref="Vertex"/> or <see cref="Edge"/>
    /// </summary>
    public TypeId TypeId
    {
      get
      {
        return m_typeId;
      }
    }

    /// <summary>
    /// Get name of this property
    /// </summary>
    public string Name
    {
      get
      {
        return m_propertyName;
      }
    }

    /// <summary>
    /// Is this a <see cref="Vertex"/> property, if not then it is an <see cref="Edge"/> property
    /// </summary>
    public bool IsVertexProperty
    {
      get
      {
        return m_isVertexProperty;
      }
    }

    /// <summary>
    /// Get the type of property values for this property
    /// </summary>
    abstract public Type ValueType { get; }
    /// <summary>
    /// Try to find a <see cref="Vertex"/> with a given property value.
    /// </summary>
    /// <param name="value">The property value to look for</param>
    /// <param name="polymorphic">If true, also look for property value matching vertices of property <see cref="VertexType"/> sub classes</param>
    /// <param name="errorIfNotFound">If true, signal an error if no matching <see cref="Vertex"/> found</param>
    /// <returns>A matching Vertex</returns>
    abstract public Vertex GetPropertyVertex(IComparable value, bool polymorphic = false, bool errorIfNotFound = true);
    /// <summary>
    /// Try to find all <see cref="Vertex"/> with a given property value.
    /// </summary>
    /// <param name="value">The property value to look for</param>
    /// <param name="polymorphic">If true, also look for property value matching vertices of property <see cref="VertexType"/> sub classes</param>
    /// <returns>Enumeration of matching vertices</returns>
    abstract public IEnumerable<Vertex> GetPropertyVertices(IComparable value, bool polymorphic = false);
    /// <summary>
    /// Try to find an <see cref="Edge"/> with a given property value.
    /// </summary>
    /// <param name="value">the property value to look for</param>
    /// <returns>An edge with a matching property value</returns>
    abstract public Edge GetPropertyEdge(IComparable value);
    /// <summary>
    /// Try to find all <see cref="Edge"/> with a given property value.
    /// </summary>
    /// <param name="value">the property value to look for</param>
    /// <returns>An edge with a matching property value</returns>
    abstract public IEnumerable<Edge> GetPropertyEdges(IComparable value);
    /// <summary>
    /// Get property value of a Vertex/Edge
    /// </summary>
    /// <param name="elementId">Id of a Vertex/Edge</param>
    /// <returns>the property value</returns>
    abstract public IComparable GetPropertyValue(ElementId elementId);
    /// <summary>
    /// Check if an element has a property value
    /// </summary>
    /// <param name="elementId">Id of a Vertex/Edge</param>
    /// <returns><c>true</c> if element has a property value for this property type</returns>
    abstract public bool HasPropertyValue(ElementId elementId);
    /// <summary>
    /// Sets a property value for an element
    /// </summary>
    /// <param name="elementId">Id of a vertex or edge</param>
    /// <param name="typeId">Id of vertex/edge</param>
    /// <param name="value">Value to assign to property</param>
    abstract public void SetPropertyValue(ElementId elementId, TypeId typeId, IComparable value);
    /// <summary>
    /// Remove a property value
    /// </summary>
    /// <param name="elementId">Id of an edge/vertex</param>
    /// <returns>the value that was assigned prior to removing the property value</returns>
    abstract public IComparable RemovePropertyValue(ElementId elementId);
  }
}
