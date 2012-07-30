using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

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
    [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "obj", Justification = "No can do.")]
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method may not be called by all users of Guard.")]
    public static void ArgumentNotNullOrEmpty(string argName, IEnumerable argValue)
    {
        ArgumentNotNull(argName, argValue);

        foreach (object obj in argValue)
            return;

        throw new ArgumentException("Argument was empty", argName);
    }

    /// <summary/>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method may not be called by all users of Guard.")]
    public static void ArgumentValid(string argName, string message, bool test)
    {
        if (!test)
            throw new ArgumentException(message, argName);
    }
}