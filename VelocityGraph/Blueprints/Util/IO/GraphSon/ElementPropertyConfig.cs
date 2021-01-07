using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    ///     Configure how the GraphSON utility treats edge and vertex properties.
    /// </summary>
    public class ElementPropertyConfig
    {
        public enum ElementPropertiesRule
        {
            Include,
            Exclude
        }

        /// <summary>
        ///     A configuration that includes all properties of vertices and edges.
        /// </summary>
        public static readonly ElementPropertyConfig AllProperties =
            new ElementPropertyConfig(null, null, ElementPropertiesRule.Include, ElementPropertiesRule.Include);

        public ElementPropertyConfig(IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                     ElementPropertiesRule vertexPropertiesRule,
                                     ElementPropertiesRule edgePropertiesRule)
        {
            VertexPropertiesRule = vertexPropertiesRule;
            VertexPropertyKeys = vertexPropertyKeys;
            EdgePropertiesRule = edgePropertiesRule;
            EdgePropertyKeys = edgePropertyKeys;
        }

        public IEnumerable<string> VertexPropertyKeys { get; protected set; }

        public IEnumerable<string> EdgePropertyKeys { get; protected set; }

        public ElementPropertiesRule VertexPropertiesRule { get; protected set; }

        public ElementPropertiesRule EdgePropertiesRule { get; protected set; }

        /// <summary>
        ///     Construct a configuration that includes the specified properties from both vertices and edges.
        /// </summary>
        public static ElementPropertyConfig IncludeProperties(IEnumerable<string> vertexPropertyKeys,
                                                              IEnumerable<string> edgePropertyKeys)
        {
            return new ElementPropertyConfig(vertexPropertyKeys, edgePropertyKeys, ElementPropertiesRule.Include,
                                             ElementPropertiesRule.Include);
        }

        /// <summary>
        ///     Construct a configuration that excludes the specified properties from both vertices and edges.
        /// </summary>
        public static ElementPropertyConfig ExcludeProperties(IEnumerable<string> vertexPropertyKeys,
                                                              IEnumerable<string> edgePropertyKeys)
        {
            return new ElementPropertyConfig(vertexPropertyKeys, edgePropertyKeys, ElementPropertiesRule.Exclude,
                                             ElementPropertiesRule.Exclude);
        }
    }
}