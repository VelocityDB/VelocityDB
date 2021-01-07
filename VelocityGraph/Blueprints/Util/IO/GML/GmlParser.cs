using System;
using System.Collections.Generic;
using System.IO;

namespace Frontenac.Blueprints.Util.IO.GML
{
    internal class GmlParser
    {
        private readonly string _defaultEdgeLabel;
        private readonly string _edgeIdKey;
        private readonly string _edgeLabelKey;
        private readonly IGraph _graph;
        private readonly string _vertexIdKey;
        private readonly IDictionary<object, object> _vertexMappedIdMap = new Dictionary<object, object>();
        private bool _directed;
        private int _edgeCount;

        public GmlParser(IGraph graph, string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                         string edgeLabelKey)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(defaultEdgeLabel))
                throw new ArgumentNullException(nameof(defaultEdgeLabel));

            _graph = graph;
            _vertexIdKey = vertexIdKey;
            _edgeIdKey = edgeIdKey;
            _edgeLabelKey = edgeLabelKey;
            _defaultEdgeLabel = defaultEdgeLabel;
        }

        public void Parse(StreamTokenizer st)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            while (HasNext(st))
            {
                var type = st.Ttype;
                if (!NotLineBreak(type)) continue;
                var value = st.StringValue;
                if (GmlTokens.Graph == value)
                {
                    ParseGraph(st);
                    if (!HasNext(st))
                        return;
                }
            }
            throw new IOException("Graph not complete");
        }

        private void ParseGraph(StreamTokenizer st)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            CheckValid(st, GmlTokens.Graph);
            while (HasNext(st))
            {
                var type = st.Ttype;
                if (!NotLineBreak(type)) continue;
                if (type == ']')
                    return;
                var key = st.StringValue;
                switch (key)
                {
                    case GmlTokens.Node:
                        AddNode(ParseNode(st));
                        break;
                    case GmlTokens.Edge:
                        AddEdge(ParseEdge(st));
                        break;
                    case GmlTokens.Directed:
                        _directed = ParseBoolean(st);
                        break;
                    default:
                        ParseValue("ignore", st);
                        break;
                }
            }
            throw new IOException("Graph not complete");
        }

        private void AddNode(IDictionary<string, object> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var id = map.JavaRemove(GmlTokens.Id);
            if (id == null)
                throw new IOException("No id found for node");
            var vertex = CreateVertex(map, id);
            AddProperties(vertex, map);
        }

        private IVertex CreateVertex(IDictionary<string, object> map, object id)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var vertexId = id;
            if (_vertexIdKey != null)
            {
                vertexId = map.JavaRemove(_vertexIdKey) ?? id;
                _vertexMappedIdMap[id] = vertexId;
            }
            var createdVertex = _graph.AddVertex(vertexId);

            return createdVertex;
        }

        private void AddEdge(IDictionary<string, object> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var source = map.JavaRemove(GmlTokens.Source);
            var target = map.JavaRemove(GmlTokens.Target);

            if (source == null)
                throw new IOException("Edge has no source");

            if (target == null)
                throw new IOException("Edge has no target");

            if (_vertexIdKey != null)
            {
                _vertexMappedIdMap.TryGetValue(source, out source);
                _vertexMappedIdMap.TryGetValue(target, out target);
            }

            var outVertex = _graph.GetVertex(source);
            var inVertex = _graph.GetVertex(target);
            if (outVertex == null)
                throw new IOException(string.Concat("Edge source ", source, " not found"));

            if (inVertex == null)
                throw new IOException(string.Concat("Edge target ", target, " not found"));

            object label = null;
            if (_edgeLabelKey != null)
                label = map.JavaRemove(_edgeLabelKey);
            if (label == null)
            {
                // try standard label key
                label = map.JavaRemove(GmlTokens.Label);
            }
            else
            {
                // remove label in case edge label key is not label
                // label is reserved and cannot be added as a property
                // if so this data will be lost
                map.Remove(GmlTokens.Label);
            }

            if (label == null)
                label = _defaultEdgeLabel;

            _edgeCount++;
            object edgeId = _edgeCount;
            if (_edgeIdKey != null)
            {
                var mappedKey = map.JavaRemove(_edgeIdKey);
                if (mappedKey != null)
                    edgeId = mappedKey;
                // else use edgecount - could fail if mapped ids overlap with edge count
            }

            // remove id as reserved property - can be left is edgeIdKey in not id
            // This data will be lost
            map.Remove(GmlTokens.Id);

            var edge = _graph.AddEdge(edgeId, outVertex, inVertex, (string)label);
            if (_directed)
                edge.SetProperty(GmlTokens.Directed, _directed);

            AddProperties(edge, map);
        }

        private static void AddProperties(IElement element, IEnumerable<KeyValuePair<string, object>> map)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            foreach (var entry in map)
                element.SetProperty(entry.Key, entry.Value);
        }

        private object ParseValue(string key, StreamTokenizer st)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (st == null)
                throw new ArgumentNullException(nameof(st));

            while (HasNext(st))
            {
                var type = st.Ttype;
                if (!NotLineBreak(type)) continue;
                if (type == StreamTokenizer.TtNumber)
                {
                    {
                        int intVal;
                        if (int.TryParse(st.StringValue, out intVal))
                            return intVal;
                    }

                    return st.NumberValue;
                }
                if (type == '[')
                    return ParseMap(key, st);
                if (type == '"')
                    return st.StringValue;
            }
            throw new IOException("value not found");
        }

        private static bool ParseBoolean(StreamTokenizer st)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            while (HasNext(st))
            {
                var type = st.Ttype;
                if (NotLineBreak(type))
                {
                    if (type == StreamTokenizer.TtNumber)
                        return st.NumberValue.CompareTo(1.0) == 0;
                }
            }
            throw new IOException("boolean not found");
        }

        private IDictionary<string, object> ParseNode(StreamTokenizer st)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            return ParseElement(st, GmlTokens.Node);
        }

        private IDictionary<string, object> ParseEdge(StreamTokenizer st)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            return ParseElement(st, GmlTokens.Edge);
        }

        private IDictionary<string, object> ParseElement(StreamTokenizer st, string node)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));
            if (string.IsNullOrWhiteSpace(node))
                throw new ArgumentNullException(nameof(node));

            CheckValid(st, node);
            return ParseMap(node, st);
        }

        private IDictionary<string, object> ParseMap(string node, StreamTokenizer st)
        {
            if (string.IsNullOrWhiteSpace(node))
                throw new ArgumentNullException(nameof(node));
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            var map = new Dictionary<string, object>();
            while (HasNext(st))
            {
                var type = st.Ttype;
                if (!NotLineBreak(type)) continue;
                if (type == ']')
                    return map;
                var key = st.StringValue;
                var value = ParseValue(key, st);
                map[key] = value;
            }
            throw new IOException(string.Concat(node, " incomplete"));
        }

        private static void CheckValid(StreamTokenizer st, string token)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (st.NextToken() != '[')
                throw new IOException(string.Concat(token, " not followed by ["));
        }

        private static bool HasNext(StreamTokenizer st)
        {
            if (st == null)
                throw new ArgumentNullException(nameof(st));

            return st.NextToken() != StreamTokenizer.TtEof;
        }

        private static bool NotLineBreak(int type)
        {
            return type != StreamTokenizer.TtEol;
        }
    }
}