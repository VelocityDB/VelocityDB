using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints
{
    /// <summary>
    ///     A CloseableIterable is required where it is necessary to deallocate resources from an IEnumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICloseableIterable<out T> : IDisposable, IEnumerable<T>
    {
    }
}