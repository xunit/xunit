#if NETFRAMEWORK || NETCOREAPP || NETSTANDARD2_0

using System;
using System.IO;
using System.Reflection;

static class AssemblyExtensions
{
    public static string GetLocalCodeBase(this Assembly assembly)
    {
        return GetLocalCodeBase(assembly?.CodeBase, Path.DirectorySeparatorChar);
    }

    public static string GetLocalCodeBase(string codeBase, char directorySeparator)
    {
        if (codeBase == null)
            return null;

        if (!codeBase.StartsWith("file://", StringComparison.Ordinal))
            throw new ArgumentException($"Codebase '{codeBase}' is unsupported; must start with 'file://'.", nameof(codeBase));

        // "file:///path" is a local path; "file://machine/path" is a UNC
        var localFile = codeBase.Length > 7 && codeBase[7] == '/';

        // POSIX-style directories
        if (directorySeparator == '/')
        {
            if (localFile)
                return codeBase.Substring(7);

            throw new ArgumentException($"UNC-style codebase '{codeBase}' is not supported on POSIX-style file systems.", nameof(codeBase));
        }

        // Windows-style directories
        if (directorySeparator == '\\')
        {
            codeBase = codeBase.Replace('/', '\\');

            if (localFile)
                return codeBase.Substring(8);

            return codeBase.Substring(5);
        }

        throw new ArgumentException($"Unknown directory separator '{directorySeparator}'; must be one of '/' or '\\'.", nameof(directorySeparator));
    }
}

#endif
