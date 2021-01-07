using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    internal class WrappedEdgeIterable : ICloseableIterable<IEdge>
    {
        private readonly IEnumerable<IEdge> _iterable;
        private bool _disposed;

        public WrappedEdgeIterable(IEnumerable<IEdge> iterable)
        {
            if (iterable == null)
                throw new ArgumentNullException(nameof(iterable));

            _iterable = iterable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<IEdge> GetEnumerator()
        {
            return _iterable.Select(edge => new WrappedEdge(edge)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }

        ~WrappedEdgeIterable()
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