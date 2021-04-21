using VelocityGraph.Frontenac.Blueprints;
using VelocityGraph.Frontenac.Blueprints.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElementId = System.Int32;

namespace VelocityGraph
{
  /// <summary>
  /// Base class for Edge and Vertex
  /// </summary>
  public abstract class Element : DictionaryElement
  {
    /// <summary>
    /// The id of an element
    /// </summary>
    protected readonly ElementId m_id;

    /// <summary>
    /// Constructor setting the id and graph reference
    /// </summary>
    /// <param name="id">Assigned id</param>
    /// <param name="graph">The owning graph</param>
    protected Element(ElementId id, Graph graph):base(graph)
    {
      m_id = id;
    }

    /// <summary>
    /// Access to owning graph
    /// </summary>
    protected new Graph Graph
    {
      get
      {
        return (Graph)base.Graph;
      }
    }

    /// <summary>
    /// Assign a key/value property to the element.
    /// If a value already exists for this key, then the previous key/value is overwritten.
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">the string key of the property</param>
    /// <param name="value">the T value o the property</param>
    internal abstract void SetProperty<T>(string key, T value) where T : IComparable;

    /// <summary>
    /// Use id as hash code
    /// </summary>
    /// <returns>The hash code given by id</returns>
    public override int GetHashCode()
    {
      return m_id.GetHashCode();
    }
  }
}
