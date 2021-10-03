using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;

public static class NSubstituteExtensions
{
    public static CallInfo Captured<T>(this T substitute, Expression<Action<T>> expr)
    {
        return Captured<T>(substitute, 0, expr);
    }

    public static CallInfo Captured<T>(this T substitute, int callNumber, Expression<Action<T>> expr)
    {
        var router = SubstitutionContext.Current.GetCallRouterFor(substitute);

        var method = ExtractMethodInfo(expr);

        var call = router.ReceivedCalls()
                         .Where(c => c.GetMethodInfo() == method)
                         .ElementAtOrDefault(callNumber);

        if (call == null)
            throw new Exception("Cannot find matching call.");

        var methodParameters = call.GetParameterInfos();
        var arguments = new Argument[methodParameters.Length];
        var argumentValues = call.GetArguments();

        for (int i = 0; i < arguments.Length; i++)
        {
            var methodParameter = methodParameters[i];
            var argumentIndex = i;

#pragma warning disable CS0618 // Type or member is obsolete
            arguments[i] = new Argument(methodParameter.ParameterType, () => argumentValues[argumentIndex], _ => { });
#pragma warning restore CS0618
        }

        return new CallInfo(arguments);
    }

    static MethodInfo ExtractMethodInfo<T>(Expression<Action<T>> expr)
    {
        if (expr.Body.NodeType == ExpressionType.Call)
            return ((MethodCallExpression)expr.Body).Method;

        throw new Exception("Cannot find method.");
    }

    public static void Returns<T>(this Task<T> instance, T value)
    {
        instance.Returns(Task.FromResult<T>(value));
    }

    public static void ReturnsForAnyArgs<T>(this Task<T> instance, T value)
    {
        instance.ReturnsForAnyArgs(Task.FromResult<T>(value));
    }

    public static WhenCalledAny<T> WhenAny<T>(this T substitute, Action<T> substituteCall) where T : class
    {
        var context = SubstitutionContext.Current;
        return new WhenCalledAny<T>(context, substitute, substituteCall, MatchArgs.Any);
    }

    public class WhenCalledAny<T> : WhenCalled<T>
        where T : class
    {
        public WhenCalledAny(ISubstitutionContext context, T substitute, Action<T> call, MatchArgs matchArgs)
            : base(context, substitute, call, matchArgs) { }

        public void Do<T1>(Action<T1> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0]));
        }

        public void Do<T1, T2>(Action<T1, T2> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1]));
        }

        public void Do<T1, T2, T3>(Action<T1, T2, T3> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2]));
        }

        public void Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3]));
        }

        public void Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3], (T5)callInfo[4]));
        }

        public void Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3], (T5)callInfo[4], (T6)callInfo[5]));
        }
    }
}
