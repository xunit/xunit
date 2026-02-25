using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xunit.Sdk;

/// <summary>
/// Utility class for dealing with asynchronous operations.
/// </summary>
public static partial class AsyncUtility
{
	/// <summary>
	/// Awaits an object, if <see cref="TryConvertToValueTask"/> can successfully convert it to
	/// a <see cref="ValueTask"/>. If the object is not compatible, then does nothing.
	/// </summary>
	/// <param name="task">The potential task object</param>
	/// <remarks>If <paramref name="task"/> is <see cref="Task"/> but has not yet been started, this will
	/// throw <see cref="InvalidOperationException"/>.</remarks>
	public static async ValueTask Await(object? task)
	{
		if (TryConvertToValueTask(task) is { } valueTask)
			await valueTask;
	}

	/// <summary>
	/// Determines if the given method is async, as matters to xUnit.net. This means it either (a) returns
	/// a <see cref="Task"/>, <see cref="Task{TResult}"/>, or <see cref="ValueTask"/>; or, (b) it is an F#
	/// function which was declared as <c>async</c>. Note that this is not the same thing as an "awaitable"
	/// method, since xUnit.net does not recreate the compiler's await machinery at runtime.
	/// </summary>
	/// <param name="method">The method to test</param>
	/// <returns>Returns <see langword="true"/> if the method is async; returns <see langword="false"/> otherwise.</returns>
	public static bool IsAsync(MethodInfo method)
	{
		Guard.ArgumentNotNull(method);

		var methodReturnType = method.ReturnType;
		return
			methodReturnType == typeof(Task) ||
			methodReturnType == typeof(ValueTask) ||
			(methodReturnType.IsGenericType && taskGenericTypes.Contains(methodReturnType.GetGenericTypeDefinition().SafeName()));
	}

	/// <summary>
	/// Determines if the given method is async void by looking for the <see cref="AsyncStateMachineAttribute"/>
	/// and <see cref="void"/> return type on the method definition.
	/// </summary>
	/// <param name="method">The method to test</param>
	/// <returns>Returns <see langword="true"/> if the method is async void; returns <see langword="false"/> otherwise.</returns>
	public static bool IsAsyncVoid(MethodInfo method) =>
		Guard.ArgumentNotNull(method).ReturnType == typeof(void) && method.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;
}
