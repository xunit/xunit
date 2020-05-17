#if NETFRAMEWORK

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

#else

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xunit.Sdk
{
    static class ExecutionContextHelper
    {
        static readonly object[] EmptyObjectArray = new object[0];

        static MethodInfo actionInvokeMethod;
        static Func<object> captureContext;
        static Type contextCallbackType;
        static volatile bool initialized;
        static Action<object, object> runOnContext;

        public static bool IsSupported
        {
            get
            {
                EnsureInitialized();

                return captureContext != null;
            }
        }

        public static object Capture()
        {
            EnsureInitialized();

            return captureContext();
        }

        static Delegate CreateDelegate(Action<object> action)
        {
            return actionInvokeMethod.CreateDelegate(contextCallbackType, action);
        }

        static void EnsureInitialized()
        {
            lock (EmptyObjectArray)
            {
                if (initialized)
                    return;

                try
                {
                    actionInvokeMethod = typeof(Action<object>).GetRuntimeMethod("Invoke", new[] { typeof(object) });
                    contextCallbackType = Type.GetType("System.Threading.ContextCallback");
                    var executionContextType = Type.GetType("System.Threading.ExecutionContext");

                    // Create a function which captures the execution context
                    var captureMethod = executionContextType.GetRuntimeMethod("Capture", new Type[0]);
                    var captureExpression = Expression.Call(captureMethod);
                    captureContext = Expression.Lambda<Func<object>>(captureExpression).Compile();

                    // Create a function which runs on the captured execution context
                    var runMethod = executionContextType.GetRuntimeMethod("Run", new[] { executionContextType, contextCallbackType, typeof(object) });
                    var contextArg = Expression.Parameter(typeof(object));
                    var callbackArg = Expression.Parameter(typeof(object));
                    var runExpression = Expression.Call(runMethod, Expression.Convert(contextArg, executionContextType), Expression.Convert(callbackArg, contextCallbackType), Expression.Constant(null, typeof(object)));
                    runOnContext = Expression.Lambda<Action<object, object>>(runExpression, contextArg, callbackArg).Compile();

                    // Verify that everything is callable without throwing
                    Action<object> action = _ => { };
                    var del = CreateDelegate(action);
                    var context = captureContext();
                    runOnContext(context, del);
                }
                catch
                {
                    captureContext = null;
                }

                initialized = true;
            }
        }

        public static void Run(object context, Action<object> action)
        {
            EnsureInitialized();

            runOnContext(context, CreateDelegate(action));
        }
    }
}

#endif
