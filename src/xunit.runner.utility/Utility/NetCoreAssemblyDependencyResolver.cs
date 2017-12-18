#if NETCOREAPP1_0

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Internal.Microsoft.Extensions.DependencyModel;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A class which encapsulates support for resolving dependencies for .NET core assemblies.
    /// This includes support for using the .deps.json file that sits alongside the assembly
    /// so that dependencies do not need to be copied locally. It also supports loading native
    /// assembly assets.
    /// </summary>
    public class NetCoreAssemblyDependencyResolver : AssemblyLoadContext, IDisposable
    {
        readonly DependencyContextAssemblyCache assemblyCache;
        readonly IMessageSink internalDiagnosticsMessageSink;

        /// <summary/>
        [Obsolete("Please call the constructor with the support for internal diagnostics messages")]
        public NetCoreAssemblyDependencyResolver(string assemblyFilePath) : this(assemblyFilePath, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetCoreAssemblyDependencyResolver"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The path to the assembly</param>
        /// <param name="internalDiagnosticsMessageSink">An optional message sink for use with internal diagnostics messages;
        /// may pass <c>null</c> for no internal diagnostics messages</param>
        public NetCoreAssemblyDependencyResolver(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink)
        {
            this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

            var assembly = LoadFromAssemblyPath(assemblyFileName);
            var assemblyFolder = Path.GetDirectoryName(assemblyFileName);
            var dependencyContext = DependencyContext.Load(assembly);

            assemblyCache = new DependencyContextAssemblyCache(assemblyFolder, dependencyContext, internalDiagnosticsMessageSink);

            Default.Resolving += OnResolving;
        }

        /// <inheritdoc/>
        public void Dispose()
            => Default.Resolving -= OnResolving;

        /// <inheritdoc/>
        protected override Assembly Load(AssemblyName assemblyName)
            => Default.LoadFromAssemblyName(assemblyName);

        /// <inheritdoc/>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var result = assemblyCache.LoadUnmanagedLibrary(unmanagedDllName, path => LoadUnmanagedDllFromPath(path));
            if (result == default)
                result = base.LoadUnmanagedDll(unmanagedDllName);

            return result;
        }

        Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
            => assemblyCache.LoadManagedDll(name.Name, path => LoadFromAssemblyPath(path));
    }
}

#endif
