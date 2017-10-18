#if NETCOREAPP1_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Internal.Microsoft.DotNet.PlatformAbstractions;
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
        static readonly Tuple<string, Assembly> ManagedAssemblyNotFound = new Tuple<string, Assembly>(null, null);
        static readonly string[] UnmanagedDllFormats = GetUnmanagedDllFormats().ToArray();

        readonly string assemblyFolder;
        readonly XunitPackageCompilationAssemblyResolver assemblyResolver;
        readonly DependencyContext dependencyContext;
        readonly IMessageSink internalDiagnosticsMessageSink;
        readonly Dictionary<string, Assembly> managedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> managedAssemblyMap;
        readonly Dictionary<string, string> unmanagedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> unmanagedAssemblyMap;

        /// <summary/>
        [Obsolete("Please call the constructor with the support for internal diagnostics messages")]
        public NetCoreAssemblyDependencyResolver(string assemblyFilePath) : this(assemblyFilePath, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetCoreAssemblyDependencyResolver"/> class.
        /// </summary>
        /// <param name="assemblyFilePath">The path to the assembly</param>
        /// <param name="internalDiagnosticsMessageSink">An optional message sink for use with internal diagnostics messages;
        /// may pass <c>null</c> for no internal diagnostics messages</param>
        public NetCoreAssemblyDependencyResolver(string assemblyFilePath, IMessageSink internalDiagnosticsMessageSink)
        {
            this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

            var assembly = LoadFromAssemblyPath(assemblyFilePath);

            assemblyFolder = Path.GetDirectoryName(assemblyFilePath);
            assemblyResolver = new XunitPackageCompilationAssemblyResolver(internalDiagnosticsMessageSink);
            dependencyContext = DependencyContext.Load(assembly);

            var compatibleRuntimes = default(HashSet<string>);
            var currentRuntime = RuntimeEnvironment.GetRuntimeIdentifier();
            var fallbacks = dependencyContext.RuntimeGraph.FirstOrDefault(x => string.Equals(x.Runtime, currentRuntime, StringComparison.OrdinalIgnoreCase));
            if (fallbacks != null)
                compatibleRuntimes = new HashSet<string>(fallbacks.Fallbacks, StringComparer.OrdinalIgnoreCase);
            else
                compatibleRuntimes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            compatibleRuntimes.Add(currentRuntime);
            compatibleRuntimes.Add(string.Empty);

            managedAssemblyCache = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            managedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.RuntimeAssemblyGroups?.Count > 0)
                                 .Select(lib => Tuple.Create(lib, lib.RuntimeAssemblyGroups.FirstOrDefault(libGroup => compatibleRuntimes.Contains(libGroup.Runtime))))
                                 .Where(tuple => tuple.Item2?.AssetPaths != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileNameWithoutExtension(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver..ctor] Managed assembly map includes: {string.Join(",", managedAssemblyMap.Keys.Select(k => $"'{k}'").OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}"));

            unmanagedAssemblyCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            unmanagedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.NativeLibraryGroups?.Count > 0)
                                 .Select(lib => Tuple.Create(lib, lib.NativeLibraryGroups.FirstOrDefault(libGroup => compatibleRuntimes.Contains(libGroup.Runtime))))
                                 .Where(tuple => tuple.Item2?.AssetPaths != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileName(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver..ctor] Unmanaged assembly map includes: {string.Join(",", unmanagedAssemblyMap.Keys.Select(k => $"'{k}'").OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}"));

            Default.Resolving += OnResolving;
        }

        /// <inheritdoc/>
        public void Dispose()
            => Default.Resolving -= OnResolving;

        static IEnumerable<string> GetUnmanagedDllFormats()
        {
            yield return "{0}";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                yield return "{0}.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                yield return "lib{0}.dylib";
                yield return "{0}.dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                yield return "lib{0}.so";
                yield return "{0}.so";
            }
        }

        /// <inheritdoc/>
        protected override Assembly Load(AssemblyName assemblyName)
            => Default.LoadFromAssemblyName(assemblyName);

        /// <inheritdoc/>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var result = base.LoadUnmanagedDll(unmanagedDllName);
            if (result != default)
                return result;

            if (!unmanagedAssemblyCache.TryGetValue(unmanagedDllName, out var resolvedAssemblyPath))
            {
                if (internalDiagnosticsMessageSink != null)
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.LoadUnmanagedDll] Resolving '{unmanagedDllName}'"));

                resolvedAssemblyPath = ResolveUnmanagedAssembly(unmanagedDllName);
                unmanagedAssemblyCache[unmanagedDllName] = resolvedAssemblyPath;

                if (internalDiagnosticsMessageSink != null)
                {
                    if (resolvedAssemblyPath == null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.LoadUnmanagedDll] Resolution failed, passed down to next resolver"));
                    else
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.LoadUnmanagedDll] Successful: '{resolvedAssemblyPath}'"));
                }
            }

            return resolvedAssemblyPath != null ? LoadUnmanagedDllFromPath(resolvedAssemblyPath) : default;
        }

        Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            if (!managedAssemblyCache.TryGetValue(name.Name, out var result))
            {
                if (internalDiagnosticsMessageSink != null)
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.OnResolving] Resolving '{name.Name}'"));

                var tupleResult = ResolveManagedAssembly(name.Name);
                var resolvedAssemblyPath = tupleResult.Item1;
                result = tupleResult.Item2;
                managedAssemblyCache[name.Name] = result;

                if (internalDiagnosticsMessageSink != null)
                {
                    if (result == null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.OnResolving] Resolution failed, passed down to next resolver"));
                    else
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.OnResolving] Successful: '{resolvedAssemblyPath}'"));
                }
            }

            return result;
        }

        Tuple<string, Assembly> ResolveManagedAssembly(string assemblyName)
        {
            // Try to find dependency in the local folder
            var assemblyPath = Path.Combine(assemblyFolder, assemblyName);

            foreach (var extension in new[] { ".dll", ".exe" })
                try
                {
                    var resolvedAssemblyPath = assemblyPath + extension;
                    if (File.Exists(resolvedAssemblyPath))
                    {
                        var assembly = LoadFromAssemblyPath(resolvedAssemblyPath);
                        if (assembly != null)
                            return Tuple.Create(resolvedAssemblyPath, assembly);
                    }
                }
                catch { }

            // Try to find dependency from .deps.json
            if (managedAssemblyMap.TryGetValue(assemblyName, out var libraryTuple))
            {
                var library = libraryTuple.Item1;
                var assetGroup = libraryTuple.Item2;
                var wrapper = new CompilationLibrary(library.Type, library.Name, library.Version, library.Hash,
                                                     assetGroup.AssetPaths, library.Dependencies, library.Serviceable);

                var assemblies = new List<string>();
                if (assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                {
                    var resolvedAssemblyPath = assemblies.FirstOrDefault(a => string.Equals(assemblyName, Path.GetFileNameWithoutExtension(a), StringComparison.OrdinalIgnoreCase));
                    if (resolvedAssemblyPath != null)
                    {
                        resolvedAssemblyPath = Path.GetFullPath(resolvedAssemblyPath);

                        var assembly = LoadFromAssemblyPath(resolvedAssemblyPath);
                        if (assembly != null)
                            return Tuple.Create(resolvedAssemblyPath, assembly);

                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.ResolveManagedAssembly] Found assembly path '{resolvedAssemblyPath}' but the assembly would not load"));
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.ResolveManagedAssembly] Found a resolved path, but could not map a filename in [{string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                }
                else
                {
                    if (internalDiagnosticsMessageSink != null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.ResolveManagedAssembly] Found in dependency map, but unable to resolve a path in [{string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                }
            }

            return ManagedAssemblyNotFound;
        }

        string ResolveUnmanagedAssembly(string unmanagedDllName)
        {
            foreach (var format in UnmanagedDllFormats)
            {
                var formattedUnmanagedDllName = string.Format(format, unmanagedDllName);

                if (unmanagedAssemblyMap.TryGetValue(formattedUnmanagedDllName, out var libraryTuple))
                {
                    var library = libraryTuple.Item1;
                    var assetGroup = libraryTuple.Item2;
                    var wrapper = new CompilationLibrary(library.Type, library.Name, library.Version, library.Hash, assetGroup.AssetPaths, library.Dependencies, library.Serviceable);

                    var assemblies = new List<string>();
                    if (assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                    {
                        var resolvedAssemblyPath = assemblies.FirstOrDefault(a => string.Equals(formattedUnmanagedDllName, Path.GetFileName(a), StringComparison.OrdinalIgnoreCase));
                        if (resolvedAssemblyPath != null)
                            return Path.GetFullPath(resolvedAssemblyPath);

                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.ResolveUnmanagedDll] Found a resolved path, but could not map a filename in [{string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[NetCoreAssemblyDependencyResolver.ResolveUnmanagedDll] Found in dependency map, but unable to resolve a path in [{string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                }
            }

            return null;
        }
    }
}

#endif
