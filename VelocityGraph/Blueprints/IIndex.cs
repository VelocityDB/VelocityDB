using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints
{
    public interface IIndex
    {
        /// <summary>
        ///     Get the name of the index.
        /// </summary>
        /// <value>the name of the index</value>
        string Name { get; }

        /// <summary>
        ///     Get the class that this index is indexing.
        /// </summary>
        /// <value>the class this index is indexing</value>
        Type Type { get; }

        /// <summary>
        ///     Index an element by a key and a value.
        /// </summary>
        /// <param name="key">the key to index the element by</param>
        /// <param name="value">the value to index the element by</param>
        /// <param name="element">the element to index</param>
        void Put(string key, object value, IElement element);

        /// <summary>
        ///     Get all elements that are indexed by the provided key/value.
        /// </summary>
        /// <param name="key">the key of the indexed elements</param>
        /// <param name="value">the value of the indexed elements</param>
        /// <returns>an IEnumerable of elements that have a particular key/value in the index</returns>
        IEnumerable<IElement> Get(string key, object value);

        /// <summary>
        ///     Get all the elements that are indexed by the provided key and specified query object.
        ///     This is useful for graph implementations that support complex query capabilities.
        ///     If querying is not supported, simply throw a NotSupportedException.
        /// </summary>
        /// <param name="key">the key of the indexed elements</param>
        /// <param name="query">the query object for the indexed elements' keys</param>
        /// <returns>an IEnumerable of elements that have a particular key/value in the index that match the query object</returns>
        IEnumerable<IElement> Query(string key, object query);

        /// <summary>
        ///     Get a count of elements with a particular key/value pair.
        ///     The semantics are the same as the get method.
        /// </summary>
        /// <param name="key">denoting the sub-index to search</param>
        /// <param name="value">the value to search for</param>
        /// <returns>the collection of elements that meet that criteria</returns>
        long Count(string key, object value);

        /// <summary>
        ///     Remove an element indexed by a particular key/value.
        /// </summary>
        /// <param name="key">the key of the indexed element</param>
        /// <param name="value">the value of the indexed element</param>
        /// <param name="element">the element to remove given the key/value pair</param>
        void Remove(string key, object value, IElement element);
    }
}