using System;
using System.Threading;

namespace Xunit.Sdk
{
    static class ExecutionContextHelper
    {
        public static bool IsSupported
            => true;

        public static object Capture()
        {
            return ExecutionContext.Capture();
        }

        public static void Run(object context, Action<object> action)
        {
            var callback = new ContextCallback(action);
            ExecutionContext.Run((ExecutionContext)context, callback, null);
        }
    }
}
