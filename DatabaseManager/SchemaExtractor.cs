using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;

namespace DatabaseManager
{
  public static class TypeExtensions
  {
    /// <summary>
    /// Gets a string representation of a Type corresponding to how it looks in C# code
    /// </summary>
    /// <param name="t">The type to get the string for</param>
    /// <returns>A string representation of a type</returns>
    public static string ToGenericTypeString(this Type t)
    {
      if (t.DeclaringType != null)
        return t.DeclaringType + "." + t.Name;
      if (!t.IsGenericType)
        return t.FullName;
      string genericTypeName = t.GetGenericTypeDefinition().Name;
      genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
      string genericArgs = string.Join(",", t.GetGenericArguments().Select(ta => ToGenericTypeString(ta)).ToArray());
      return genericTypeName + "<" + genericArgs + ">";
    }
  }

  /// <summary>
  /// Class able to extract SquemaInfo from assemblies.
  /// </summary>
  public class SchemaExtractor : MarshalByRefObject
  {
    /// <summary>
    /// When an assembly is not found, search for it on the already loaded ones.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="pArgs"></param>
    /// <returns></returns>
    private static Assembly AssemblyResolve(object sender, ResolveEventArgs pArgs)
    {
      return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(lAssembly =>lAssembly.FullName.Equals(pArgs.Name));
    }
    /// <summary>
    /// Creates dictionaries that maps from types fullnames to 
    /// a suitable collection name. The resulting name is usually
    /// simple the name of the type. When there is more than
    /// one type with the same name, FullName is progressively
    /// prepended to name until there is no ambiguity.
    /// Two dictionaries are generated, one with pluralized last name
    /// and one with singular one.
    /// </summary>
    /// <param name="pPersistables">Types to be translated.</param>
    /// <param name="pSchema">Schema to add names dictionaries.</param>
    private static void CreateNamesDictionary(Type[] pPersistables, ref SchemaInfo pSchema)
    {
      Dictionary<string, string> lPlural;
      SortedSet<string> lSingular = new SortedSet<string>();

      // Initialy maps FullName to Name.
      lPlural = pPersistables.ToDictionary(lPersistable => lPersistable.ToGenericTypeString(), lPersistable => lPersistable.Name + "s");
      foreach (Type type in pPersistables)
        lSingular.Add(type.ToGenericTypeString());
      // Solve name clashes.
      pPersistables
          .ToLookup(lPersistable => lPersistable.Name)
          .Where(lGroup => lGroup.Count() > 1)
          .Select(lGroup => SolveNameClash(lGroup))
          .ToList()
          .ForEach(delegate(Dictionary<string, string[]> lSub)
          {
            foreach (KeyValuePair<string, string[]> lPair in lSub)
            {
              // Singular names just join names.
             // lSingular[lPair.Key] = String.Join("_", lPair.Value);
              // Last name gets pluralized for plural names.
              lPair.Value[lPair.Value.Count() - 1] = lPair.Value.Last() + "s";
              lPlural[lPair.Key] = String.Join("_", lPair.Value);

            }
          });
      pSchema.SingularNames = lSingular;
      pSchema.TypesNameToPluralName = lPlural;
    }
    /// <summary>
    /// Extract schema information from assemblies.
    /// </summary>
    /// <param name="pClassesFilenames">Assemblies containing 
    /// desired persistable types.</param>
    /// <param name="pDependenciesFilenames">Assemblies required by
    /// pClassesFilenames Assemblies.</param>
    /// <returns></returns>
    public static SchemaInfo Extract(string[] pClassesFilenames, string[] pDependenciesFilenames)
    {
      // Create schema, get persistable types, loaded assemblies and
      // loaded assemblies names, and finally create names for each type.
      SchemaInfo lSchema = new SchemaInfo()
      {
        UserClassesFiles = pClassesFilenames,
        UserDependenciesFiles = pDependenciesFilenames
      };
      // Already loaded assemblies sometimes get not found! register
      // resolver.
      AppDomain.CurrentDomain.AssemblyResolve += SchemaExtractor.AssemblyResolve;
      GetAssembliesAndTypes(pClassesFilenames, pDependenciesFilenames, ref lSchema);
      CreateNamesDictionary(lSchema.PersistableTypes, ref lSchema);
      AppDomain.CurrentDomain.AssemblyResolve -= SchemaExtractor.AssemblyResolve;
      return lSchema;
    }

