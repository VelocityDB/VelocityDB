using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    ///     GraphMLWriter writes a Graph to a GraphML OutputStream.
    /// </summary>
    public class GraphMlWriter
    {
        private const string W3CXmlSchemaInstanceNsUri = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly IGraph _graph;
        private Dictionary<string, string> _edgeKeyTypes;
        private string _edgeLabelKey;
        private bool _normalize;
        private Dictionary<string, string> _vertexKeyTypes;

        private string _xmlSchemaLocation;

        /// <summary>
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphMlWriter(IGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        ///     the location of the GraphML XML Schema instance
        /// </summary>
        /// <param name="xmlSchemaLocation"></param>
        public void SetXmlSchemaLocation(string xmlSchemaLocation)
        {
            _xmlSchemaLocation = xmlSchemaLocation;
        }

        /// <summary>
        ///     Set the name of the edge label in the GraphML. When this value is not set the value of the Edge.getLabel()
        ///     is written as a "label" attribute on the edge element.  This does not validate against the GraphML schema.
        ///     If this value is set then the the value of Edge.getLabel() is written as a data element on the edge and
        ///     the appropriate key element is added to define it in the GraphML
        /// </summary>
        /// <param name="edgeLabelKey">if the label of an edge will be handled by the data property.</param>
        public void SetEdgeLabelKey(string edgeLabelKey)
        {
            _edgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        ///     whether to normalize the output. Normalized output is deterministic with respect to the order of
        ///     elements and properties in the resulting XML document, and is compatible with line diff-based tools
        ///     such as Git. Note: normalized output is memory-intensive and is not appropriate for very large graphs.
        /// </summary>
        /// <param name="normalize"></param>
        public void SetNormalize(bool normalize)
        {
            _normalize = normalize;
        }

        /// <summary>
        ///     a IDictionary&lt;string, string> of the data types of the vertex keys
        /// </summary>
        /// <param name="vertexKeyTypes"></param>
        public void SetVertexKeyTypes(Dictionary<string, string> vertexKeyTypes)
        {
            _vertexKeyTypes = vertexKeyTypes;
        }

        /// <summary>
        ///     a IDictionary&lt;string, string> of the data types of the edge keys
        /// </summary>
        /// <param name="edgeKeyTypes"></param>
        public void SetEdgeKeyTypes(Dictionary<string, string> edgeKeyTypes)
        {
            _edgeKeyTypes = edgeKeyTypes;
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        public void OutputGraph(string filename)
        {
            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(fos);
            }
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graphMlOutputStream">the GraphML OutputStream to write the Graph data to</param>
        public void OutputGraph(Stream graphMlOutputStream)
        {
            if (null == _vertexKeyTypes || null == _edgeKeyTypes)
            {
                var vertexKeyTypes = new Dictionary<string, string>();
                var edgeKeyTypes = new Dictionary<string, string>();

                foreach (var vertex in _graph.GetVertices())
                {
                    foreach (var key in vertex.GetPropertyKeys())
                    {
                        if (!vertexKeyTypes.ContainsKey(key))
                            vertexKeyTypes[key] = GetStringType(vertex.GetProperty(key));
                    }
                    foreach (var edge in vertex.GetEdges(Direction.Out))
                    {
                        foreach (var key in edge.GetPropertyKeys())
                        {
                            if (!edgeKeyTypes.ContainsKey(key))
                                edgeKeyTypes[key] = GetStringType(edge.GetProperty(key));
                        }
                    }
                }

                if (null == _vertexKeyTypes)
                    _vertexKeyTypes = vertexKeyTypes;

                if (null == _edgeKeyTypes)
                    _edgeKeyTypes = edgeKeyTypes;
            }

            // adding the edge label key will push the label into the data portion of the graphml otherwise it
            // will live with the edge data itself (which won't validate against the graphml schema)
            if (null != _edgeLabelKey && null != _edgeKeyTypes && null == _edgeKeyTypes.Get(_edgeLabelKey))
                _edgeKeyTypes[_edgeLabelKey] = GraphMlTokens.String;

            var settings = new XmlWriterSettings
                {
                    Indent = _normalize,
                    IndentChars = "\t",
                    Encoding = new UTF8Encoding(false),
                    OmitXmlDeclaration = true
                };

            using (var writer = XmlWriter.Create(graphMlOutputStream, settings))
            {
                var xmlDeclaration = string.Concat("<?xml version=\"1.0\"?>",
                                                   _normalize ? Environment.NewLine : string.Empty);
                var xmlDeclarationData = settings.Encoding.GetBytes(xmlDeclaration);
                graphMlOutputStream.Write(xmlDeclarationData, 0, xmlDeclarationData.Length);
                writer.WriteStartDocument();
                writer.WriteStartElement(GraphMlTokens.Graphml, GraphMlTokens.GraphmlXmlns);
                writer.WriteAttributeString(GraphMlTokens.Xmlns, string.Empty, GraphMlTokens.GraphmlXmlns);

                //XML Schema instance namespace definition (xsi)
                writer.WriteAttributeString(GraphMlTokens.Xmlns, GraphMlTokens.XmlSchemaNamespaceTag, string.Empty,
                                            W3CXmlSchemaInstanceNsUri);

                //XML Schema location
                writer.WriteAttributeString(GraphMlTokens.XmlSchemaNamespaceTag,
// ReSharper disable AssignNullToNotNullAttribute
                                            GraphMlTokens.XmlSchemaLocationAttribute, null,
// ReSharper restore AssignNullToNotNullAttribute
                                            string.Concat(GraphMlTokens.GraphmlXmlns, " ",
                                                          _xmlSchemaLocation ??
                                                          GraphMlTokens.DefaultGraphmlSchemaLocation));

                // <key id="weight" for="edge" attr.name="weight" attr.type="float"/>
                IEnumerable<string> keyset;

                if (_normalize)
                {
                    var sortedKeyset = new List<string>(_vertexKeyTypes.Keys);
                    sortedKeyset.Sort();
                    keyset = sortedKeyset;
                }
                else
                    keyset = _vertexKeyTypes.Keys.ToList();

                foreach (var key in keyset)
                {
                    writer.WriteStartElement(GraphMlTokens.Key);
                    writer.WriteAttributeString(GraphMlTokens.Id, key);
                    writer.WriteAttributeString(GraphMlTokens.For, GraphMlTokens.Node);
                    writer.WriteAttributeString(GraphMlTokens.AttrName, key);
                    writer.WriteAttributeString(GraphMlTokens.AttrType, _vertexKeyTypes.Get(key));
                    writer.WriteFullEndElement();
                }

                if (_normalize)
                {
                    var sortedKeyset = new List<string>(_edgeKeyTypes.Keys);
                    sortedKeyset.Sort();
                    keyset = sortedKeyset;
                }
                else
                    keyset = _edgeKeyTypes.Keys;

                foreach (var key in keyset)
                {
                    writer.WriteStartElement(GraphMlTokens.Key);
                    writer.WriteAttributeString(GraphMlTokens.Id, key);
                    writer.WriteAttributeString(GraphMlTokens.For, GraphMlTokens.Edge);
                    writer.WriteAttributeString(GraphMlTokens.AttrName, key);
                    writer.WriteAttributeString(GraphMlTokens.AttrType, _edgeKeyTypes.Get(key));
                    writer.WriteFullEndElement();
                }

                writer.WriteStartElement(GraphMlTokens.Graph);
                writer.WriteAttributeString(GraphMlTokens.Id, GraphMlTokens.G);
                writer.WriteAttributeString(GraphMlTokens.Edgedefault, GraphMlTokens.Directed);

                IEnumerable<IVertex> vertices;
                if (_normalize)
                {
                    var sortedVertices = new List<IVertex>(_graph.GetVertices());
                    sortedVertices.Sort(new LexicographicalElementComparator());
                    vertices = sortedVertices;
                }
                else
                    vertices = _graph.GetVertices();

                foreach (var vertex in vertices)
                {
                    writer.WriteStartElement(GraphMlTokens.Node);
                    writer.WriteAttributeString(GraphMlTokens.Id, vertex.Id.ToString());
                    IEnumerable<string> keys;
                    if (_normalize)
                    {
                        var sortedKeys = new List<string>(vertex.GetPropertyKeys());
                        sortedKeys.Sort();
                        keys = sortedKeys;
                    }
                    else
                        keys = vertex.GetPropertyKeys();

                    foreach (var key in keys)
                    {
                        writer.WriteStartElement(GraphMlTokens.Data);
                        writer.WriteAttributeString(GraphMlTokens.Key, key);
                        var value = vertex.GetProperty(key);
                        if (null != value)
                            writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (_normalize)
                {
                    var edges = new List<IEdge>();
                    foreach (var vertex in _graph.GetVertices())
                        edges.AddRange(vertex.GetEdges(Direction.Out));
                    edges.Sort(new LexicographicalElementComparator());

                    foreach (var edge in edges)
                    {
                        writer.WriteStartElement(GraphMlTokens.Edge);
                        writer.WriteAttributeString(GraphMlTokens.Id, edge.Id.ToString());
                        writer.WriteAttributeString(GraphMlTokens.Source, edge.GetVertex(Direction.Out).Id.ToString());
                        writer.WriteAttributeString(GraphMlTokens.Target, edge.GetVertex(Direction.In).Id.ToString());

                        if (_edgeLabelKey == null)
                        {
                            // this will not comply with the graphml schema but is here so that the label is not
                            // mixed up with properties.
                            writer.WriteAttributeString(GraphMlTokens.Label, edge.Label);
                        }
                        else
                        {
                            writer.WriteStartElement(GraphMlTokens.Data);
                            writer.WriteAttributeString(GraphMlTokens.Key, _edgeLabelKey);
                            writer.WriteString(edge.Label);
                            writer.WriteEndElement();
                        }

                        var keys = new List<string>(edge.GetPropertyKeys());
                        keys.Sort();

                        foreach (var key in keys)
                        {
                            writer.WriteStartElement(GraphMlTokens.Data);
                            writer.WriteAttributeString(GraphMlTokens.Key, key);
                            var value = edge.GetProperty(key);
                            if (null != value)
                                writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }
                else
                {
                    foreach (var vertex in _graph.GetVertices())
                    {
                        foreach (var edge in vertex.GetEdges(Direction.Out))
                        {
                            writer.WriteStartElement(GraphMlTokens.Edge);
                            writer.WriteAttributeString(GraphMlTokens.Id, edge.Id.ToString());
                            writer.WriteAttributeString(GraphMlTokens.Source,
                                                        edge.GetVertex(Direction.Out).Id.ToString());
                            writer.WriteAttributeString(GraphMlTokens.Target, edge.GetVertex(Direction.In).Id.ToString());
                            writer.WriteAttributeString(GraphMlTokens.Label, edge.Label);

                            foreach (var key in edge.GetPropertyKeys())
                            {
                                writer.WriteStartElement(GraphMlTokens.Data);
                                writer.WriteAttributeString(GraphMlTokens.Key, key);
                                var value = edge.GetProperty(key);
                                if (null != value)
                                    writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                    }
                }

                writer.WriteEndElement(); // graph
                writer.WriteEndElement(); // graphml
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMlOutputStream">the GraphML OutputStream to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, Stream graphMlOutputStream)
        {
            var writer = new GraphMlWriter(graph);
            writer.OutputGraph(graphMlOutputStream);
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        public static void OutputGraph(IGraph graph, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new GraphMlWriter(graph);
            writer.OutputGraph(filename);
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        /// <param name="vertexKeyTypes">a IDictionary&lt;string, string> of the data types of the vertex keys</param>
        /// <param name="edgeKeyTypes">a IDictionary&lt;string, string> of the data types of the edge keys</param>
        public static void OutputGraph(IGraph graph, string filename, Dictionary<string, string> vertexKeyTypes,
                                       Dictionary<string, string> edgeKeyTypes)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new GraphMlWriter(graph);
            writer.SetVertexKeyTypes(vertexKeyTypes);
            writer.SetEdgeKeyTypes(edgeKeyTypes);
            writer.OutputGraph(filename);
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMlOutputStream">the GraphML OutputStream to write the Graph data to</param>
        /// <param name="vertexKeyTypes">a IDictionary&lt;string, string> of the data types of the vertex keys</param>
        /// <param name="edgeKeyTypes">a IDictionary&lt;string, string> of the data types of the edge keys</param>
        public static void OutputGraph(IGraph graph, Stream graphMlOutputStream,
                                       Dictionary<string, string> vertexKeyTypes,
                                       Dictionary<string, string> edgeKeyTypes)
        {
            var writer = new GraphMlWriter(graph);
            writer.SetVertexKeyTypes(vertexKeyTypes);
            writer.SetEdgeKeyTypes(edgeKeyTypes);
            writer.OutputGraph(graphMlOutputStream);
        }

        private static string GetStringType(object object_)
        {
            if (object_ is string)
                return GraphMlTokens.String;
            if (object_ is int)
                return GraphMlTokens.Int;
            if (object_ is long)
                return GraphMlTokens.Long;
            if (object_ is float)
                return GraphMlTokens.Float;
            if (object_ is double)
                return GraphMlTokens.Double;
            if (object_ is bool)
                return GraphMlTokens.Boolean;
            return GraphMlTokens.String;
        }
    }
}