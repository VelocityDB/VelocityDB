using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class UrlCompression : StringCompression
    {
        private const string Delimiter = "$";
        private static readonly char[] UrlDelimiters = new[] {'/', '#', ':'};

        private static readonly char[] Base36Chars = new[]
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K',
                'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };

        private readonly Dictionary<string, string> _urlPrefix = new Dictionary<string, string>();
        private int _prefixCounter;

        public override string Compress(string input)
        {
            StringCompressionContract.ValidateCompress(input);

            var url = SplitUrl(input);
            string prefix;
            _urlPrefix.TryGetValue(url[0], out prefix);
            if (prefix == null)
            {
                //New Prefix
                prefix = string.Concat(IntToBase36String(_prefixCounter), Delimiter);
                _prefixCounter++;
                _urlPrefix[url[0]] = prefix;
            }

            return string.Concat(prefix, url.Length > 1 ? url[1] : string.Empty);
        }

        private static string[] SplitUrl(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var res = new string[2];
            var pos = UrlDelimiters.Select(delimiter => url.LastIndexOf(delimiter)).Concat(new[] {-1}).Max();
            if (pos < 0)
            {
                res[0] = "";
                res[1] = url;
            }
            else
            {
                res[0] = url.Substring(0, pos + 1);
                res[1] = url.Substring(pos + 1);
            }

            return res;
        }

        //see http://msdn.microsoft.com/en-ca/library/aa245218%28v=vs.60%29.aspx and
        //http://grepcode.com/file/repository.grepcode.com/java/root/jdk/openjdk/6-b14/java/lang/Character.java
        //where I found out that MAX_RADIX = 36

        //const int MAX_RADIX = 36;

        private static string IntToBase36String(int value)
        {
            return IntToStringFast(value, Base36Chars);
        }

        /// <summary>
        ///     http://stackoverflow.com/questions/923771/quickest-way-to-convert-a-base-10-number-to-any-base-in-net
        /// </summary>
        private static string IntToStringFast(int value, IList<char> baseChars)
        {
            if (baseChars == null)
                throw new ArgumentNullException(nameof(baseChars));

            // 32 is the worst cast buffer size for base 2 and int.MaxValue
            var i = 32;
            var buffer = new char[i];
            var targetBase = baseChars.Count;

            do
            {
                if (buffer.Length <= --i) continue;
                var idx = value%targetBase;
                if (baseChars.Count > idx && i >= 0) buffer[i] = baseChars[idx];
                value = value / targetBase;
            } while (value > 0);

            var result = new char[32 - i];
            if(i >= buffer.GetLowerBound(0))
                Array.Copy(buffer, i, result, 0, 32 - i);

            return new string(result);
        }
    }
}