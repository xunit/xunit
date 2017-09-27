#if NETCOREAPP1_0

// Adapted from https://github.com/dotnet/core-setup/blob/652b680dff6b1afb9cd26cc3c2e883a664c209fd/src/managed/Microsoft.Extensions.DependencyModel/Resolution/PackageCompilationAssemblyResolver.cs

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Xunit
{
    class XunitPackageCompilationAssemblyResolver : ICompilationAssemblyResolver
    {
        readonly List<string> nugetPackageDirectories = GetDefaultProbeDirectories();

        static List<string> GetDefaultProbeDirectories() =>
            GetDefaultProbeDirectories(RuntimeEnvironment.OperatingSystemPlatform);

        static List<string> GetDefaultProbeDirectories(Platform osPlatform)
        {
            var results = new List<string>();

            // The fact that the original code would only use PROBING_DIRECTORIES was causing failures to load
            // referenced packages, so instead we'll use PROBING_DIRECTORIES as a supplemental folder.
            var probeDirectories = AppContext.GetData("PROBING_DIRECTORIES");
            var listOfDirectories = probeDirectories as string;

            if (!string.IsNullOrEmpty(listOfDirectories))
                results.AddRange(listOfDirectories.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries));

            // Allow the user to override the default location of NuGet packages
            var packageDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrEmpty(packageDirectory))
                results.Add(packageDirectory);
            else
            {
                string basePath;
                if (osPlatform == Platform.Windows)
                    basePath = Environment.GetEnvironmentVariable("USERPROFILE");
                else
                    basePath = Environment.GetEnvironmentVariable("HOME");

                if (!string.IsNullOrEmpty(basePath))
                    results.Add(Path.Combine(basePath, ".nuget", "packages"));
            }

            return results;
        }

        public bool TryResolveAssemblyPaths(CompilationLibrary library, List<string> assemblies)
        {
            if (nugetPackageDirectories.Count == 0 || !string.Equals(library.Type, "package", StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (var directory in nugetPackageDirectories)
                if (ResolverUtils.TryResolvePackagePath(library, directory, out var packagePath))
                    if (TryResolveFromPackagePath(library, packagePath, out var fullPathsFromPackage))
                    {
                        assemblies.AddRange(fullPathsFromPackage);
                        return true;
                    }

            return false;
        }

        static bool TryResolveFromPackagePath(CompilationLibrary library, string basePath, out IEnumerable<string> results)
        {
            var paths = new List<string>();

            foreach (var assembly in library.Assemblies)
            {
                if (!ResolverUtils.TryResolveAssemblyFile(basePath, assembly, out var fullName))
                {
                    // if one of the files can't be found, skip this package path completely.
                    // there are package paths that don't include all of the "ref" assemblies 
                    // (ex. ones created by 'dotnet store')
                    results = null;
                    return false;
                }

                paths.Add(fullName);
            }

            results = paths;
            return true;
        }
    }
}

#endif
