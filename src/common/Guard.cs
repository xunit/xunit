using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>
/// Guard class, used for guard clauses and argument validation
/// </summary>
internal static class Guard
{
    /// <summary/>
    public static void ArgumentNotNull(string argName, object argValue)
    {
        if (argValue == null)
            throw new ArgumentNullException(argName);
    }

    /// <summary/>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method may not be called by all users of Guard.")]
    public static void ArgumentNotNullOrEmpty(string argName, IEnumerable argValue)
    {
        ArgumentNotNull(argName, argValue);

        if (!argValue.GetEnumerator().MoveNext())
            throw new ArgumentException("Argument was empty", argName);
    }

    /// <summary/>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method may not be called by all users of Guard.")]
    public static void ArgumentValid(string argName, string message, bool test)
    {
        if (!test)
            throw new ArgumentException(message, argName);
    }

#if !XUNIT_CORE_DLL
    /// <summary/>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method may not be called by all users of Guard.")]
    public static void FileExists(string argName, string fileName)
    {
#if !ANDROID && !ASPNET50 && !ASPNETCORE50
        Guard.ArgumentNotNullOrEmpty(argName, fileName);
        Guard.ArgumentValid("assemblyFileName",
                            String.Format("File not found: {0}", fileName),
                            File.Exists(fileName));
#endif
    }
#endif
}