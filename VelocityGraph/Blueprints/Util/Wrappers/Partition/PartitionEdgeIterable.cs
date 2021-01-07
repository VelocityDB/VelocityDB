using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    internal class PartitionEdgeIterable : ICloseableIterable<IEdge>
    {
        private readonly PartitionGraph _graph;
        private readonly IEnumerable<IEdge> _iterable;
        private bool _disposed;

        public PartitionEdgeIterable(IEnumerable<IEdge> iterable, PartitionGraph graph)
        {
            if (iterable == null)
                throw new ArgumentNullException(nameof(iterable));
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _iterable = iterable;
            _graph = graph;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<IEdge> GetEnumerator()
        {
            return new InnerPartitionEdgeIterable(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }

        ~PartitionEdgeIterable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_iterable is IDisposable)
                    (_iterable as IDisposable).Dispose();
            }

            _disposed = true;
        }

        private class InnerPartitionEdgeIterable : IEnumerable<IEdge>
        {
            private readonly IEnumerator<IEdge> _itty;
            private readonly PartitionEdgeIterable _partitionEdgeIterable;
            private PartitionEdge _nextEdge;

            public InnerPartitionEdgeIterable(PartitionEdgeIterable partitionEdgeIterable)
            {
                if (partitionEdgeIterable == null)
                    throw new ArgumentNullException(nameof(partitionEdgeIterable));

                _partitionEdgeIterable = partitionEdgeIterable;
                _itty = _partitionEdgeIterable._iterable.GetEnumerator();
            }

            public IEnumerator<IEdge> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _nextEdge)
                    {
                        var temp = _nextEdge;
                        _nextEdge = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            var edge = _itty.Current;
                            if (_partitionEdgeIterable._graph.IsInPartition(edge))
                                yield return new PartitionEdge(edge, _partitionEdgeIterable._graph);
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool HasNext()
            {
                if (null != _nextEdge)
                    return true;

                while (_itty.MoveNext())
                {
                    var edge = _itty.Current;
                    if (_partitionEdgeIterable._graph.IsInPartition(edge))
                    {
                        _nextEdge = new PartitionEdge(edge, _partitionEdgeIterable._graph);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}