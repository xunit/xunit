// Adapted from https://samcragg.wordpress.com/2017/06/30/resolving-assemblies-in-net-core/

#if NETCOREAPP1_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        readonly string assemblyFolder;
        readonly ICompilationAssemblyResolver assemblyResolver;
        readonly DependencyContext dependencyContext;
        readonly AssemblyLoadContext loadContext;

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
            dependencyContext = DependencyContext.Load(assembly);
            assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
            {
                new AppBaseCompilationAssemblyResolver(assemblyFolder),
                new ReferenceAssemblyPathResolver(),
                new PackageCompilationAssemblyResolver()
            });
            loadContext = AssemblyLoadContext.GetLoadContext(assembly);
            loadContext.Resolving += OnResolving;
        }

        /// <inheritdoc/>
        public void Dispose()
            => loadContext.Resolving -= OnResolving;

        Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            bool NamesMatch(RuntimeLibrary runtime)
                => string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);

            // Try to find dependency from .deps.json
            var library = dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
            if (library != null)
            {
                var wrapper = new CompilationLibrary(library.Type, library.Name, library.Version, library.Hash,
                                                     library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                                                     library.Dependencies, library.Serviceable);

                var assemblies = new List<string>();
                assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);

                if (assemblies.Count > 0)
                    return loadContext.LoadFromAssemblyPath(assemblies[0]);
            }

            // Try to find dependency in the local folder
            var assemblyPath = Path.Combine(assemblyFolder, name.Name);
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath + ".dll")
                ?? AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath + ".exe");
        }
    }
}

#endif
