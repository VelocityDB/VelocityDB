using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndexableGraph : ReadOnlyGraph, IIndexableGraph
    {
        private readonly IIndexableGraph _baseIndexableGraph;

        public ReadOnlyIndexableGraph(IIndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            if (baseIndexableGraph == null)
                throw new ArgumentNullException(nameof(baseIndexableGraph));

            _baseIndexableGraph = baseIndexableGraph;
        }

        public void DropIndex(string indexName)
        {
            IndexableGraphContract.ValidateDropIndex(indexName);

            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            IndexableGraphContract.ValidateGetIndex(indexName, indexClass);

            var index = _baseIndexableGraph.GetIndex(indexName, indexClass);
            return new ReadOnlyIndex(this, index);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            return new ReadOnlyIndexIterable(this, _baseIndexableGraph.GetIndices());
        }
    }
}