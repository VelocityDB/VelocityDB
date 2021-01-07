using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    internal class WrappedVertexIterable : ICloseableIterable<IVertex>
    {
        private readonly IEnumerable<IVertex> _iterable;
        private bool _disposed;

        public WrappedVertexIterable(IEnumerable<IVertex> iterable)
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

        public IEnumerator<IVertex> GetEnumerator()
        {
            return _iterable.Select(v => new WrappedVertex(v)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }

        ~WrappedVertexIterable()
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