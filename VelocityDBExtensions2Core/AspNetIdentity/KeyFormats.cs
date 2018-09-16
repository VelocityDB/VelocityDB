using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace VelocityDBExtensions2.AspNet.Identity
{
    public static class KeyFormats
    {
        public static string LoginFormat = "L:{0};U:{1}";

        public static string UserFormat = "U:{0}";

        public static string GetKey(object loginKey, object userKey)
        {
            return string.Format(LoginFormat, loginKey, userKey);
        }
    }
}