    /// <summary>
    /// Load assemblies and get exported persistable types 
    /// from them.
    /// </summary>
    /// <param name="pClassesFilenames">Filenames of all assemblies to 
    /// load.</param>
    /// <param name="pAssemblies">Loaded assemblies.</param>
    /// <param name="pTypes">Types found.</param>
    private static void GetAssembliesAndTypes(string[] pClassFilenames, string[] pDependencyFilenames,ref SchemaInfo pSchema)
    {
      // Creates a SchemaExtractor instance on a new domain. This is done
      // so that no assembly is loaded on process start, and at the end,
      // only needed assemblies are loaded.
      //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      AppDomain lDomain = AppDomain.CreateDomain("User assemblies domain.");
      SchemaExtractor lExtractor;
      OptimizedPersistable pObj = (OptimizedPersistable)lDomain.CreateInstanceFromAndUnwrap(typeof(OptimizedPersistable).Assembly.CodeBase, typeof(OptimizedPersistable).FullName);
      lExtractor = (SchemaExtractor)lDomain.CreateInstanceFromAndUnwrap(typeof(SchemaExtractor).Assembly.CodeBase,typeof(SchemaExtractor).FullName);
      // Load assemblies and types on the new domain.
      List<string> lTypeNames = null;
      List<string> lAssemblyNames = null;
      List<string> lActualDependencies = null;
      lExtractor.GetAssembliesAndTypesHelper(pClassFilenames, pDependencyFilenames, ref lAssemblyNames, ref lTypeNames, ref lActualDependencies);
      AppDomain.Unload(lDomain);

      // Load assemblies on this domain (to be able to access types).
      Assembly l;
      foreach (string lDep in lActualDependencies)
      {
        l = Assembly.LoadFrom(lDep);
      }

      // Obtain types from names and fill in schema.
      pSchema.PersistableTypes = lTypeNames.Select(lTypeName => Type.GetType(lTypeName, true)).ToArray();
      pSchema.LoadedAssemblies = lActualDependencies.ToArray();
      pSchema.LoadedAssembliesNames = lAssemblyNames.ToArray();
    }
    /// <summary>
    /// Internal method to be used on a clean domain.
    /// </summary>
    /// <param name="pClassFilenames">Assemblies containing target
    /// persistable types.</param>
    /// <param name="pDependencyFilenames">Assemblies containing 
    /// dependencies</param>
    /// <param name="pAssemblyNames">Names of the loaded assemblies</param>
    /// <param name="pTypes">Persistable types found</param>
    /// <param name="pActualDependencies">Full names of loaded
    /// assemblies</param>
    internal void GetAssembliesAndTypesHelper(
        string[] pClassFilenames, string[] pDependencyFilenames,
        ref List<string> pAssemblyNames, ref List<string> pTypes,
        ref List<string> pActualDependencies)
    {
      // Get initially loaded assemblies.
      string[] lInitialAssemblies = GetLoadedAssemblies();

      pTypes = new List<string>();
      pAssemblyNames = new List<string>();
      // Preload all dependency files. Keep in mind that dependencies
      // on the same folder gets loaded automatically.
      // Some runtime needed assemblies must be explicitly named, even
      // if on the same folder.
      foreach (string lDependencyFile in pDependencyFilenames)
      {
        try
        {
          Assembly.LoadFrom(lDependencyFile);
        }
        catch (FileNotFoundException e)
        {
          throw new FileNotFoundException("Dependency file not found:\n" + e.FileName);
        }
      }
      // Load assemblies.
      foreach (string lAssemblyFile in pClassFilenames)
      {
        try
        {
          Assembly lAssembly = Assembly.LoadFrom(lAssemblyFile);
          pAssemblyNames.Add(lAssembly.GetName().Name);
          try
          {
            pTypes.AddRange(
                from lType in lAssembly.GetExportedTypes()
                where lType
                    .GetInterface("IOptimizedPersistable") != null
                where !lType.IsGenericType
                select lType.AssemblyQualifiedName);
          }
          // If not found while reading types, dependency is missing.
          catch (FileNotFoundException e)
          {
            throw new DllNotFoundException(
                "Could not locate dependency:\n" +
                e.FileName + ",\nrequired by:\n" +
                lAssemblyFile +
                "\nPlace the dependency under the same directory" +
                " as requiring Assembly or add it on " +
                "dependency list of database configuration.");
          }
        }
        // If not found while oppening, file was deleted.
        catch (FileNotFoundException e)
        {
          throw new FileNotFoundException("Classes Assembly not found:\n" + e.FileName);
        }
      }
      // Actual dependencies are loaded assemblies except for those
      // that were already loaded.
      pActualDependencies = GetLoadedAssemblies().Except(lInitialAssemblies).ToList();
    }

