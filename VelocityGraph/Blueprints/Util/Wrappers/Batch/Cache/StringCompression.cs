namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public abstract class StringCompression
    {
        public static StringCompression NoCompression = new NullStringCompression();

        public abstract string Compress(string input);

        private class NullStringCompression : StringCompression
        {
            public override string Compress(string input)
            {
                StringCompressionContract.ValidateCompress(input);
                return input;
            }
        }
    }
}