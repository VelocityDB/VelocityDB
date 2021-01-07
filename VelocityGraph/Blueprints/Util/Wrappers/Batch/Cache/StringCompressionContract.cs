using System;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public static class StringCompressionContract
    {
        public static void ValidateCompress(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
        }
    }
}