using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Guard class, used for guard clauses and argument validation
    /// </summary>
    public static class Guard
    {
        /// <summary/>
        public static void ArgumentNotNull(string argName, object argValue)
        {
            if (argValue == null)
                throw new ArgumentNullException(argName);
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "This parameter is verified elsewhere.")]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "obj", Justification = "No can do.")]
        public static void ArgumentNotNullOrEmpty(string argName, IEnumerable argValue)
        {
            ArgumentNotNull(argName, argValue);

            foreach (object obj in argValue)
                return;

            throw new ArgumentException("Argument was empty", argName);
        }

        /// <summary/>
        public static void ArgumentValid(string argName, string message, bool test)
        {
            if (!test)
                throw new ArgumentException(message, argName);
        }
    }
}
