using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VelocityDBAccess
{
  /// <summary>
  /// Class able to generate a dynamic assembly to access a VelocityDb
  /// database.
  /// </summary>
  public class VelocityDBAccessBuilder
  {
    private SchemaInfo schema;
    private SessionInfo sessionInfo;

    /// <summary>
    /// Generate and build access assembly.
    /// The built assembly has a class with a property for each 
    /// persistable type on the schema returning a IEnumerator.
    /// Also a constructor that connects to db and a function to
    /// close the connection.
    /// </summary>
    /// <param name="pAssemblyName">Name of the generated assembly</param>
    /// <param name="pNamespace">Namespace of the generated class</param>
    /// <param name="pTypeName">Name of the generated class</param>
    /// <param name="pInMemory">Choose if generate in memory or 
    /// on disk</param>
    /// <returns>The generated assembly.</returns>
    public Assembly BuildAssembly(AssemblyName pAssemblyName, string pNamespace, string pTypeName, bool pInMemory)
    {
      string lCode;
      CodeTemplate lGenerator = new CodeTemplate()
      {
        NameSpace = pNamespace,
        TypeName = pTypeName,
        Schema = schema,
        SessionInfo = sessionInfo
      };
      // Generate access code from CodeTemplate.tt.
      lCode = lGenerator.TransformText();

      return GenerateAssembly(lCode, pAssemblyName, pInMemory);
    }

    private Assembly GenerateAssembly(string pCode, AssemblyName pName, bool pInMemory)
    {
      CompilerResults lResults;
      Dictionary<string, string> lProviderOpt = new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
      using (var lCodeProvider = new CSharpCodeProvider(lProviderOpt))
      {
        // Obtain list of dependencies from properties.
        List<string> lAssemblies = new List<string> { "System.dll", "System.Transactions.dll", "System.Core.dll" };
        lAssemblies.AddRange(schema.LoadedAssemblies);
        // Create options.
        var lOptions = new CompilerParameters(lAssemblies.ToArray(), pName.CodeBase, true);
        lOptions.GenerateInMemory = pInMemory;
        // Generate Class Library.
        lOptions.GenerateExecutable = false;
        // Compile.
        lResults = lCodeProvider.CompileAssemblyFromSource(lOptions, new string[] { pCode });

      }
      if (lResults.Errors.Count > 0)
      {
        throw new SystemException($@"Could not generate VelocityDB Access Assembly.\nAre there any missing dependencies? {lResults.Errors.ToString()}");
      }
      return lResults.CompiledAssembly;
    }

    public VelocityDBAccessBuilder(SchemaInfo pSchema, SessionInfo pSessionInfo)
    {
      schema = pSchema;
      sessionInfo = pSessionInfo;
    }
  }
}
