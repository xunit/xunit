using System.Diagnostics.CodeAnalysis;

namespace System.Threading
{
    // Since WPA81 doesn't have threads or execution context, we just simulate the APIs we need
    [SuppressMessage("Language Usage Opportunities", "RECS0014:If all fields, properties and methods members are static, the class can be made static.", Justification = "This is a simulation of a CLR type, so it cannot be static")]
    internal class ExecutionContext
    {
        public static ExecutionContext Capture()
        {
            return null;
        }

        public static void Run(ExecutionContext context, Action<object> callback, object state)
        {
            callback(state);
        }
    }
}
