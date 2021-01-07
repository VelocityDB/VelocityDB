using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public abstract class PartitionElement : DictionaryElement
    {
        protected readonly IElement Element;
        protected PartitionGraph PartitionInnerTinkerGrapĥ;

        protected PartitionElement(IElement element, PartitionGraph partitionInnerTinkerGrapĥ):base(partitionInnerTinkerGrapĥ)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (partitionInnerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(partitionInnerTinkerGrapĥ));

            Element = element;
            PartitionInnerTinkerGrapĥ = partitionInnerTinkerGrapĥ;
        }

        public override void SetProperty(string key, object value)
        {
            ElementContract.ValidateSetProperty(key, value);

            if (!key.Equals(PartitionInnerTinkerGrapĥ.PartitionKey))
                Element.SetProperty(key, value);
        }

        public override object GetProperty(string key)
        {
            ElementContract.ValidateGetProperty(key);
            return key.Equals(PartitionInnerTinkerGrapĥ.PartitionKey) ? null : Element.GetProperty(key);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            return key.Equals(PartitionInnerTinkerGrapĥ.PartitionKey) ? null : Element.RemoveProperty(key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Element.GetPropertyKeys()
                .Except(new[] { PartitionInnerTinkerGrapĥ.PartitionKey })
                .ToArray();
        }

        public override object Id
        {
            get { return Element.Id; }
        }

        public override void Remove()
        {
            if (this is IVertex)
                Graph.RemoveVertex(this as IVertex);
            else
                Graph.RemoveEdge(this as IEdge);
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }

        public override int GetHashCode()
        {
            return Element.GetHashCode();
        }

        public IElement GetBaseElement()
        {
            return Element;
        }

        public string GetPartition()
        {
            return (string)Element.GetProperty(PartitionInnerTinkerGrapĥ.PartitionKey);
        }

        public void SetPartition(string partition)
        {
            if (string.IsNullOrWhiteSpace(partition))
                throw new ArgumentNullException(nameof(partition));
            Element.SetProperty(PartitionInnerTinkerGrapĥ.PartitionKey, partition);
        }

        public override string ToString()
        {
            return Element.ToString();
        }
    }
}