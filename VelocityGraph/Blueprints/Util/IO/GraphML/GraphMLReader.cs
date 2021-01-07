using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Frontenac.Blueprints.Util.Wrappers.Batch;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    ///     GraphMLReader writes the data from a GraphML stream to a graph.
    /// </summary>
    public class GraphMlReader
    {
        private readonly IGraph _graph;

        /// <summary>
        /// </summary>
        /// <param name="graph">the graph to populate with the GraphML data</param>
        public GraphMlReader(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _graph = graph;
        }

        /// <summary>
        ///     if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <value></value>
        public string VertexIdKey { get; set; }

        /// <summary>
        ///     if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <value></value>
        public string EdgeIdKey { get; set; }

        /// <summary>
        ///     if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <value></value>
        public string EdgeLabelKey { get; set; }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        public void InputGraph(Stream graphMlInputStream)
        {
            if (graphMlInputStream == null)
                throw new ArgumentNullException(nameof(graphMlInputStream));

            InputGraph(_graph, graphMlInputStream, 1000, VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file containing GraphML data</param>
        public void InputGraph(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            InputGraph(_graph, filename, 1000, VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(Stream graphMlInputStream, int bufferSize)
        {
            if (graphMlInputStream == null)
                throw new ArgumentNullException(nameof(graphMlInputStream));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            InputGraph(_graph, graphMlInputStream, bufferSize, VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file containing GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(string filename, int bufferSize)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            InputGraph(_graph, filename, bufferSize, VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        public static void InputGraph(IGraph inputGraph, Stream graphMlInputStream)
        {
            if (inputGraph == null)
                throw new ArgumentNullException(nameof(inputGraph));
            if (graphMlInputStream == null)
                throw new ArgumentNullException(nameof(graphMlInputStream));

            InputGraph(inputGraph, graphMlInputStream, 1000, null, null, null);
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="filename">name of a file containing GraphML data</param>
        public static void InputGraph(IGraph inputGraph, string filename)
        {
            if (inputGraph == null)
                throw new ArgumentNullException(nameof(inputGraph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            InputGraph(inputGraph, filename, 1000, null, null, null);
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="filename">name of a file containing GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize, string vertexIdKey,
                                      string edgeIdKey, string edgeLabelKey)
        {
            if (inputGraph == null)
                throw new ArgumentNullException(nameof(inputGraph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            using (var fis = File.OpenRead(filename))
            {
                InputGraph(inputGraph, fis, bufferSize, vertexIdKey, edgeIdKey, edgeLabelKey);
            }
        }

        /// <summary>
        ///     Input the GraphML stream data into the graph.
        ///     More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(IGraph inputGraph, Stream graphMlInputStream, int bufferSize, string vertexIdKey,
                                      string edgeIdKey, string edgeLabelKey)
        {
            if (inputGraph == null)
                throw new ArgumentNullException(nameof(inputGraph));
            if (graphMlInputStream == null)
                throw new ArgumentNullException(nameof(graphMlInputStream));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            using (var reader = XmlReader.Create(graphMlInputStream))
            {
                var graph = BatchGraph.Wrap(inputGraph, bufferSize);

                var keyIdMap = new Dictionary<string, string>();
                var keyTypesMaps = new Dictionary<string, string>();
                // <Mapped ID string, ID object>

                // <Default ID string, Mapped ID string>
                var vertexMappedIdMap = new Dictionary<string, string>();

                // Buffered Vertex Data
                string vertexId = null;
                Dictionary<string, object> vertexProps = null;
                bool inVertex = false;

                // Buffered Edge Data
                string edgeId = null;
                string edgeLabel = null;
                IVertex[] edgeEndVertices = null; //[0] = outVertex , [1] = inVertex
                Dictionary<string, object> edgeProps = null;
                var inEdge = false;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        var elementName = reader.Name;

                        switch (elementName)
                        {
                            case GraphMlTokens.Key:
                                {
                                    var id = reader.GetAttribute(GraphMlTokens.Id);
                                    var attributeName = reader.GetAttribute(GraphMlTokens.AttrName);
                                    var attributeType = reader.GetAttribute(GraphMlTokens.AttrType);
                                    keyIdMap[id] = attributeName;
                                    keyTypesMaps[id] = attributeType;
                                }
                                break;
                            case GraphMlTokens.Node:
                                vertexId = reader.GetAttribute(GraphMlTokens.Id);
                                if (vertexIdKey != null)
                                    vertexMappedIdMap[vertexId] = vertexId;
                                inVertex = true;
                                vertexProps = new Dictionary<string, object>();
                                break;
                            case GraphMlTokens.Edge:
                                {
                                    edgeId = reader.GetAttribute(GraphMlTokens.Id);
                                    edgeLabel = reader.GetAttribute(GraphMlTokens.Label);
                                    edgeLabel = edgeLabel ?? GraphMlTokens.Default;

                                    var vertexIds = new string[2];
                                    vertexIds[0] = reader.GetAttribute(GraphMlTokens.Source);
                                    vertexIds[1] = reader.GetAttribute(GraphMlTokens.Target);
                                    edgeEndVertices = new IVertex[2];

                                    for (var i = 0; i < 2; i++)
                                    {
                                        //i=0 => outVertex, i=1 => inVertex
                                        if (vertexIdKey == null)
                                        {
                                            edgeEndVertices[i] = graph.GetVertex(vertexIds[i]);
                                        }
                                        else
                                        {
                                            edgeEndVertices[i] = graph.GetVertex(vertexMappedIdMap.Get(vertexIds[i]));
                                        }

                                        if (null != edgeEndVertices[i]) continue;

                                        edgeEndVertices[i] = graph.AddVertex(vertexIds[i]);
                                        if (vertexIdKey != null)
                                            vertexMappedIdMap[vertexIds[i]] = vertexIds[i];
                                        // Default to standard ID system (in case no mapped  ID is found later)
                                    }

                                    inEdge = true;
                                    edgeProps = new Dictionary<string, object>();
                                }
                                break;
                            case GraphMlTokens.Data:
                                {
                                    var key = reader.GetAttribute(GraphMlTokens.Key);
                                    var attributeName = keyIdMap.Get(key);

                                    if (attributeName != null)
                                    {
                                        reader.Read();
                                        var value = reader.Value;

                                        if (inVertex)
                                        {
                                            if ((vertexIdKey != null) && (key == vertexIdKey))
                                            {
                                                // Should occur at most once per Vertex
                                                // Assumes single ID prop per Vertex
                                                vertexMappedIdMap[vertexId] = value;
                                                vertexId = value;
                                            }
                                            else if(vertexProps != null)
                                                vertexProps[attributeName] = TypeCastValue(key, value, keyTypesMaps);
                                        }
                                        else if (inEdge)
                                        {
                                            if ((edgeLabelKey != null) && (key == edgeLabelKey))
                                                edgeLabel = value;
                                            else if ((edgeIdKey != null) && (key == edgeIdKey))
                                                edgeId = value;
                                            else if(edgeProps != null)
                                                edgeProps[attributeName] = TypeCastValue(key, value, keyTypesMaps);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        var elementName = reader.Name;

                        switch (elementName)
                        {
                            case GraphMlTokens.Node:
                                if (vertexId != null)
                                {
                                    var currentVertex = graph.GetVertex(vertexId) ?? graph.AddVertex(vertexId);
                                    if (vertexProps != null)
                                    {
                                        foreach (var prop in vertexProps)
                                            currentVertex.SetProperty(prop.Key, prop.Value);
                                    }

                                    vertexId = null;
                                    vertexProps = null;
                                    inVertex = false;
                                }
                                break;
                            case GraphMlTokens.Edge:
                                if (edgeEndVertices.Length > 1)
                                {
                                    var currentEdge = graph.AddEdge(edgeId, edgeEndVertices[0], edgeEndVertices[1], edgeLabel);

                                    if(edgeProps != null)
                                    {
                                        foreach (var prop in edgeProps)
                                            currentEdge.SetProperty(prop.Key, prop.Value);
                                    }
                                    
                                    edgeId = null;
                                    edgeLabel = null;
                                    edgeEndVertices = null;
                                    edgeProps = null;
                                    inEdge = false;
                                }
                                break;
                        }
                    }
                }

                graph.Commit();
            }
        }

        private static object TypeCastValue(string key, string value, IDictionary<string, string> keyTypes)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (keyTypes == null)
                throw new ArgumentNullException(nameof(keyTypes));

            var type = keyTypes.Get(key);
            switch (type)
            {
                case GraphMlTokens.String:
                case null:
                    return value;
                case GraphMlTokens.Float:
                    return float.Parse(value, CultureInfo.InvariantCulture);
                case GraphMlTokens.Int:
                    return int.Parse(value, CultureInfo.InvariantCulture);
                case GraphMlTokens.Double:
                    return double.Parse(value, CultureInfo.InvariantCulture);
                case GraphMlTokens.Boolean:
                    return bool.Parse(value);
                case GraphMlTokens.Long:
                    return long.Parse(value, CultureInfo.InvariantCulture);
            }
            return value;
        }
    }
}