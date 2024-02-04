#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Internal.Microsoft.DotNet.PlatformAbstractions;
using Internal.Microsoft.Extensions.DependencyModel;
using Xunit.Abstractions;
using RuntimeEnvironment = Internal.Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace Xunit
{
    class DependencyContextAssemblyCache
    {
        static readonly RuntimeFallbacks AnyAndBase = new RuntimeFallbacks("unknown", "any", "base");
        static readonly string[] ManagedAssemblyExtensions = { ".dll", ".exe" };
        static readonly Tuple<string, Assembly> ManagedAssemblyNotFound = new Tuple<string, Assembly>(null, null);
        static readonly Regex RuntimeIdRegex = new Regex(@"(?<os>[A-Za-z0-9]+)(\.(?<version>[0-9\.]+))?(?<arch>\-[A-Za-z0-9]+)?(?<extra>\-[A-Za-z0-9]+)?");

        readonly string assemblyFolder;
        readonly XunitPackageCompilationAssemblyResolver assemblyResolver;
        readonly string currentRuntimeIdentifier;
        readonly DependencyContext dependencyContext;
        readonly Lazy<string> fallbackRuntimeIdentifier;
        readonly IFileSystem fileSystem;
        readonly IMessageSink internalDiagnosticsMessageSink;
        readonly Dictionary<string, Assembly> managedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> managedAssemblyMap;
        readonly Platform operatingSystemPlatform;
        readonly string[] unmanagedDllFormats;
        readonly Dictionary<string, string> unmanagedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> unmanagedAssemblyMap;

        public DependencyContextAssemblyCache(string assemblyFolder,
                                              DependencyContext dependencyContext,
                                              IMessageSink internalDiagnosticsMessageSink,
                                              Platform? operatingSystemPlatform = null,
                                              string currentRuntimeIdentifier = null,
                                              IFileSystem fileSystem = null)
        {
            this.assemblyFolder = assemblyFolder;
            this.dependencyContext = dependencyContext;
            this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;
            this.operatingSystemPlatform = operatingSystemPlatform ?? RuntimeEnvironment.OperatingSystemPlatform;
            this.currentRuntimeIdentifier = currentRuntimeIdentifier ?? RuntimeEnvironment.GetRuntimeIdentifier();
            this.fileSystem = fileSystem ?? new FileSystemWrapper();

            fallbackRuntimeIdentifier = new Lazy<string>(() => GetFallbackRuntime(this.currentRuntimeIdentifier));
            assemblyResolver = new XunitPackageCompilationAssemblyResolver(internalDiagnosticsMessageSink, fileSystem);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(
                    new _DiagnosticMessage(
                        "[DependencyContextAssemblyCache..ctor] Runtime graph: [{0}]",
                        string.Join(",", dependencyContext.RuntimeGraph.Select(x => string.Format(CultureInfo.CurrentCulture, "'{0}'", x.Runtime)))
                    )
                );

            var compatibleRuntimes = GetCompatibleRuntimes(dependencyContext);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(
                    new _DiagnosticMessage(
                        "[DependencyContextAssemblyCache..ctor] Compatible runtimes: [{0}]",
                        string.Join(",", compatibleRuntimes.Select(x => string.Format(CultureInfo.CurrentCulture, "'{0}'", x)))
                    )
                );

            managedAssemblyCache = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            managedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.RuntimeAssemblyGroups?.Count > 0)
                                 .Select(lib => compatibleRuntimes.Select(runtime => Tuple.Create(lib, lib.RuntimeAssemblyGroups.FirstOrDefault(libGroup => string.Equals(libGroup.Runtime, runtime))))
                                                                  .FirstOrDefault(tuple => tuple.Item2?.AssetPaths != null))
                                 .Where(tuple => tuple != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null)
                                                                            .Select(path => Tuple.Create(Path.GetFileNameWithoutExtension(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(
                    new _DiagnosticMessage(
                        "[DependencyContextAssemblyCache..ctor] Managed assembly map: [{0}]",
                        string.Join(",", managedAssemblyMap.Keys.Select(k => string.Format(CultureInfo.CurrentCulture, "'{0}'", k)).OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                    )
                );

            unmanagedDllFormats = GetUnmanagedDllFormats().ToArray();
            unmanagedAssemblyCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            unmanagedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Select(lib => compatibleRuntimes.Select(runtime => Tuple.Create(lib, lib.NativeLibraryGroups.FirstOrDefault(libGroup => string.Equals(libGroup.Runtime, runtime))))
                                                                  .FirstOrDefault(tuple => tuple.Item2?.AssetPaths != null))
                                 .Where(tuple => tuple != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null)
                                                                            .Select(path => Tuple.Create(Path.GetFileName(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(
                    new _DiagnosticMessage(
                        "[DependencyContextAssemblyCache..ctor] Unmanaged assembly map: [{0}]",
                        string.Join(",", unmanagedAssemblyMap.Keys.Select(k => string.Format(CultureInfo.CurrentCulture, "'{0}'", k)).OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                    )
                );
        }

        List<string> GetCompatibleRuntimes(DependencyContext dependencyContext)
        {
            var result = new List<string>(GetFallbacks(dependencyContext.RuntimeGraph).Fallbacks);
            result.Insert(0, fallbackRuntimeIdentifier.IsValueCreated ? fallbackRuntimeIdentifier.Value : currentRuntimeIdentifier);
            result.Add(string.Empty);
            return result;
        }

        RuntimeFallbacks GetFallbacks(IReadOnlyList<RuntimeFallbacks> runtimeGraph)
            => runtimeGraph.FirstOrDefault(x => string.Equals(x.Runtime, currentRuntimeIdentifier, StringComparison.OrdinalIgnoreCase))
            ?? runtimeGraph.FirstOrDefault(x => string.Equals(x.Runtime, fallbackRuntimeIdentifier.Value, StringComparison.OrdinalIgnoreCase))
            ?? AnyAndBase;

        // This mimics the behavior of https://github.com/dotnet/core-setup/blob/863047f3ca16bada3ffc82493d1dbad6e560b80a/src/corehost/common/pal.h#L53-L73
        string GetFallbackRuntime(string runtime)
        {
            var match = RuntimeIdRegex.Match(runtime);
            var arch = match?.Groups?["arch"]?.Value;
            var result = default(string);

            switch (operatingSystemPlatform)
            {
                case Platform.Windows:
                    result = "win10" + arch;
                    break;

                case Platform.Darwin:
                    result = "osx.10.12" + arch;
                    break;

                case Platform.Linux:
                    result = "linux" + arch;
                    break;

                default:
                    result = "unknown";
                    break;
            }

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.GetFallbackRuntime] Could not find runtime '{0}', falling back to '{1}'", runtime, result));

            return result;
        }

        IEnumerable<string> GetUnmanagedDllFormats()
        {
            yield return "{0}";

            if (operatingSystemPlatform == Platform.Windows)
            {
                yield return "{0}.dll";
            }
            else if (operatingSystemPlatform == Platform.Darwin)
            {
                yield return "lib{0}.dylib";
                yield return "{0}.dylib";
            }
            else if (operatingSystemPlatform == Platform.Linux)
            {
                yield return "lib{0}.so";
                yield return "{0}.so";
            }
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
                        internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.LoadManagedDll] Resolution for '{0}' failed, passed down to next resolver", assemblyName));
                    else
                        internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.LoadManagedDll] Resolved '{0}' to '{1}'", assemblyName, resolvedAssemblyPath));
                }
            }

            return result;
        }

        public IntPtr LoadUnmanagedLibrary(string unmanagedLibraryName, Func<string, IntPtr> unmanagedAssemblyLoader)
        {
            var result = default(IntPtr);
            var needDiagnostics = false;

            if (!unmanagedAssemblyCache.TryGetValue(unmanagedLibraryName, out var resolvedAssemblyPath))
            {
                resolvedAssemblyPath = ResolveUnmanagedLibrary(unmanagedLibraryName);
                unmanagedAssemblyCache[unmanagedLibraryName] = resolvedAssemblyPath;
                needDiagnostics = true;
            }

            if (resolvedAssemblyPath != null)
                result = unmanagedAssemblyLoader(resolvedAssemblyPath);

            if (needDiagnostics && internalDiagnosticsMessageSink != null)
                if (result != default)
                    internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolved '{0}' to '{1}'", unmanagedLibraryName, resolvedAssemblyPath));
                else
                {
                    if (resolvedAssemblyPath != null)
                        internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolving '{0}', found assembly path '{1}' but the assembly would not load", unmanagedLibraryName, resolvedAssemblyPath));

                    internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolution for '{0}' failed, passed down to next resolver", unmanagedLibraryName));
                }

            return result;
        }

        Tuple<string, Assembly> ResolveManagedAssembly(string assemblyName, Func<string, Assembly> managedAssemblyLoader)
        {
            // Try to find dependency in the local folder
            var assemblyPath = Path.Combine(Path.GetFullPath(assemblyFolder), assemblyName);

            foreach (var extension in ManagedAssemblyExtensions)
                try
                {
                    var resolvedAssemblyPath = assemblyPath + extension;
                    if (fileSystem.File.Exists(resolvedAssemblyPath))
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
                                                     assetGroup.AssetPaths, library.Dependencies, library.Serviceable,
                                                     library.Path, library.HashPath);

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
                            internalDiagnosticsMessageSink.OnMessage(new _DiagnosticMessage("[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving '{0}', found assembly path '{1}' but the assembly would not load", assemblyName, resolvedAssemblyPath));
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(
                                new _DiagnosticMessage(
                                    "[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving '{0}', found a resolved path, but could not map a filename in [{1}]",
                                    assemblyName,
                                    string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => string.Format(CultureInfo.CurrentCulture, "'{0}'", k)))
                                )
                            );
                    }
                }
                else
                {
                    if (internalDiagnosticsMessageSink != null)
                        internalDiagnosticsMessageSink.OnMessage(
                            new _DiagnosticMessage(
                                "[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving '{0}', found in dependency map, but unable to resolve a path in [{1}]",
                                assemblyName,
                                string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => string.Format(CultureInfo.CurrentCulture, "'{0}'", k)))
                            )
                        );
                }
            }

            return ManagedAssemblyNotFound;
        }

        public string ResolveUnmanagedLibrary(string unmanagedLibraryName)
        {
            foreach (var format in unmanagedDllFormats)
            {
                var formattedUnmanagedDllName = string.Format(CultureInfo.InvariantCulture, format, unmanagedLibraryName);

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
                            internalDiagnosticsMessageSink.OnMessage(
                                new _DiagnosticMessage(
                                    "[DependencyContextAssemblyCache.ResolveUnmanagedLibrary] Found a resolved path, but could not map a filename in [{0}]",
                                    string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => string.Format(CultureInfo.CurrentCulture, "'{0}'", k)))
                                )
                            );
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(
                                new _DiagnosticMessage(
                                    "[DependencyContextAssemblyCache.ResolveUnmanagedLibrary] Found in dependency map, but unable to resolve a path in [{0}]",
                                    string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => string.Format(CultureInfo.CurrentCulture, "'{0}'", k)))
                                )
                            );
                    }
                }
            }

            return null;
        }
    }
}

#endif
