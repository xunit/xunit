using System;
using System.Collections.Generic;

public static class ArgParser
{
    // Simple argument parser that doesn't do much validation, since we will rely on the inner
    // runners to do the argument validation.
    public static Dictionary<string, List<string>> Parse(string[] args)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var idx = 0;

        while (idx < args.Length)
        {
            var arg = args[idx++];
            if (!arg.StartsWith("-"))
                throw new ArgumentException($"Unexpected parameter: {arg}");

            if (!result.TryGetValue(arg, out var values))
            {
                values = new List<string>();
                result.Add(arg, values);
            }

            if (idx < args.Length && !args[idx].StartsWith("-"))
                values.Add(args[idx++]);
            else
                values.Add(null);
        }

        return result;
    }
}
