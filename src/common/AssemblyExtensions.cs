#if !DOTNETCORE

using System;
using System.IO;
using System.Reflection;

static class AssemblyExtensions
{
    public static string GetLocalCodeBase(this Assembly assembly)
    {
        var codeBase = assembly.CodeBase;
        if (codeBase == null)
            return null;

        if (!codeBase.StartsWith("file:///", StringComparison.Ordinal))
            throw new ArgumentException($"Code base {codeBase} in wrong format; must start with file:///", nameof(assembly));

        codeBase = codeBase.Substring(8);
        if (Path.DirectorySeparatorChar == '/')
            return "/" + codeBase;

        return codeBase.Replace('/', Path.DirectorySeparatorChar);
    }
}

#endif
