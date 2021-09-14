#if NETCOREAPP

using System;
using System.IO;
using Internal.Microsoft.DotNet.PlatformAbstractions;
using NSubstitute;
using Xunit;

public static class DependencyContextAssemblyCacheTests
{
    public class Initialization
    {
        [Theory]
        [InlineData(Platform.Darwin,
                    "osx.10.10-x64",
                    "['osx.10.12-x64','osx.10.12','osx.10.11-x64','osx.10.11','osx.10.10-x64','osx.10.10','osx-x64','osx']",
                    "['osx.10.10-x64','osx.10.10','osx-x64','osx','unix-x64','unix','any','base','']",
                    "['dependency1.dylib','libdependency2.dylib']"
        )]
        [InlineData(Platform.Linux,
                    "ubuntu.16.04-x64",
                    "['ubuntu.16.04-x64','ubuntu.16.04','linux-x64','linux']",
                    "['ubuntu.16.04-x64','ubuntu.16.04','ubuntu-x64','ubuntu','linux-x64','linux','unix-x64','unix','any','base','']",
                    "['dependency1.so','libdependency2.so']"
        )]
        [InlineData(Platform.Windows,
                    "win10-x64",
                    "['win10-x64','win10']",
                    "['win10-x64','win10','win8-x64','win8','win7-x64','win7','win','any','base','']",
                    "['dependency1.dll','dependency2.dll']"
        )]
        internal void KnownRuntime(Platform platform, string runtime, string expectedRuntimeGraph, string expectedCompatibleRuntimes, string expectedUnmanagedMap)
        {
            var cache = TestableDependencyContextAssemblyCache.Create(platform, runtime);

            Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                message => Assert.StartsWith($"[XunitPackageCompilationAssemblyResolver.GetDefaultProbeDirectories] returns: [", message),
                message => Assert.Equal($"[DependencyContextAssemblyCache..ctor] Runtime graph: {expectedRuntimeGraph}", message),
                message => Assert.Equal($"[DependencyContextAssemblyCache..ctor] Compatible runtimes: {expectedCompatibleRuntimes}", message),
                message => Assert.Equal("[DependencyContextAssemblyCache..ctor] Managed assembly map: ['managed.ref1','managed.ref2']", message),
                message => Assert.Equal($"[DependencyContextAssemblyCache..ctor] Unmanaged assembly map: {expectedUnmanagedMap}", message)
            );
        }

