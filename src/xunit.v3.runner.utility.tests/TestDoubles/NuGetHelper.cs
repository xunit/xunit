using System;
using System.IO;

static class NuGetHelper
{
    static NuGetHelper()
    {
        var homePath = Environment.GetEnvironmentVariable("USERPROFILE")
                    ?? Environment.GetEnvironmentVariable("HOME")
                    ?? throw new InvalidOperationException("Cannot find the home path for the current user");

        PackageCachePath = Path.Combine(homePath, ".nuget", "packages");
    }

    public static readonly string PackageCachePath;
}
