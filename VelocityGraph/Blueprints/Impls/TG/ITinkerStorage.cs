namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Implementations are responsible for loading and saving a TinkerGrapĥ data.
    /// </summary>
    internal interface ITinkerStorage
    {
        TinkerGrapĥ Load(string directory);
        void Save(TinkerGrapĥ tinkerGrapĥ, string directory);
    }
}