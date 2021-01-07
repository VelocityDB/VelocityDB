using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    internal class ReadOnlyVertexIterable : ICloseableIterable<IVertex>
    {
        private readonly ReadOnlyGraph _graph;
        private readonly IEnumerable<IVertex> _iterable;
        private bool _disposed;

        public ReadOnlyVertexIterable(ReadOnlyGraph graph, IEnumerable<IVertex> iterable)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (iterable == null)
                throw new ArgumentNullException(nameof(iterable));

            _graph = graph;
            _iterable = iterable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<IVertex> GetEnumerator()
        {
            return _iterable.Select(v => new ReadOnlyVertex(_graph, v)).Cast<IVertex>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }

        ~ReadOnlyVertexIterable()
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
    }
}