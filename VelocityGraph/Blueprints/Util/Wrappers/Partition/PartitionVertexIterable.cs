using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    internal class PartitionVertexIterable : ICloseableIterable<IVertex>
    {
        private readonly PartitionGraph _graph;
        private readonly IEnumerable<IVertex> _iterable;
        private bool _disposed;

        public PartitionVertexIterable(IEnumerable<IVertex> iterable, PartitionGraph graph)
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

        public IEnumerator<IVertex> GetEnumerator()
        {
            return new InnerPartitionVertexIterable(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }

        ~PartitionVertexIterable()
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

        private class InnerPartitionVertexIterable : IEnumerable<IVertex>
        {
            private readonly IEnumerator<IVertex> _itty;
            private readonly PartitionVertexIterable _partitionVertexIterable;
            private PartitionVertex _nextVertex;

            public InnerPartitionVertexIterable(PartitionVertexIterable partitionVertexIterable)
            {
                if (partitionVertexIterable == null)
                    throw new ArgumentNullException(nameof(partitionVertexIterable));

                _partitionVertexIterable = partitionVertexIterable;
                _itty = _partitionVertexIterable._iterable.GetEnumerator();
            }

            public IEnumerator<IVertex> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _nextVertex)
                    {
                        var temp = _nextVertex;
                        _nextVertex = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            var vertex = _itty.Current;
                            if (_partitionVertexIterable._graph.IsInPartition(vertex))
                                yield return new PartitionVertex(vertex, _partitionVertexIterable._graph);
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
                if (null != _nextVertex)
                    return true;

                while (_itty.MoveNext())
                {
                    var vertex = _itty.Current;
                    if (_partitionVertexIterable._graph.IsInPartition(vertex))
                    {
                        _nextVertex = new PartitionVertex(vertex, _partitionVertexIterable._graph);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}