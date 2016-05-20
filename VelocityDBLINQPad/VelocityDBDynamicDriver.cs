using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using System.IO;
using LINQPad;
using System.Net;
using System.Collections;
using VelocityDBAccess;
using UtilitiesModule;

namespace VelocityDB.LINQPad
{
    public class VelocityDBDynamicDriver : DynamicDataContextDriver
    {
        public override string Author { get { return "Laércio Asano"; } }
        public override string Name { get { return "VelocityDB"; } }

        /// <summary>
        /// Class used to customize data presentation.
        /// It exposes only public instance fields or properties,
        /// excluding ones defined by IOptimizedPersitable.
        /// </summary>
        private class CleanPersistable : ICustomMemberProvider
        {
            private readonly List<MemberInfo> members;
            private readonly Object obj;

            public CleanPersistable(object pObj)
            {
                obj = pObj;
                members = SchemaExtractor.GetDataMembers(pObj.GetType());
            }
            public IEnumerable<string> GetNames()
            {
                return members.Select(lProp => lProp.Name);
            }
            public IEnumerable<Type> GetTypes()
            {
                return members.Select(lProp => ReflectionUtils.GetDataMemberType(lProp));
            }
            public IEnumerable<object> GetValues()
            {
                return members.Select(lProp => ReflectionUtils.GetDataMemberValue(lProp, obj));
            }
        }	

        public override void DisplayObjectInGrid(object objectToDisplay, GridOptions options)
        {
            Type lType = objectToDisplay.GetType();
            // If it objectToDisplay is enumerable, get its type argument.
            if (lType.IsGenericType)
            {
                Type lEnumerable = lType.GetInterface("System.Collections.Generic.IEnumerable`1");
                if (lEnumerable != null)
                {
                    lType = lEnumerable.GetGenericArguments()[0];
                }
            }
            string[] lIgnore = { "IOptimizedPersistable", "OptimizedPersistable" };
            // Customize only if it is a persistable object.
            // Note: Unluckly IOptimizedPersistable members that have
            // a name overridden by derived class can't be filtered.
            if (lType.GetInterface("IOptimizedPersistable") != null)
            {
                // Keep properties public instance fields and properties
                // that are not defined by IOptimizedPersistable.
                string[] lKeep = SchemaExtractor
                    .GetDataMembers(lType)
                    .Select(lMember => lMember.Name).ToArray();
                // Select to exclude all members that should not be keeped...
                options.MembersToExclude = lType.GetMembers()
                    .Select(lMember => lMember.Name)
                    .Where(lName => !lKeep.Contains(lName))
                    .ToArray();
                // If a OptimizedPersistable (i.e. the type being used is the 
                // same as the object's instead of a element type) 
                // is send to render on grid,
                // an exception is rised because LINQPad assumes to be an
                // enumerable of only one kind, which is not true for
                // OptimizedPersistable. In this case, wrap on a list.
                if(lType.Equals(objectToDisplay.GetType()))
                {
                    objectToDisplay = new List<object>() { objectToDisplay };
                }
            }
            // TODO: Some problem with duplicated members.
            // On BI.Model, Customer -> Items -> Items -> PartyRole ->??
            base.DisplayObjectInGrid(objectToDisplay, options);
        }

        /// <summary>
        /// Returns a list of additional assemblies to reference when building queries. To refer to
        /// an assembly in the GAC, specify its fully qualified name, otherwise specified the assembly's full
        /// location on the hard drive. Assemblies in the same folder as the driver, however, don't require a
        /// folder name. If you're unable to find the necessary assemblies, throw an exception, with a message
        /// indicating the problem assembly.</summary>
        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo pCxInfo)
        {
            List<string> lAssemblies = new List<string>();
            VelocityDBProperties lProp = new VelocityDBProperties(pCxInfo);
            Assembly[] lLoaded = AppDomain.CurrentDomain.GetAssemblies();
            // Add only not loaded assemblies. (Skip(8) removes the file:/// prefix.
            string[] lRes = lProp.ActualDepencies.Where(lDep => !lLoaded.Where(lAssembly => new string(lAssembly.CodeBase.Skip(8).ToArray()).Equals(lDep)).Any()).ToArray();
            return lRes;
        }

        public override string GetConnectionDescription(IConnectionInfo pCxInfo)
        {
            // Format VelocityDB@<server>-<database>. If local, <server> = local.
            var lProp = new VelocityDBProperties(pCxInfo);
            string lDesc = Path.GetFileNameWithoutExtension(lProp.DBFolder);
            string lServ = "local";
            if (lProp.SessionType.Equals(VelocityDBProperties.ServerClientSession))
            {
                lServ = lProp.Host;
            }
            return "VelocityDB@" + lServ + "-" + lDesc;
        }

