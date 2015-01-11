namespace System.Threading
{
    // Since WPA81 doesn't have threads or execution context, we just simulate the APIs we need
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
