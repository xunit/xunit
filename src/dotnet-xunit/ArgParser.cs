using System;
using System.Collections.Generic;

public static class ArgParser
{
    // Simple argument parser that doesn't do much validation, since we will rely on the inner
    // runners to do the argument validation.
    public static Dictionary<string, string> Parse(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var idx = 0;

        while (idx < args.Length)
        {
            var arg = args[idx++];
            if (!arg.StartsWith("-"))
                throw new ArgumentException($"Unexpected parameter: {arg}");

            if (idx < args.Length && !args[idx].StartsWith("-"))
                result.Add(arg, args[idx++]);
            else
                result.Add(arg, null);
        }

        return result;
    }
}
