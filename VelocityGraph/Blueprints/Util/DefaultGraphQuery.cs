using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     For those graph engines that do not support the low-level querying of the vertices or edges, then DefaultQuery can be used.
    ///     DefaultQuery assumes, at minimum, that Graph.getVertices() and Graph.getEdges() is implemented by the respective Graph.
    /// </summary>
    public class DefaultGraphQuery : DefaultQuery
    {
        private readonly IGraph _graph;

        public DefaultGraphQuery(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _graph = graph;
        }

        public override IEnumerable<IEdge> Edges()
        {
            return new DefaultGraphQueryIterable<IEdge>(this, GetElementIterable<IEdge>(typeof (IEdge)));
        }

        public override IEnumerable<IVertex> Vertices()
        {
            return new DefaultGraphQueryIterable<IVertex>(this, GetElementIterable<IVertex>(typeof (IVertex)));
        }

        private IEnumerable<T> GetElementIterable<T>(Type elementClass) where T : IElement
        {
            if (elementClass == null)
                throw new ArgumentNullException(nameof(elementClass));

            if (_graph is IKeyIndexableGraph)
            {
                var keys = (_graph as IKeyIndexableGraph).GetIndexedKeys(elementClass).ToArray();
                foreach (
                    var hasContainer in
                        HasContainers.Where(
                            hasContainer =>
                            hasContainer.Compare == Compare.Equal && hasContainer.Value != null &&
                            keys.Contains(hasContainer.Key)))
                {
                    if (typeof (IVertex).IsAssignableFrom(elementClass))
                        return (IEnumerable<T>) _graph.GetVertices(hasContainer.Key, hasContainer.Value);
                    return (IEnumerable<T>) _graph.GetEdges(hasContainer.Key, hasContainer.Value);
                }
            }

            foreach (var hasContainer in HasContainers.Where(hasContainer => hasContainer.Compare == Compare.Equal))
            {
                if (typeof (IVertex).IsAssignableFrom(elementClass))
                    return (IEnumerable<T>) _graph.GetVertices(hasContainer.Key, hasContainer.Value);
                return (IEnumerable<T>) _graph.GetEdges(hasContainer.Key, hasContainer.Value);
            }

            return typeof (IVertex).IsAssignableFrom(elementClass)
                       ? (IEnumerable<T>) _graph.GetVertices()
                       : (IEnumerable<T>) _graph.GetEdges();
        }

        private class DefaultGraphQueryIterable<T> : IEnumerable<T> where T : IElement
        {
            private readonly DefaultGraphQuery _defaultQuery;
            private readonly IEnumerable<T> _iterable;
            private long _count;
            private T _nextElement;

            public DefaultGraphQueryIterable(DefaultGraphQuery defaultQuery, IEnumerable<T> iterable)
            {
                if (defaultQuery == null)
                    throw new ArgumentNullException(nameof(defaultQuery));
                if (iterable == null)
                    throw new ArgumentNullException(nameof(iterable));

                _defaultQuery = defaultQuery;
                _iterable = iterable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (LoadNext()) yield return _nextElement;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool LoadNext()
            {
                _nextElement = default(T);
                if (_count >= _defaultQuery.Innerlimit)
                    return false;

                foreach (var element in _iterable)
                {
                    var filter = _defaultQuery.HasContainers.Any(hasContainer => !hasContainer.IsLegal(element));

                    if (filter) continue;
                    _nextElement = element;
                    _count++;
                    return true;
                }
                return false;
            }
        }
    }
}