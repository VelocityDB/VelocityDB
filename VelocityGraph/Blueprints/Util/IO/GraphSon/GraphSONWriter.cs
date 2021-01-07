using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    ///     GraphSONWriter writes a Graph to a TinkerPop JSON OutputStream.
    /// </summary>
    public class GraphSonWriter
    {
        private readonly IGraph _graph;

        /// <summary>
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphSonWriter(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _graph = graph;
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public void OutputGraph(string filename, IEnumerable<string> vertexPropertyKeys,
                                IEnumerable<string> edgePropertyKeys, GraphSonMode mode)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(fos, vertexPropertyKeys, edgePropertyKeys, mode);
            }
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public void OutputGraph(Stream jsonOutputStream, IEnumerable<string> vertexPropertyKeys,
                                IEnumerable<string> edgePropertyKeys, GraphSonMode mode)
        {
            if (jsonOutputStream == null)
                throw new ArgumentNullException(nameof(jsonOutputStream));

            var sw = new StreamWriter(jsonOutputStream);
            var jg = new JsonTextWriter(sw);

            var graphson = new GraphSonUtility(mode, null, vertexPropertyKeys, edgePropertyKeys);

            jg.WriteStartObject();

            jg.WritePropertyName(GraphSonTokens.Mode);
            jg.WriteValue(mode.ToString());

            jg.WritePropertyName(GraphSonTokens.Vertices);
            jg.WriteStartArray();
            foreach (var v in _graph.GetVertices())
                jg.WriteRawValue(graphson.JsonFromElement(v).ToString());

            jg.WriteEndArray();

            jg.WritePropertyName(GraphSonTokens.Edges);
            jg.WriteStartArray();
            foreach (var e in _graph.GetEdges())
                jg.WriteRawValue(graphson.JsonFromElement(e).ToString());

            jg.WriteEndArray();

            jg.WriteEndObject();
            jg.Flush();
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        ///     GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (jsonOutputStream == null)
                throw new ArgumentNullException(nameof(jsonOutputStream));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(jsonOutputStream, null, null, GraphSonMode.NORMAL);
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        ///     GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, string filename)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(filename, null, null, GraphSonMode.NORMAL);
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream, GraphSonMode mode)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (jsonOutputStream == null)
                throw new ArgumentNullException(nameof(jsonOutputStream));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(jsonOutputStream, null, null, mode);
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, string filename, GraphSonMode mode)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(filename, null, null, mode);
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream,
                                       IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                       GraphSonMode mode)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (jsonOutputStream == null)
                throw new ArgumentNullException(nameof(jsonOutputStream));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(jsonOutputStream, vertexPropertyKeys, edgePropertyKeys, mode);
        }

        /// <summary>
        ///     Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, string filename,
                                       IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                       GraphSonMode mode)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(filename, vertexPropertyKeys, edgePropertyKeys, mode);
        }
    }
}