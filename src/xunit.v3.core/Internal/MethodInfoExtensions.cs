using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Xunit.Internal;

static class MethodInfoExtensions
{
	public static bool IsAsync(this MethodInfo method)
	{
		if (method.IsAsyncVoid())
			return true;

		var methodReturnType = method.ReturnType;
		if (methodReturnType == typeof(Task) || methodReturnType == typeof(ValueTask))
			return true;

		if (methodReturnType.IsGenericType)
		{
			var genericDefinition = methodReturnType.GetGenericTypeDefinition();
			if (genericDefinition == typeof(Task<>) || genericDefinition == typeof(ValueTask<>) || genericDefinition.FullName == "Microsoft.FSharp.Control.FSharpAsync`1")
				return true;
		}

		return false;
	}

	public static bool IsAsyncVoid(this MethodInfo method) =>
		method.ReturnType == typeof(void) && method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
}
