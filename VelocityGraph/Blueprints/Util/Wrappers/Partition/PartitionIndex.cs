using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndex : IIndex
    {
        protected PartitionGraph Graph;
        protected IIndex RawIndex;

        public PartitionIndex(IIndex rawIndex, PartitionGraph graph)
        {
            if (rawIndex == null)
                throw new ArgumentNullException(nameof(rawIndex));
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            RawIndex = rawIndex;
            Graph = graph;
        }

        public string Name
        {
            get { return RawIndex.Name; }
        }

        public Type Type
        {
            get { return RawIndex.Type; }
        }

        public long Count(string key, object value)
        {
            IndexContract.ValidateCount(key, value);

            return Get(key, value).LongCount();
        }

        public void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                RawIndex.Remove(key, value, partitionElement.GetBaseElement());
        }

        public void Put(string key, object value, IElement element)
        {
            IndexContract.ValidatePut(key, value, element);

            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                RawIndex.Put(key, value, partitionElement.GetBaseElement());
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            IndexContract.ValidateGet(key, value);

            if (typeof (IVertex).IsAssignableFrom(Type))
                return new PartitionVertexIterable((IEnumerable<IVertex>) RawIndex.Get(key, value), Graph);
            return new PartitionEdgeIterable((IEnumerable<IEdge>) RawIndex.Get(key, value), Graph);
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new PartitionVertexIterable((IEnumerable<IVertex>) RawIndex.Query(key, value), Graph);
            return new PartitionEdgeIterable((IEnumerable<IEdge>) RawIndex.Query(key, value), Graph);
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}