using System;

namespace Frontenac.Blueprints.Impls.TG
{
    public static class TinkerStorageContract
    {
        public static void ValidateLoad(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));
        }

        public static void ValidateSave(TinkerGrapĥ tinkerGrapĥ, string directory)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));
        }
    }
}