        /// <summary>
        /// Allows you to change how types are displayed in the output window - in particular, this 
        /// lets you prevent LINQPad from endlessly enumerating lazily evaluated properties. Overriding this 
        /// method is an alternative to implementing ICustomMemberProvider in the target types. See
        /// http://www.linqpad.net/FAQ.aspx#extensibility for more info.</summary>
        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            // Customize only if it is a persistable object.
            if (objectToWrite.GetType().GetInterface("IOptimizedPersistable") != null)
            {
                return new CleanPersistable(objectToWrite);
            }
            else return null;
        }

        /// <summary>
        /// Returns a list of additional namespaces that should be imported automatically into all 
        /// queries that use this driver. This should include the commonly used namespaces of your ORM or
        /// querying technology .</summary>
        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo pCxInfo)
        {
            var lProp = new VelocityDBProperties(pCxInfo);
            return new List<string> { "VelocityDb" };
        }

        /// <summary>
        /// Builds an assembly containing a typed data pContext, and returns data for the Schema Explorer.
        /// </summary>
        /// <param name="pCxInfo">Connection information, as entered by the user</param>
        /// <param name="pAssemblyToBuild">Name and location of the target assembly to build</param>
        /// <param name="pNameSpace">The suggested namespace of the typed data pContext. You must update this
        /// parameter if you don't use the suggested namespace.</param>
        /// <param name="pTypeName">The suggested type name of the typed data pContext. You must update this
        /// parameter if you don't use the suggested type name.</param>
        /// <returns>Schema which will be subsequently loaded into the Schema Explorer.</returns>
        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo pCxInfo, AssemblyName pAssemblyToBuild, ref string pNameSpace, ref string pTypeName)
        {
            var lProp = new VelocityDBProperties(pCxInfo);
            VelocityDBBuilder lBuilder = new VelocityDBBuilder(this, lProp);
            lBuilder.BuildAssembly(pAssemblyToBuild, pNameSpace, pTypeName);
            return lBuilder.BuildSchema();
        }

        /// <summary>
        /// Displays a dialog prompting the user for connection details. The isNewConnection
        /// parameter will be true if the user is creating a new connection rather than editing an
        /// existing connection. This should return true if the user clicked OK. If it returns false,
        /// any changes to the IConnectionInfo object will be rolled back.</summary>
        public override bool ShowConnectionDialog(IConnectionInfo pCxInfo, bool pIsNewConnection)
        {
            VelocityDBProperties lProp;
            if (pIsNewConnection)
            {
                lProp = new VelocityDBProperties(pCxInfo)
                {
                    Host = Dns.GetHostName(),
                    WindowsAuth = false,
                    PessimisticLocking = false
                };
            }
            else 
              lProp = new VelocityDBProperties(pCxInfo);
            bool? result = new ConnectionDialog(pCxInfo).ShowDialog();
            if (result != true)
              return false;

            // This function, as well as GetSchemaAndBuildAssembly, runs on a separeted appdomain. But different from 
            // GetSchemaAndBuildAssembly, pCxInfo gets persisted if true is returned. So this is the best (found) place to create
            // a list of dependencies.
            
            // Save already loaded assemblies.
            SchemaInfo lSchema = SchemaExtractor.Extract(lProp.ClassesFilenamesArray, lProp.DependencyFilesArray);
            SessionInfo lSessionInfo = new SessionInfo()
            {
                DBFolder = lProp.DBFolder,
                Host = lProp.Host,
                PessimisticLocking = lProp.PessimisticLocking,
                SessionType = lProp.SessionType,
                WindowsAuth = lProp.WindowsAuth
            };

            VelocityDBAccessBuilder lBuilder = new VelocityDBAccessBuilder(lSchema, lSessionInfo);
            lBuilder.BuildAssembly(new AssemblyName("DummyName"), "DummyName", "DummyName", false);

            lProp.ActualDepencies = lSchema.LoadedAssemblies;
            return true;
        }

        /// <summary>
        /// This virtual method is called after a query has completed. You can use this hook to
        /// perform cleanup activities such as disposing of the context or other objects.</summary>
        public override void TearDownContext(IConnectionInfo pCxInfo, object pContext, QueryExecutionManager pExecutionManager, object[] pConstructorArguments) 
        {
            MethodInfo lMethod = pContext.GetType().GetMethod("CloseSession");
            lMethod.Invoke(pContext, new object[] { });
        }
    }
}
