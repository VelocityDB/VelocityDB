using System.Diagnostics.Contracts;

namespace VelocityGraph.Frontenac.Blueprints.Impls.TG
{
    [ContractClassFor(typeof (ITinkerStorage))]
    public abstract class TinkerStorageContract : ITinkerStorage
    {
        public TinkerGrapĥ Load(string directory)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            Contract.Ensures(Contract.Result<TinkerGrapĥ>() != null);
            return null;
        }

        public void Save(TinkerGrapĥ tinkerGrapĥ, string directory)
        {
            Contract.Requires(tinkerGrapĥ != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));
        }
    }
}