using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     VerticesFromEdgesIterable is a helper class that returns vertices that meet the direction/label criteria of the incident edges.
    /// </summary>
    public class VerticesFromEdgesIterable : IEnumerable<IVertex>
    {
        private readonly Direction _direction;
        private readonly IEnumerable<IEdge> _iterable;
        private readonly IVertex _vertex;

        public VerticesFromEdgesIterable(IVertex vertex, Direction direction, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            _direction = direction;
            _vertex = vertex;
            _iterable = vertex.GetEdges(direction, labels);
        }

        public IEnumerator<IVertex> GetEnumerator()
        {
            foreach (var edge in _iterable)
            {
                if (_direction == Direction.Out)
                    yield return edge.GetVertex(Direction.In);
                else if (_direction == Direction.In)
                {
                    yield return edge.GetVertex(Direction.Out);
                }
                else
                {
                    if (edge.GetVertex(Direction.In).Equals(_vertex))
                        yield return edge.GetVertex(Direction.Out);
                    else
                        yield return edge.GetVertex(Direction.In);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }
    }
}