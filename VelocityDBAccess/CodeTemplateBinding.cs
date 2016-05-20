using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;

namespace VelocityDBAccess
{
    /// <summary>
    /// Store data to be used on template.
    /// </summary>
    internal partial class CodeTemplate
    {
        #region properties
        public string NameSpace { get; set; }
        public SchemaInfo Schema { get; set; }
        public SessionInfo SessionInfo { get; set; }
        public string TypeName { get; set; }
        #endregion
    }
}
