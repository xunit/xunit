#if NETCOREAPP1_0

// Adapted from https://samcragg.wordpress.com/2017/06/30/resolving-assemblies-in-net-core/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Xunit
{
    /// <summary>
    /// A class which encapsulates support for resolving dependencies for .NET core assemblies.
    /// This includes support for using the .deps.json file that sits alongside the assembly
    /// so that dependencies do not need to be copied locally.
    /// </summary>
    public class NetCoreAssemblyDependencyResolver : IDisposable
    {
        static string CurrentRuntime = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win"
                                     : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx"
                                     : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "unix"
                                     : "unknown";

        Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> assemblyFileNameToLibraryMap;
        string assemblyFolder;
        ICompilationAssemblyResolver assemblyResolver;
        DependencyContext dependencyContext;
        AssemblyLoadContext loadContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetCoreAssemblyDependencyResolver"/> class.
        /// </summary>
        /// <param name="assemblyFilePath">The path to the assembly</param>
        public NetCoreAssemblyDependencyResolver(string assemblyFilePath)
        {
            if (assemblyFilePath != null)
                assemblyFilePath = Path.GetFullPath(assemblyFilePath);

            Guard.FileExists(nameof(assemblyFilePath), assemblyFilePath);

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFilePath);
            assemblyFolder = Path.GetDirectoryName(assemblyFilePath);

            Initialize(assembly);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetCoreAssemblyDependencyResolver"/> class.
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public NetCoreAssemblyDependencyResolver(Assembly assembly)
        {
            var assemblyFilePath = assembly.GetLocalCodeBase();
            assemblyFolder = Path.GetDirectoryName(assemblyFilePath);

            Initialize(assembly);
        }

        void Initialize(Assembly assembly)
        {
            dependencyContext = DependencyContext.Load(assembly);

            assemblyFileNameToLibraryMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.RuntimeAssemblyGroups?.Count > 0)
                                 .Select(lib => Tuple.Create(lib, lib.RuntimeAssemblyGroups.FirstOrDefault(grp => grp.Runtime == CurrentRuntime || string.IsNullOrEmpty(grp.Runtime))))
                                 .Where(tuple => tuple.Item2?.AssetPaths != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileNameWithoutExtension(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            assemblyResolver = new XunitPackageCompilationAssemblyResolver();
            loadContext = AssemblyLoadContext.GetLoadContext(assembly);
            loadContext.Resolving += OnResolving;
        }

        /// <inheritdoc/>
        public void Dispose()
            => loadContext.Resolving -= OnResolving;

        Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            // Try to find dependency from .deps.json
            if (assemblyFileNameToLibraryMap.TryGetValue(name.Name, out var libraryTuple))
            {
                var library = libraryTuple.Item1;
                var assetGroup = libraryTuple.Item2;
                var wrapper = new CompilationLibrary(library.Type, library.Name, library.Version, library.Hash,
                                                     assetGroup.AssetPaths, library.Dependencies, library.Serviceable);

                var assemblies = new List<string>();
                if (assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                {
                    var assembly = assemblies.FirstOrDefault(a => string.Equals(name.Name, Path.GetFileNameWithoutExtension(a), StringComparison.OrdinalIgnoreCase));
                    if (assembly != null)
                        return loadContext.LoadFromAssemblyPath(assembly);
                }
            }

            // Try to find dependency in the local folder
            var assemblyPath = Path.Combine(assemblyFolder, name.Name);

            foreach (var extension in new[] { ".dll", ".exe" })
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath + extension);
                    if (assembly != null)
                        return assembly;
                }
                catch { }

            return null;
        }
    }
}

#endif
