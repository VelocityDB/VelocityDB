using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Frontenac.Blueprints.Util.IO.GraphJson
{
    /// <summary>
    ///     GraphJsonWriter writes a Graph to a GraphJson OutputStream.
    /// </summary>
    public static class GraphJsonWriter
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

        /// <summary>
        ///     Write the data in a Graph to a GraphJson OutputStream.
        /// </summary>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="graph">the graph to serialize</param>
        public static void OutputGraph(IGraph graph, string filename)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            OutputGraph(graph, filename, GraphJsonSettings.Default);
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphJson OutputStream.
        /// </summary>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="graph">the graph to serialize</param>
        /// <param name="settings">Contains field names that the writer will use to map to BluePrints</param>
        public static void OutputGraph(IGraph graph, string filename, GraphJsonSettings settings)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(graph, fos, settings);
            }
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphJson OutputStream.
        /// </summary>
        /// <param name="jsonOutputStream">the OutputStream to write to</param>
        /// <param name="graph">the graph to serialize</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (jsonOutputStream == null)
                throw new ArgumentNullException(nameof(jsonOutputStream));

            OutputGraph(graph, jsonOutputStream, GraphJsonSettings.Default);
        }

        /// <summary>
        ///     Write the data in a Graph to a GraphJson OutputStream.
        /// </summary>
        /// <param name="jsonOutputStream">the OutputStream to write to</param>
        /// <param name="graph">the graph to serialize</param>
        /// <param name="settings">Contains field names that the writer will use to map to BluePrints</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream, GraphJsonSettings settings)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (jsonOutputStream == null)
                throw new ArgumentNullException(nameof(jsonOutputStream));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var sw = new StreamWriter(jsonOutputStream);
            var jg = new JsonTextWriter(sw);


            jg.WriteStartObject();

            jg.WritePropertyName("nodes");
            jg.WriteStartArray();
            foreach (var v in graph.GetVertices())
                jg.WriteRawValue(JsonFromElement(v, settings).ToString());
            jg.WriteEndArray();

            jg.WritePropertyName("edges");
            jg.WriteStartArray();
            foreach (var e in graph.GetEdges())
                jg.WriteRawValue(JsonFromElement(e, settings).ToString());

            jg.WriteEndArray();

            jg.WriteEndObject();
            jg.Flush();
        }

        /// <summary>
        ///     Creates GraphJson for a single graph element.
        /// </summary>
        private static JObject JsonFromElement(IElement element, GraphJsonSettings settings)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var isEdge = element is IEdge;
            var map = element.ToDictionary(t => t.Key, t => t.Value);
                
            if (isEdge)
            {
                var edge = element as IEdge;
                var source = edge.GetVertex(Direction.In).Id;
                var target = edge.GetVertex(Direction.Out).Id;
                var caption = edge.Label;

                map.Add(settings.SourceProp, source);
                map.Add(settings.TargetProp, target);
                map.Add(settings.EdgeCaptionProp, caption);
            }
            else
                map.Add(settings.IdProp, element.Id);

            return JObject.FromObject(map, Serializer);
        }
    }
}