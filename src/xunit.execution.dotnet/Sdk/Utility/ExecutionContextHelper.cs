using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xunit.Sdk
{
    static class ExecutionContextHelper
    {
        static readonly object[] EmptyObjectArray = new object[0];

        static Func<object> captureContext;
        static Func<object, object> createDelegate;
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

        static void EnsureInitialized()
        {
            lock (EmptyObjectArray)
            {
                if (initialized)
                    return;

                try
                {
                    var contextCallbackType = Type.GetType("System.Threading.ContextCallback");
                    var executionContextType = Type.GetType("System.Threading.ExecutionContext");

                    // Create a function which can make the ContextCallback delegate out of Action<object>
                    var createDelegateMethod = typeof(Delegate).GetRuntimeMethod("CreateDelegate", new[] { typeof(Type), typeof(object), typeof(string) });
                    var actionArg = Expression.Parameter(typeof(object));
                    var createDelegateExpression = Expression.Call(createDelegateMethod, Expression.Constant(contextCallbackType), actionArg, Expression.Constant("Invoke"));
                    createDelegate = Expression.Lambda<Func<object, object>>(createDelegateExpression, actionArg).Compile();

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
                    var del = createDelegate(action);
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

            var callback = createDelegate(action);
            runOnContext(context, callback);
        }
    }
}
