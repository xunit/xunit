using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xunit.Sdk
{
    static class ExecutionContextHelper
    {
        static readonly object[] EmptyObjectArray = new object[0];

        static Func<object> captureContext;
        static Func<object, Delegate> createDelegate;
        static Action<object, Delegate> runOnContext;

        static ExecutionContextHelper()
        {
            var contextCallbackType = Type.GetType("System.Threading.ContextCallback");
            if (contextCallbackType == null)
                throw new InvalidOperationException("Could not find type: System.Threading.ContextCallback");

            var executionContextType = Type.GetType("System.Threading.ExecutionContext");
            if (executionContextType == null)
                throw new InvalidOperationException("Could not find type: System.Threading.ExecutionContext");

            // Create a function which can make the ContextCallback delegate out of Action<object>
            var createDelegateMethod = typeof(Delegate).GetRuntimeMethod("CreateDelegate", new[] { typeof(Type), typeof(object), typeof(string) });
            if (createDelegateMethod == null)
                throw new InvalidOperationException("Could not find method: System.Delegate.CreateDelegate");

            var actionArg = Expression.Parameter(typeof(object));
            var createDelegateExpression = Expression.Call(createDelegateMethod, Expression.Constant(contextCallbackType), actionArg, Expression.Constant("Invoke"));
            createDelegate = Expression.Lambda<Func<object, Delegate>>(createDelegateExpression, actionArg).Compile();

            // Create a function which captures the execution context
            var captureMethod = executionContextType.GetRuntimeMethod("Capture", new Type[0]);
            if (captureMethod == null)
                throw new InvalidOperationException("Could not find method: System.Threading.ExecutionContext.Capture");

            var captureExpression = Expression.Call(captureMethod);
            captureContext = Expression.Lambda<Func<object>>(captureExpression).Compile();

            // Create a function which runs on the captured execution context
            var runMethod = executionContextType.GetRuntimeMethod("Run", new[] { executionContextType, contextCallbackType, typeof(object) });
            if (runMethod == null)
                throw new InvalidOperationException("Could not find method: System.Threading.ExecutionContext.Run");

            var contextArg = Expression.Parameter(typeof(object));
            var callbackArg = Expression.Parameter(typeof(Delegate));
            var runExpression = Expression.Call(runMethod, Expression.Convert(contextArg, executionContextType), Expression.Convert(callbackArg, contextCallbackType), Expression.Constant(null, typeof(object)));
            runOnContext = Expression.Lambda<Action<object, Delegate>>(runExpression, contextArg, callbackArg).Compile();
        }

        public static object Capture()
        {
            return captureContext();
        }

        public static void Run(object context, Action<object> action)
        {
            var callback = createDelegate(action);
            runOnContext(context, callback);
        }
    }
}
