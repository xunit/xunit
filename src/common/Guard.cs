using System;
using System.Collections;
using System.Globalization;

#if !XUNIT_FRAMEWORK && !NETSTANDARD
using System.IO;
#endif

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
    public static void ArgumentValid(string argName, bool test, string message)
    {
        if (!test)
            throw new ArgumentException(message, argName);
    }

    /// <summary/>
    public static void ArgumentValid(string argName, bool test, string messageFormat, params object[] args)
    {
        if (!test)
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, messageFormat, args), argName);
    }

#if !XUNIT_FRAMEWORK
    /// <summary/>
    public static void FileExists(string argName, string fileName)
    {
        ArgumentNotNullOrEmpty(argName, fileName);
#if !NETSTANDARD
        ArgumentValid(argName, File.Exists(fileName), "File not found: {0}", fileName);
#endif
    }
#endif
}
