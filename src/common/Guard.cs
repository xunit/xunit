#nullable enable

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
    public static void ArgumentNotNullOrEmpty(string argName, [NotNull] IEnumerable? argValue)
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
