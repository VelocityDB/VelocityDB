using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager
{
    public class SchemaInfo
    {
        /// <summary>
        /// Absolute file paths of assemblies loaded during schema extraction.
        /// </summary>
        public string[] LoadedAssemblies { get; set; }
        /// <summary>
        /// Names of assemblies loaded during schema extraction.
        /// </summary>
        public string[] LoadedAssembliesNames { get; set; }
        /// <summary>
        /// Types which may be present at database.
        /// </summary>
        public Type[] PersistableTypes { get; set; }
        /// <summary>
        /// Type's FullName to a unique, readable singular name.
        /// </summary>
        public SortedSet<string> SingularNames { get; set; }
        /// <summary>
        /// Type's FullName to a unique, readable plural name.
        /// </summary>
        public Dictionary<string, string> TypesNameToPluralName { get; set; }
        /// <summary>
        /// File paths of assemblies containing persistable types to be read.
        /// As provided by the user.
        /// </summary>
        public string[] UserClassesFiles { get; set; }
        /// <summary>
        /// File paths of assemblies which need to be loaded to read 
        /// UserClassesFiles. As provided by user.
        /// </summary>
        public string[] UserDependenciesFiles { get; set; }
    }
}
