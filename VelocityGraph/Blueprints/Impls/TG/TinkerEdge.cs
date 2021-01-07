using System;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal class TinkerEdge : TinkerElement, IEdge
    {
        private readonly IVertex _inVertex;
        private readonly IVertex _outVertex;

        public TinkerEdge(string id, IVertex outVertex, IVertex inVertex, string label, TinkerGrapĥ tinkerGrapĥ)
            : base(id, tinkerGrapĥ)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            Label = label;
            _outVertex = outVertex;
            _inVertex = inVertex;
            tinkerGrapĥ.EdgeKeyIndex.AutoUpdate(StringFactory.Label, Label, null, this);
        }

        public string Label { get; protected set; }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            if (direction == Direction.In)
                return _inVertex;
            if (direction == Direction.Out)
                return _outVertex;
            throw ExceptionFactory.BothIsNotSupported();
        }

        public override string ToString()
        {
            return this.EdgeString();
        }
    }
}