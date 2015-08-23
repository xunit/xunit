using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Xunit.Sdk
{
    static class ExecutionContextHelper
    {
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
