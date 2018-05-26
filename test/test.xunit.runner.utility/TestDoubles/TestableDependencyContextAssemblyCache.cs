#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Internal.Microsoft.DotNet.PlatformAbstractions;
using Internal.Microsoft.Extensions.DependencyModel;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

class TestableDependencyContextAssemblyCache : DependencyContextAssemblyCache
{
    static readonly string[] AnyRuntimes = new[] { "any", "base" };
    static readonly IEnumerable<Dependency> EmptyDependencies = Enumerable.Empty<Dependency>();
    static readonly IEnumerable<ResourceAssembly> EmptyResourceAssemblies = Enumerable.Empty<ResourceAssembly>();
    static readonly Dictionary<Platform, List<RuntimeFallbacks>> RuntimeGraphs = new Dictionary<Platform, List<RuntimeFallbacks>>
    {
        // These aren't required to be full lists, just known quanitities. The Darwin list is more complete because it's
        // used for the platform-dependent unit tests. The Windows and Linux lists are just full enough to test fallback logic.
        {
            Platform.Darwin,
            new List<RuntimeFallbacks>
            {
                new RuntimeFallbacks("osx.10.12-x64", "osx.10.12", "osx.10.11-x64", "osx.10.11", "osx.10.10-x64", "osx.10.10", "osx-x64", "osx", "unix-x64", "unix", "any", "base"),
                new RuntimeFallbacks("osx.10.12", "osx.10.11", "osx.10.10", "osx", "unix", "any", "base"),
                new RuntimeFallbacks("osx.10.11-x64", "osx.10.11", "osx.10.10-x64", "osx.10.10", "osx-x64", "osx", "unix-x64", "unix", "any", "base"),
                new RuntimeFallbacks("osx.10.11", "osx.10.10", "osx", "unix", "any", "base"),
                new RuntimeFallbacks("osx.10.10-x64", "osx.10.10", "osx-x64", "osx", "unix-x64", "unix", "any", "base"),
                new RuntimeFallbacks("osx.10.10", "osx", "unix", "any", "base"),
                new RuntimeFallbacks("osx-x64", "osx", "unix-x64", "unix", "any", "base"),
                new RuntimeFallbacks("osx", "unix", "any", "base"),
            }
        },
        {
            Platform.Linux,
            new List<RuntimeFallbacks>
            {
                new RuntimeFallbacks("ubuntu.16.04-x64", "ubuntu.16.04", "ubuntu-x64", "ubuntu", "linux-x64", "linux", "unix-x64", "unix", "any", "base"),
                new RuntimeFallbacks("ubuntu.16.04", "ubuntu", "linux", "unix", "any", "base"),
                new RuntimeFallbacks("linux-x64", "linux", "unix-x64", "unix", "any", "base"),
                new RuntimeFallbacks("linux", "unix", "any", "base"),
            }
        },
        {
            Platform.Windows,
            new List<RuntimeFallbacks>
            {
                new RuntimeFallbacks("win10-x64", "win10", "win8-x64", "win8", "win7-x64", "win7", "win", "any", "base"),
                new RuntimeFallbacks("win10", "win8", "win7", "win", "any", "base"),
            }
        },
        { Platform.Unknown, new List<RuntimeFallbacks>() }
    };

    readonly TestableDependencyContext dependencyContext;
    readonly SpyMessageSink<IDiagnosticMessage> internalDiagnosticsMessageSink;

    TestableDependencyContextAssemblyCache(TestableDependencyContext dependencyContext,
                                           SpyMessageSink<IDiagnosticMessage> internalDiagnosticsMessageSink,
                                           Platform operatingSystemPlatform,
                                           string currentRuntimeIdentifier,
                                           IFileSystem fileSystem,
                                           string assemblyFolder = "/assembly/root")
        : base(assemblyFolder, dependencyContext, internalDiagnosticsMessageSink, operatingSystemPlatform, currentRuntimeIdentifier, fileSystem)
    {
        this.dependencyContext = dependencyContext;
        this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

        AssemblyFolder = Path.GetFullPath(assemblyFolder);

        FileSystem = fileSystem;
        FileSystem.File.Returns(Substitute.For<IFile>());
        FileSystem.Directory.Returns(Substitute.For<IDirectory>());
        FileSystem.Directory.Exists(Path.GetFullPath(AssemblyFolder)).Returns(true);

        OperatingSystemPlatform = operatingSystemPlatform;
    }

