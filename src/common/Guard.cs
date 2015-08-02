using System;
using System.Collections;
using System.IO;

/// <summary>
/// Guard class, used for guard clauses and argument validation
/// </summary>
static class Guard
{
    /// <summary/>
    public static void ArgumentNotNull(string argName, object argValue)
    {
        if (argValue == null)
            throw new ArgumentNullException(argName);
    }

    /// <summary/>
    public static void ArgumentNotNullOrEmpty(string argName, IEnumerable argValue)
    {
        ArgumentNotNull(argName, argValue);

        if (!argValue.GetEnumerator().MoveNext())
            throw new ArgumentException("Argument was empty", argName);
    }

    /// <summary/>
    public static void ArgumentValid(string argName, string message, bool test)
    {
        if (!test)
            throw new ArgumentException(message, argName);
    }

#if !XUNIT_CORE_DLL
    /// <summary/>
    public static void FileExists(string argName, string fileName)
    {
#if !ANDROID && !DOTNETCORE
        Guard.ArgumentNotNullOrEmpty(argName, fileName);
        Guard.ArgumentValid("assemblyFileName",
                            $"File not found: {fileName}",
                            File.Exists(fileName));
#endif
    }
#endif
}