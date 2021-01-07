using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     For those graph engines that do not support the low-level querying of the edges of a vertex, then DefaultVertexQuery can be used.
    ///     DefaultVertexQuery assumes, at minimum, that Vertex.getOutEdges() and Vertex.getInEdges() is implemented by the respective Vertex.
    /// </summary>
    public class DefaultVertexQuery : DefaultQuery, IVertexQuery
    {
        private readonly IVertex _vertex;

        public DefaultVertexQuery(IVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            _vertex = vertex;
        }

        public override IEnumerable<IEdge> Edges()
        {
            return new DefaultVertexQueryIterable<IEdge>(this, false);
        }

        public override IEnumerable<IVertex> Vertices()
        {
            return new DefaultVertexQueryIterable<IVertex>(this, true);
        }

        public new IVertexQuery Direction(Direction direction)
        {
            base.Direction = direction;
            return this;
        }

        public new IVertexQuery Labels(params string[] labels)
        {
            base.Labels = labels;
            return this;
        }

        public long Count()
        {
            return Edges().LongCount();
        }

        public IEnumerable<object> VertexIds()
        {
            return Vertices().Select(vertex => vertex.Id);
        }

        private class DefaultVertexQueryIterable<T> : IEnumerable<T> where T : IElement
        {
            private readonly DefaultVertexQuery _defaultVertexQuery;
            private readonly bool _forVertex;
            private readonly IEnumerator<IEdge> _itty;
            private long _count;
            private IEdge _nextEdge;

            public DefaultVertexQueryIterable(DefaultVertexQuery defaultVertexQuery, bool forVertex)
            {
                if (defaultVertexQuery == null)
                    throw new ArgumentNullException(nameof(defaultVertexQuery));

                _defaultVertexQuery = defaultVertexQuery;
                _forVertex = forVertex;
                _itty =
                    _defaultVertexQuery._vertex.GetEdges(((DefaultQuery) _defaultVertexQuery).Direction,
                                                         ((DefaultQuery) _defaultVertexQuery).Labels).GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (LoadNext())
                {
                    var temp = _nextEdge;
                    _nextEdge = null;
                    if (_forVertex && temp != null)
                    {
                        switch (((DefaultQuery) _defaultVertexQuery).Direction)
                        {
                            case Blueprints.Direction.Out:
                                yield return (T) temp.GetVertex(Blueprints.Direction.In);
                                break;
                            case Blueprints.Direction.In:
                                yield return (T) temp.GetVertex(Blueprints.Direction.Out);
                                break;
                            default:
                                if (temp.GetVertex(Blueprints.Direction.Out).Equals(_defaultVertexQuery._vertex))
                                    yield return (T) temp.GetVertex(Blueprints.Direction.In);
                                else
                                    yield return (T) temp.GetVertex(Blueprints.Direction.Out);
                                break;
                        }
                    }
                    else
                        yield return (T) temp;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool LoadNext()
            {
                _nextEdge = null;
                if (_count >= _defaultVertexQuery.Innerlimit) return false;
                while (_itty.MoveNext())
                {
                    var edge = _itty.Current;
                    var filter = _defaultVertexQuery.HasContainers.Any(hasContainer => !hasContainer.IsLegal(edge));
                    if (!filter)
                    {
                        _nextEdge = edge;
                        _count++;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}