    public string AssemblyFolder { get; }
    public IFileSystem FileSystem { get; }
    public List<string> MissingFileNames = new List<string>();
    public Platform OperatingSystemPlatform { get; }
    public List<RuntimeFallbacks> RuntimeGraph => dependencyContext.InnerRuntimeGraph;
    public List<RuntimeLibrary> RuntimeLibraries => dependencyContext.InnerRuntimeLibraries;

    public List<string> GetAndClearDiagnosticMessages()
    {
        var result = internalDiagnosticsMessageSink.Messages.OfType<IDiagnosticMessage>().Select(_ => _.Message).ToList();
        internalDiagnosticsMessageSink.Messages.Clear();
        return result;
    }

    public Assembly LoadManagedDll(string assemblyName)
        => LoadManagedDll(assemblyName, location => new DummyAssembly(location));

    public IntPtr LoadUnmanagedLibrary(string unmanagedLibraryName)
        => LoadUnmanagedLibrary(unmanagedLibraryName, location => new IntPtr(42));

    public void MockAllLibrariesPresentInNuGetCache()
    {
        foreach (var runtimeLibrary in RuntimeLibraries)
        {
            var packagePath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, runtimeLibrary.Name.ToLowerInvariant(), runtimeLibrary.Version.ToLowerInvariant()));
            FileSystem.Directory.Exists(packagePath).Returns(true);

            foreach (var assetPath in runtimeLibrary.RuntimeAssemblyGroups.SelectMany(group => group.AssetPaths)
                              .Concat(runtimeLibrary.NativeLibraryGroups.SelectMany(group => group.AssetPaths)))
            {
                FileSystem.File.Exists(Path.GetFullPath(Path.Combine(packagePath, assetPath))).Returns(true);
            }
        }
    }

    public static TestableDependencyContextAssemblyCache Create(Platform platform = Platform.Windows, string runtime = "win10-x64", params string[] compatibleRuntimes)
    {
        if (compatibleRuntimes == null || compatibleRuntimes.Length == 0)
            compatibleRuntimes = AnyRuntimes;

        var runtimeAssemblyGroups = compatibleRuntimes.Select(r => new RuntimeAssetGroup(r, $"runtime/{r}/managed.ref1.dll", $"runtime/{r}/managed.ref2.dll")).ToList();
        var nativeLibraryGroups = new[]
        {
            new RuntimeAssetGroup("win", "native/win/dependency1.dll", "native/win/dependency2.dll"),
            new RuntimeAssetGroup("osx", "native/osx/dependency1.dylib", "native/osx/libdependency2.dylib"),
            new RuntimeAssetGroup("linux", "native/linux/dependency1.so", "native/linux/libdependency2.so"),
        };
        var runtimeLibraries = new List<RuntimeLibrary>
        {
            new RuntimeLibrary("package", "PackageName", "1.2.3.4", "abcdef1234567890", runtimeAssemblyGroups, nativeLibraryGroups, EmptyResourceAssemblies, EmptyDependencies, true),
        };

        var runtimeGraph = RuntimeGraphs[platform];

        return new TestableDependencyContextAssemblyCache(new TestableDependencyContext(runtimeLibraries, runtimeGraph),
                                                          new SpyMessageSink<IDiagnosticMessage>(),
                                                          platform,
                                                          runtime,
                                                          Substitute.For<IFileSystem>());
    }

    public static TestableDependencyContextAssemblyCache CreateEmpty()
        => new TestableDependencyContextAssemblyCache(new TestableDependencyContext(new List<RuntimeLibrary>(), new List<RuntimeFallbacks>()),
                                                      new SpyMessageSink<IDiagnosticMessage>(),
                                                      Platform.Unknown,
                                                      "unknown",
                                                      Substitute.For<IFileSystem>());
}

#endif
