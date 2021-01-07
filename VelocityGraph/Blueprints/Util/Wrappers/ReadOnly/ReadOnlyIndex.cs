using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndex : IIndex
    {
        private readonly ReadOnlyGraph _graph;
        protected IIndex RawIndex;

        public ReadOnlyIndex(ReadOnlyGraph graph, IIndex rawIndex)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (rawIndex == null)
                throw new ArgumentNullException(nameof(rawIndex));

            _graph = graph;
            RawIndex = rawIndex;
        }

        public void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public void Put(string key, object value, IElement element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            IndexContract.ValidateGet(key, value);

            if (typeof (IVertex).IsAssignableFrom(Type))
                return new ReadOnlyVertexIterable(_graph, (IEnumerable<IVertex>)RawIndex.Get(key, value));
            return new ReadOnlyEdgeIterable(_graph, (IEnumerable<IEdge>) RawIndex.Get(key, value));
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new ReadOnlyVertexIterable(_graph, (IEnumerable<IVertex>) RawIndex.Query(key, value));
            return new ReadOnlyEdgeIterable(_graph, (IEnumerable<IEdge>) RawIndex.Query(key, value));
        }

        public long Count(string key, object value)
        {
            IndexContract.ValidateCount(key, value);

            return RawIndex.Count(key, value);
        }

        public string Name
        {
            get { return RawIndex.Name; }
        }

        public Type Type
        {
            get { return RawIndex.Type; }
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}