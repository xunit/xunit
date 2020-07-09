#nullable enable

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

#if !NETSTANDARD
using System.IO;
#endif

/// <summary>
/// Guard class, used for guard clauses and argument validation
/// </summary>
static class Guard
{
    /// <summary/>
    public static void ArgumentNotNull(string argName, [NotNull] object? argValue)
    {
        if (argValue == null)
            throw new ArgumentNullException(argName);
    }

    /// <summary/>
    public static T ArgumentNotNull<T>(string argName, [NotNull] T? argValue)
        where T : class
    {
        if (argValue == null)
            throw new ArgumentNullException(argName);

        return argValue;
    }

    /// <summary/>
    public static T ArgumentNotNullOrEmpty<T>(string argName, [NotNull] T? argValue)
        where T : class, IEnumerable
    {
        ArgumentNotNull(argName, argValue);

        if (!argValue.GetEnumerator().MoveNext())
            throw new ArgumentException("Argument was empty", argName);

        return argValue;
    }

    /// <summary/>
    public static void ArgumentValid(string argName, string message, bool test)
    {
        if (!test)
            throw new ArgumentException(message, argName);
    }

    public static T ArgumentValidNotNull<T>(string argName, string message, [NotNull] T? testValue)
        where T : class
    {
        if (testValue == null)
            throw new ArgumentException(message, argName);

        return testValue;
    }

#if !XUNIT_FRAMEWORK
    /// <summary/>
    public static void FileExists(string argName, [NotNull] string? fileName)
    {
        ArgumentNotNullOrEmpty(argName, fileName);
#if !NETSTANDARD
        ArgumentValid(argName, $"File not found: {fileName}", File.Exists(fileName));
#endif
    }
#endif

    /// <summary/>
    public static T NotNull<T>(string message, [NotNull] T? value)
        where T : class
    {
        if (value == null)
            throw new InvalidOperationException(message);

        return value;
    }
}
