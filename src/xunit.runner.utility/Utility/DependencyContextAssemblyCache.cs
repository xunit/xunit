#if NET452 || NETCOREAPP1_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Internal.Microsoft.Extensions.DependencyModel;
using Xunit.Abstractions;
using RuntimeEnvironment = Internal.Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace Xunit
{
    class DependencyContextAssemblyCache
    {
        static readonly string[] ManagedAssemblyExtensions = new[] { ".dll", ".exe" };
        static readonly Tuple<string, Assembly> ManagedAssemblyNotFound = new Tuple<string, Assembly>(null, null);

        readonly string assemblyFolder;
        readonly XunitPackageCompilationAssemblyResolver assemblyResolver;
        readonly DependencyContext dependencyContext;
        readonly IMessageSink internalDiagnosticsMessageSink;
        readonly Dictionary<string, Assembly> managedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> managedAssemblyMap;

        public DependencyContextAssemblyCache(string assemblyFolder,
                                              DependencyContext dependencyContext,
                                              IMessageSink internalDiagnosticsMessageSink)
        {
            this.assemblyFolder = assemblyFolder;
            this.dependencyContext = dependencyContext;
            this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

            assemblyResolver = new XunitPackageCompilationAssemblyResolver(internalDiagnosticsMessageSink);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Runtime graph: [{string.Join(",", dependencyContext.RuntimeGraph.Select(x => $"'{x.Runtime}'"))}]"));

            var compatibleRuntimes = default(HashSet<string>);
            var currentRuntime = RuntimeEnvironment.GetRuntimeIdentifier();
            var fallbacks = dependencyContext.RuntimeGraph.FirstOrDefault(x => string.Equals(x.Runtime, currentRuntime, StringComparison.OrdinalIgnoreCase));
            if (fallbacks != null)
                compatibleRuntimes = new HashSet<string>(fallbacks.Fallbacks, StringComparer.OrdinalIgnoreCase);
            else
                compatibleRuntimes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            compatibleRuntimes.Add(currentRuntime);
            compatibleRuntimes.Add(string.Empty);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Compatible runtimes: [{string.Join(",", compatibleRuntimes.Select(x => $"'{x}'"))}]"));

            managedAssemblyCache = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            managedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.RuntimeAssemblyGroups?.Count > 0)
                                 .Select(lib => Tuple.Create(lib, lib.RuntimeAssemblyGroups.FirstOrDefault(libGroup => compatibleRuntimes.Contains(libGroup.Runtime))))
                                 .Where(tuple => tuple.Item2?.AssetPaths != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileNameWithoutExtension(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Managed assembly map includes: {string.Join(",", managedAssemblyMap.Keys.Select(k => $"'{k}'").OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}"));

#if NETCOREAPP1_0
            unmanagedAssemblyCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            unmanagedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.NativeLibraryGroups?.Count > 0)
                                 .Select(lib => Tuple.Create(lib, lib.NativeLibraryGroups.FirstOrDefault(libGroup => compatibleRuntimes.Contains(libGroup.Runtime))))
                                 .Where(tuple => tuple.Item2?.AssetPaths != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileName(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Unmanaged assembly map includes: {string.Join(",", unmanagedAssemblyMap.Keys.Select(k => $"'{k}'").OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}"));
#endif
        }

        public Assembly LoadManagedDll(string assemblyName, Func<string, Assembly> managedAssemblyLoader)
        {
            if (!managedAssemblyCache.TryGetValue(assemblyName, out var result))
            {
                var tupleResult = ResolveManagedAssembly(assemblyName, managedAssemblyLoader);
                var resolvedAssemblyPath = tupleResult.Item1;
                result = tupleResult.Item2;
                managedAssemblyCache[assemblyName] = result;

                if (internalDiagnosticsMessageSink != null)
                {
                    if (result == null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadManagedDll] Resolution for '{assemblyName}' failed, passed down to next resolver"));
                    else
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved '{assemblyName}' to '{resolvedAssemblyPath}'"));
                }
            }

            return result;
        }

        Tuple<string, Assembly> ResolveManagedAssembly(string assemblyName, Func<string, Assembly> managedAssemblyLoader)
        {
            // Try to find dependency in the local folder
            var assemblyPath = Path.Combine(assemblyFolder, assemblyName);

            foreach (var extension in ManagedAssemblyExtensions)
                try
                {
                    var resolvedAssemblyPath = assemblyPath + extension;
                    if (File.Exists(resolvedAssemblyPath))
                    {
                        var assembly = managedAssemblyLoader(resolvedAssemblyPath);
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

                        var assembly = managedAssemblyLoader(resolvedAssemblyPath);
                        if (assembly != null)
                            return Tuple.Create(resolvedAssemblyPath, assembly);

                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving '{assemblyName}', found assembly path '{resolvedAssemblyPath}' but the assembly would not load"));
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving '{assemblyName}', found a resolved path, but could not map a filename in [{string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                }
                else
                {
                    if (internalDiagnosticsMessageSink != null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving '{assemblyName}', found in dependency map, but unable to resolve a path in [{string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                }
            }

            return ManagedAssemblyNotFound;
        }

#if NETCOREAPP1_0
        // Unmanaged DLL support

        static readonly string[] UnmanagedDllFormats = GetUnmanagedDllFormats().ToArray();

        readonly Dictionary<string, string> unmanagedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> unmanagedAssemblyMap;

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

        public IntPtr LoadUnmanagedDll(string unmanagedDllName, Func<string, IntPtr> unmanagedAssemblyLoader)
        {
            if (!unmanagedAssemblyCache.TryGetValue(unmanagedDllName, out var resolvedAssemblyPath))
            {
                resolvedAssemblyPath = ResolveUnmanagedAssembly(unmanagedDllName);
                unmanagedAssemblyCache[unmanagedDllName] = resolvedAssemblyPath;

                if (internalDiagnosticsMessageSink != null)
                {
                    if (resolvedAssemblyPath == null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadManagedDll] Resolution for '{unmanagedDllName}' failed, passed down to next resolver"));
                    else
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved '{unmanagedDllName}' to '{resolvedAssemblyPath}'"));
                }
            }

            return resolvedAssemblyPath != null ? unmanagedAssemblyLoader(resolvedAssemblyPath) : default;
        }

        public string ResolveUnmanagedAssembly(string unmanagedDllName)
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
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveUnmanagedDll] Found a resolved path, but could not map a filename in [{string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveUnmanagedDll] Found in dependency map, but unable to resolve a path in [{string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                }
            }

            return null;
        }
#endif
    }
}

#endif