        [Theory]
        [InlineData(Platform.Darwin,
                    "rush.21.12-x64",
                    "osx.10.12-x64",
                    "['osx.10.12-x64','osx.10.12','osx.10.11-x64','osx.10.11','osx.10.10-x64','osx.10.10','osx-x64','osx','unix-x64','unix','any','base','']")]
        [InlineData(Platform.Darwin,
                    "rush.21.12",
                    "osx.10.12",
                    "['osx.10.12','osx.10.11','osx.10.10','osx','unix','any','base','']")]
        [InlineData(Platform.Linux,
                    "rush.21.12-x64",
                    "linux-x64",
                    "['linux-x64','linux','unix-x64','unix','any','base','']")]
        [InlineData(Platform.Linux,
                    "rush.21.12",
                    "linux",
                    "['linux','unix','any','base','']")]
        [InlineData(Platform.Windows,
                    "rush.21.12-x64",
                    "win10-x64",
                    "['win10-x64','win10','win8-x64','win8','win7-x64','win7','win','any','base','']")]
        [InlineData(Platform.Windows,
                    "rush.21.12",
                    "win10",
                    "['win10','win8','win7','win','any','base','']")]
        internal void UnknownRuntime(Platform platform, string runtime, string expectedFallbackRuntime, string expectedCompatibleRuntimes)
        {
            var cache = TestableDependencyContextAssemblyCache.Create(platform, runtime);

            Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                message => { },  // NuGet path is same as known runtime
                message => { },  // Runtime graph is same as known runtime
                message => Assert.Equal($"[DependencyContextAssemblyCache.GetFallbackRuntime] Could not find runtime '{runtime}', falling back to '{expectedFallbackRuntime}'", message),
                message => Assert.Equal($"[DependencyContextAssemblyCache..ctor] Compatible runtimes: {expectedCompatibleRuntimes}", message),
                message => { },  // Managed assembly map is the same as known runtime
                message => { }   // Unmanaged assembly map is the same as known runtime
            );
        }
    }

    public class EmptyCache
    {
        [Fact]
        public void UnknownAssembly_ReturnsNull()
        {
            var cache = TestableDependencyContextAssemblyCache.CreateEmpty();
            cache.GetAndClearDiagnosticMessages();

            var result = cache.LoadManagedDll("foo");

            Assert.Null(result);
            Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                message => Assert.Equal("[DependencyContextAssemblyCache.LoadManagedDll] Resolution for 'foo' failed, passed down to next resolver", message)
            );
        }

        [Fact]
        public void LocalAssembly_LoadsFromOutputLocation()
        {
            var cache = TestableDependencyContextAssemblyCache.CreateEmpty();
            cache.GetAndClearDiagnosticMessages();
            var expectedPath = Path.GetFullPath(Path.Combine(cache.AssemblyFolder, "foo.dll"));
            cache.FileSystem.File.Exists(expectedPath).Returns(true);

            var result = cache.LoadManagedDll("foo");

            Assert.Equal(expectedPath, result.Location);
            Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved 'foo' to '{expectedPath}'", message)
            );
        }
    }

    public class ManagedAssembly
    {
        public class PlatformDependent
        {
            [Fact]
            public void WithNoSuitableRuntime()
            {
                var cache = TestableDependencyContextAssemblyCache.Create(Platform.Darwin, "osx.10.10-x64", "osx.10.12-x64");
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();

                var result = cache.LoadManagedDll("managed.ref1");

                Assert.Null(result);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal("[DependencyContextAssemblyCache.LoadManagedDll] Resolution for 'managed.ref1' failed, passed down to next resolver", message)
                );
            }

            [Theory]
            [InlineData("managed.ref1")]
            [InlineData("managed.ref2")]
            public void WithExactMatchRuntime(string assemblyName)
            {
                var cache = TestableDependencyContextAssemblyCache.Create(Platform.Darwin, "osx.10.12-x64", "osx.10.11-x64", "osx.10.12-x64");
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var expectedPath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, $"packagename/1.2.3.4/runtime/osx.10.12-x64/{assemblyName}.dll"));

                var result = cache.LoadManagedDll(assemblyName);

                Assert.NotNull(result);
                Assert.Equal(expectedPath, result.Location);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved '{assemblyName}' to '{expectedPath}'", message)
                );
            }

            [Fact]
            public void WithDownlevelRuntime()
            {
                var cache = TestableDependencyContextAssemblyCache.Create(Platform.Darwin, "osx.10.12-x64", "osx.10.11-x64");
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var expectedPath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, "packagename/1.2.3.4/runtime/osx.10.11-x64/managed.ref1.dll"));

                var result = cache.LoadManagedDll("managed.ref1");

                Assert.NotNull(result);
                Assert.Equal(expectedPath, result.Location);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved 'managed.ref1' to '{expectedPath}'", message)
                );
            }

            [Fact]
            public void PresentLocallyWithMatch()
            {
                var cache = TestableDependencyContextAssemblyCache.Create(Platform.Darwin, "osx.10.12-x64", "osx.10.12-x64");
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var expectedPath = Path.GetFullPath(Path.Combine(cache.AssemblyFolder, "managed.ref1.dll"));
                cache.FileSystem.File.Exists(expectedPath).Returns(true);

                var result = cache.LoadManagedDll("managed.ref1");

                Assert.NotNull(result);
                Assert.Equal(expectedPath, result.Location);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved 'managed.ref1' to '{expectedPath}'", message)
                );
            }

            [Fact]
            public void PresentLocallyWithoutMatch()
            {
                var cache = TestableDependencyContextAssemblyCache.Create(Platform.Darwin, "osx.10.10-x64", "osx.10.12-x64");
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var expectedPath = Path.GetFullPath(Path.Combine(cache.AssemblyFolder, "managed.ref1.dll"));
                cache.FileSystem.File.Exists(expectedPath).Returns(true);

                var result = cache.LoadManagedDll("managed.ref1");

                Assert.NotNull(result);
                Assert.Equal(expectedPath, result.Location);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved 'managed.ref1' to '{expectedPath}'", message)
                );
            }
        }

        public class PlatformIndependent
        {
            [Fact]
            public void NotInCache()
            {
                var cache = TestableDependencyContextAssemblyCache.Create();
                cache.GetAndClearDiagnosticMessages();

                var result = cache.LoadManagedDll("managed.ref1");

                Assert.Null(result);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal("[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving 'managed.ref1', found in dependency map, but unable to resolve a path in ['runtime/any/managed.ref1.dll','runtime/any/managed.ref2.dll']", message),
                    message => Assert.Equal("[DependencyContextAssemblyCache.LoadManagedDll] Resolution for 'managed.ref1' failed, passed down to next resolver", message)
                );
            }

            [Fact]
            public void WontLoad()
            {
                var cache = TestableDependencyContextAssemblyCache.Create();
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var expectedPath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, "packagename/1.2.3.4/runtime/any/managed.ref1.dll"));

                var result = cache.LoadManagedDll("managed.ref1", _ => null);

                Assert.Null(result);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Resolving 'managed.ref1', found assembly path '{expectedPath}' but the assembly would not load", message),
                    message => Assert.Equal("[DependencyContextAssemblyCache.LoadManagedDll] Resolution for 'managed.ref1' failed, passed down to next resolver", message)
                );
            }

            [Theory]
            [InlineData("runtime/any/managed.ref1.dll")]
            [InlineData("runtime/any/managed.ref2.dll")]
            public void InCache(string relativePath)
            {
                var cache = TestableDependencyContextAssemblyCache.Create();
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var fileName = Path.GetFileNameWithoutExtension(relativePath);
                var expectedPath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, "packagename/1.2.3.4", relativePath));

                var result = cache.LoadManagedDll(fileName);

                Assert.Equal(expectedPath, result.Location);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved '{fileName}' to '{expectedPath}'", message)
                );
            }

            [Fact]
            public void PresentLocally()
            {
                var cache = TestableDependencyContextAssemblyCache.Create();
                cache.GetAndClearDiagnosticMessages();
                cache.MockAllLibrariesPresentInNuGetCache();
                var expectedPath = Path.GetFullPath(Path.Combine(cache.AssemblyFolder, "managed.ref1.dll"));
                cache.FileSystem.File.Exists(expectedPath).Returns(true);

                var result = cache.LoadManagedDll("managed.ref1");

                Assert.Equal(expectedPath, result.Location);
                Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                    message => Assert.Equal($"[DependencyContextAssemblyCache.LoadManagedDll] Resolved 'managed.ref1' to '{expectedPath}'", message)
                );
            }
        }
    }

    public class UnmanagedLibrary
    {
        [Fact]
        public void NotInCache()
        {
            var cache = TestableDependencyContextAssemblyCache.Create();
            cache.GetAndClearDiagnosticMessages();

            var result = cache.LoadUnmanagedLibrary("dependency1");

            Assert.Equal(IntPtr.Zero, result);
            Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                message => Assert.Equal("[DependencyContextAssemblyCache.ResolveUnmanagedLibrary] Found in dependency map, but unable to resolve a path in ['native/win/dependency1.dll','native/win/dependency2.dll']", message),
                message => Assert.Equal("[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolution for 'dependency1' failed, passed down to next resolver", message)
            );
        }

        [Fact]
        public void WontLoad()
        {
            var cache = TestableDependencyContextAssemblyCache.Create();
            cache.GetAndClearDiagnosticMessages();
            cache.MockAllLibrariesPresentInNuGetCache();
            var expectedPath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, "packagename/1.2.3.4/native/win/dependency1.dll"));

            var result = cache.LoadUnmanagedLibrary("dependency1", _ => IntPtr.Zero);

            Assert.Collection(cache.GetAndClearDiagnosticMessages(),
                message => Assert.Equal($"[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolving 'dependency1', found assembly path '{expectedPath}' but the assembly would not load", message),
                message => Assert.Equal("[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolution for 'dependency1' failed, passed down to next resolver", message)
            );
        }

        [Theory]
        [InlineData(Platform.Darwin, "osx.10.12-x64", "dependency1", "native/osx/dependency1.dylib")]
        [InlineData(Platform.Darwin, "osx.10.12-x64", "dependency2", "native/osx/libdependency2.dylib")]
        [InlineData(Platform.Linux, "ubuntu.16.04-x64", "dependency1", "native/linux/dependency1.so")]
        [InlineData(Platform.Linux, "ubuntu.16.04-x64", "dependency2", "native/linux/libdependency2.so")]
        [InlineData(Platform.Windows, "win10-x64", "dependency1", "native/win/dependency1.dll")]
        [InlineData(Platform.Windows, "win10-x64", "dependency2", "native/win/dependency2.dll")]
        internal void InCache(Platform platform, string runtime, string unmanagedLibraryName, string relativePath)
        {
            var cache = TestableDependencyContextAssemblyCache.Create(platform, runtime);
            cache.GetAndClearDiagnosticMessages();
            cache.MockAllLibrariesPresentInNuGetCache();
            var expectedPath = Path.GetFullPath(Path.Combine(NuGetHelper.PackageCachePath, "packagename/1.2.3.4", relativePath));

            var result = cache.LoadUnmanagedLibrary(unmanagedLibraryName);

            Assert.NotEqual(IntPtr.Zero, result);
            var message = Assert.Single(cache.GetAndClearDiagnosticMessages());
            Assert.Equal($"[DependencyContextAssemblyCache.LoadUnmanagedLibrary] Resolved '{unmanagedLibraryName}' to '{expectedPath}'", message);
        }
    }
}

#endif
