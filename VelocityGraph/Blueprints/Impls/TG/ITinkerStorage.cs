using System.Diagnostics.Contracts;

namespace VelocityGraph.Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Implementations are responsible for loading and saving a TinkerGrapĥ data.
    /// </summary>
    [ContractClass(typeof (TinkerStorageContract))]
    internal interface ITinkerStorage
    {
        TinkerGrapĥ Load(string directory);
        void Save(TinkerGrapĥ tinkerGrapĥ, string directory);
    }
}