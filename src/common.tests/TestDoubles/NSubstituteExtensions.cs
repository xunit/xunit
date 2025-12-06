#if !XUNIT_AOT

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NSubstitute.Core;

public static class NSubstituteExtensions
{
	public static CallInfo Captured<T>(
		this T substitute,
		Expression<Action<T>> expr)
			where T : notnull
	{
		return Captured(substitute, 0, expr);
	}

	public static CallInfo Captured<T>(
		this T substitute,
		int callNumber,
		Expression<Action<T>> expr)
			where T : notnull
	{
		var router = SubstitutionContext.Current.GetCallRouterFor(substitute);

		var method = ExtractMethodInfo(expr);

		var call =
			router
				.ReceivedCalls()
				.Where(c => c.GetMethodInfo() == method)
				.ElementAtOrDefault(callNumber)
					?? throw new Exception("Cannot find matching call.");

		var methodParameters = call.GetParameterInfos();
		var arguments = new Argument[methodParameters.Length];
		var argumentValues = call.GetArguments();

		for (var i = 0; i < arguments.Length; i++)
		{
			var methodParameter = methodParameters[i];
			var argumentIndex = i;

#pragma warning disable CS0618
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

	public static WhenCalledAny<T> WhenAny<T>(
		this T substitute,
		Action<T> substituteCall)
			where T : class
	{
		var context = SubstitutionContext.Current;
		return new WhenCalledAny<T>(context, substitute, substituteCall, MatchArgs.Any);
	}

	public class WhenCalledAny<T>(
		ISubstitutionContext context,
		T substitute,
		Action<T> call,
		MatchArgs matchArgs) :
			WhenCalled<T>(context, substitute, call, matchArgs)
				where T : class
	{
		public void Do<T1>(Action<T1> callbackWithArguments) =>
			Do(callInfo => callbackWithArguments((T1)callInfo[0]));

		public void Do<T1, T2>(Action<T1, T2> callbackWithArguments) =>
			Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1]));

		public void Do<T1, T2, T3>(Action<T1, T2, T3> callbackWithArguments) =>
			Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2]));

		public void Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callbackWithArguments) =>
			Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3]));

		public void Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callbackWithArguments) =>
			Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3], (T5)callInfo[4]));

		public void Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callbackWithArguments) =>
			Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3], (T5)callInfo[4], (T6)callInfo[5]));
	}
}

#endif
