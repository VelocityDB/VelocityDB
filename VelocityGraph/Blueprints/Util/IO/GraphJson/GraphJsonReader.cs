using System;
using System.Collections.Generic;
using System.IO;
using Frontenac.Blueprints.Util.Wrappers.Batch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontenac.Blueprints.Util.IO.GraphJson
{
    /// <summary>
    ///     GraphJsonReader reads the data from a GraphJson JSON stream to a graph.
    /// </summary>
    public static class GraphJsonReader
    {
        /// <summary>
        ///     Input the GraphJson stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the GraphJson data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        /// /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public static void InputGraph(IGraph graph, Stream jsonInputStream, int bufferSize = 1000)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (jsonInputStream == null)
                throw new ArgumentNullException(nameof(jsonInputStream));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            InputGraph(graph, jsonInputStream, bufferSize, GraphJsonSettings.Default);
        }

        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize = 1000)
        {
            if (inputGraph == null)
                throw new ArgumentNullException(nameof(inputGraph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            using (var fis = File.Open(filename, FileMode.Open))
            {
                InputGraph(inputGraph, fis, bufferSize, GraphJsonSettings.Default);
            }
        }

        /// <summary>
        ///     Input the GraphJson stream data into the graph.
        ///     More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphJson data</param>
        /// <param name="jsonInputStream">a Stream of GraphJson data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="settings">Contains field names that the reader will use to parse the graph</param>
        public static void InputGraph(IGraph inputGraph, Stream jsonInputStream, int bufferSize, GraphJsonSettings settings)
        {
            if (inputGraph == null)
                throw new ArgumentNullException(nameof(inputGraph));
            if (jsonInputStream == null)
                throw new ArgumentNullException(nameof(jsonInputStream));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(jsonInputStream);

                using (var jp = new JsonTextReader(sr))
                {
                    sr = null;
                    // if this is a transactional graph then we're buffering
                    var graph = BatchGraph.Wrap(inputGraph, bufferSize);
                    var serializer = JsonSerializer.Create(null);

                    while (jp.Read() && jp.TokenType != JsonToken.EndObject)
                    {
                        var fieldname = Convert.ToString(jp.Value);
                        switch (fieldname)
                        {
                            case "nodes":
                                jp.Read();
                                while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                                {
                                    var props = new Dictionary<string, object>();
                                    var node = (JObject) serializer.Deserialize(jp);
                                    object id = null;
                                    foreach (var val in node)
                                    {
                                        if (val.Key == settings.IdProp)
                                            id = val.Value.ToObject<object>();
                                        else
                                            props.Add(val.Key, val.Value.ToObject<object>());
                                    }
                                    var vertex  = graph.AddVertex(id);
                                    vertex.SetProperties(props);
                                }
                                break;

                            case "edges":
                                jp.Read();
                                while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                                {
                                    var props = new Dictionary<string, object>();
                                    var node = (JObject)serializer.Deserialize(jp);
                                    object id = null;
                                    object source = null;
                                    object target = null;
                                    var caption = string.Empty;
                                    foreach (var val in node)
                                    {
                                        if (val.Key == settings.IdProp)
                                            id = val.Value.ToObject<object>();
                                        else if (val.Key == settings.EdgeCaptionProp)
                                            caption = val.Value.ToString();
                                        else if (val.Key == settings.SourceProp)
                                            source = val.Value.ToObject<object>();
                                        else if (val.Key == settings.TargetProp)
                                            target = val.Value.ToObject<object>();
                                        else
                                            props.Add(val.Key, val.Value.ToObject<object>());
                                    }
                                    if(source == null)
                                        throw new IOException("Edge has no source");
                                    if (target == null)
                                        throw new IOException("Edge has no target");
                                    var edge = graph.AddEdge(id, graph.GetVertex(source), graph.GetVertex(target), caption);
                                    edge.SetProperties(props);
                                }
                                break;
                        }
                    }

                    graph.Commit();
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }
    }
}