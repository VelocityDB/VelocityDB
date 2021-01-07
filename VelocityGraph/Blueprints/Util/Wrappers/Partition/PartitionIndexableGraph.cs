using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndexableGraph : PartitionGraph, IIndexableGraph
    {
        protected IIndexableGraph BaseIndexableGraph;

        public PartitionIndexableGraph(IIndexableGraph baseIndexableGraph, string writeGraphKey, string writeGraph,
                                       IEnumerable<string> readGraphs)
            : base(baseIndexableGraph, writeGraphKey, writeGraph, readGraphs)
        {
            if (baseIndexableGraph == null)
                throw new ArgumentNullException(nameof(baseIndexableGraph));
            if (readGraphs == null)
                throw new ArgumentNullException(nameof(readGraphs));

            BaseIndexableGraph = baseIndexableGraph;
        }

        public PartitionIndexableGraph(IIndexableGraph baseIndexableGraph, string writeGraphKey, string readWriteGraph)
            : base(baseIndexableGraph, writeGraphKey, readWriteGraph)
        {
            if (baseIndexableGraph == null)
                throw new ArgumentNullException(nameof(baseIndexableGraph));
        }

        public void DropIndex(string indexName)
        {
            IndexableGraphContract.ValidateDropIndex(indexName);

            BaseIndexableGraph.DropIndex(indexName);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            return new PartitionIndexIterable(BaseIndexableGraph.GetIndices(), this);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            IndexableGraphContract.ValidateGetIndex(indexName, indexClass);

            var index = BaseIndexableGraph.GetIndex(indexName, indexClass);
            return null == index ? null : new PartitionIndex(index, this);
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            IndexableGraphContract.ValidateCreateIndex(indexName, indexClass, indexParameters);
            return new PartitionIndex(BaseIndexableGraph.CreateIndex(indexName, indexClass, indexParameters), this);
        }
    }
}