    /// <summary>
    /// Gets filename of all non-dynamic loaded assemblies.
    /// </summary>
    /// <returns></returns>
    private static string[] GetLoadedAssemblies()
    {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      return assemblies
          .Where(lAssembly => !lAssembly.IsDynamic)
          .Select(lAssembly => lAssembly.CodeBase)
          .Where(lCodeBase => lCodeBase != null)
        // Remove the file:/// prefix.
          .Select(lCodeBase => new string(lCodeBase.Skip(8).ToArray()))
          .ToArray();
    }
    /// <summary>
    /// Gets public instance fields and properties excluding ones
    /// internally defined by VelocityDB.
    /// </summary>
    /// <param name="pType"></param>
    /// <returns></returns>
    public static List<MemberInfo> GetDataMembers(Type pType)
    {
      List<MemberInfo> lResult = new List<MemberInfo>();
      BindingFlags lFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
      string[] lIgnore = new string[]{"IOptimizedPersistable", "OptimizedPersistable"};
      lResult.AddRange(pType.GetProperties(lFlags).Where(lProp => !lIgnore.Contains(lProp.DeclaringType.Name)));
      lResult.AddRange(pType.GetFields().Where(lField =>!lIgnore.Contains(lField.DeclaringType.Name)));
      return lResult;
    }
    /// <summary>
    /// From types with the same Name, create new names progressively
    /// using FullName to disambiguate.
    /// </summary>
    /// <param name="pNameClash">Grouping with clashing name and types
    /// that clash.</param>
    /// <returns></returns>
    private static Dictionary<string, string[]> SolveNameClash(IGrouping<string, Type> pNameClash)
    {
      // Create lookup from FullName to types.
      var lFullNameLookup = pNameClash.ToLookup(lType => lType.FullName);
      // See if there is any duplicated FullName (should never happen).
      List<string> lDuplicatedFullName = lFullNameLookup
                          .Where(lTypes => lTypes.Count() > 1)
                          .Select(lTypes => lTypes.First().FullName)
                          .ToList();
      if (lDuplicatedFullName.Any())
      {
        throw new SystemException("Found classes with same FullName: " + String.Join(", ", lDuplicatedFullName) + ".");
      }
      // Create lists for clashing types, indexes indicating when the
      // FullNames differentiate and lists of 'sections' of FullName
      // reversed.
      List<Type> lClashTypes = lFullNameLookup.Select(lGroup => lGroup.First()).ToList();
      List<int> lDiffIndex = lClashTypes.Select(lType => -1).ToList();
      List<List<string>> lFullNames = lClashTypes.Select(lType => lType.FullName.Split('.').Reverse().ToList()).ToList();
      // Loop through FullName 'sections' until exaustion of the longest.
      int lMax = lFullNames.Select(lStrings => lStrings.Count()).Max();
      for (int i = 0; i < lMax; i++)
      {
        // Loop through all clashing types.
        for (int j = 0; j < lClashTypes.Count(); j++)
        {
          // Skip types already differentiated.
          if (lDiffIndex[j] != -1) continue;
          string current = lFullNames[j][i];
          // Assume it will diferentiate.
          bool lDiffed = true;
          // Loop through all other clashing types to see if
          // this is different from others at this index.
          for (int k = 0; k < lClashTypes.Count(); k++)
          {
            if (k == j) continue;
            if ((lDiffIndex[k] == -1) && (i < lFullNames[k].Count()) && (lFullNames[k][i].Equals(current)))
            {
              lDiffed = false;
              break;
            }
          }
          // Store index of differentiation.
          if (lDiffed) lDiffIndex[j] = i;
        }
      }
      // Create a dictionary with Reversed fullnames until differentiation.
      Dictionary<string, string[]> lResult = new Dictionary<string, string[]>();
      for (int i = 0; i < lClashTypes.Count(); i++)
      {
        // Unreverse the list using only needed items.
        lFullNames[i] = lFullNames[i].Take(lDiffIndex[i] + 1).ToList();
        lFullNames[i].Reverse();
        lResult[lClashTypes[i].FullName] = lFullNames[i].ToArray();
      }
      return lResult;
    }
  }
}
