#if NETFRAMEWORK || NETCOREAPP || NETSTANDARD2_0

using System;
using System.Globalization;
using System.IO;
using System.Reflection;

static class AssemblyExtensions
{
    public static string GetLocalCodeBase(this Assembly assembly)
    {
#pragma warning disable SYSLIB0012
        return GetLocalCodeBase(assembly?.CodeBase, Path.DirectorySeparatorChar);
#pragma warning restore SYSLIB0012
    }

    public static string GetLocalCodeBase(string codeBase, char directorySeparator)
    {
        if (codeBase == null)
            return null;

        if (!codeBase.StartsWith("file://", StringComparison.Ordinal))
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Codebase '{0}' is unsupported; must start with 'file://'.", codeBase), nameof(codeBase));

        // "file:///path" is a local path; "file://machine/path" is a UNC
        var localFile = codeBase.Length > 7 && codeBase[7] == '/';

        // POSIX-style directories
        if (directorySeparator == '/')
        {
            if (localFile)
                return codeBase.Substring(7);

            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UNC-style codebase '{0}' is not supported on POSIX-style file systems.", codeBase), nameof(codeBase));
        }

        // Windows-style directories
        if (directorySeparator == '\\')
        {
            codeBase = codeBase.Replace('/', '\\');

            if (localFile)
                return codeBase.Substring(8);

            return codeBase.Substring(5);
        }

        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown directory separator '{0}'; must be one of '/' or '\\'.", directorySeparator), nameof(directorySeparator));
    }
}

#endif
