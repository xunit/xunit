#if !DNXCORE50

using System;
using System.IO;
using System.Reflection;

internal static class AssemblyExtensions
{
    public static string GetLocalCodeBase(this Assembly assembly)
    {
        string codeBase = assembly.CodeBase;
        if (codeBase == null)
            return null;

        if (!codeBase.StartsWith("file:///"))
            throw new ArgumentException(string.Format("Code base {0} in wrong format; must start with file:///", codeBase), "assembly");

        codeBase = codeBase.Substring(8);
        if (Path.DirectorySeparatorChar == '/')
            return "/" + codeBase;

        return codeBase.Replace('/', Path.DirectorySeparatorChar);
    }
}

#endif
