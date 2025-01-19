using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Utility class for dealing with asynchronous operations.
/// </summary>
public static class AsyncUtility
{
	static MethodInfo? fSharpStartAsTaskOpenGenericMethod;
	static readonly HashSet<string?> taskGenericTypes =
	[
		"Microsoft.FSharp.Control.FSharpAsync`1",
		"System.Threading.Tasks.Task`1",
		"System.Threading.Tasks.ValueTask`1",
	];

	/// <summary>
	/// Determines if the given method is async, as matters to xUnit.net. This means it either (a) returns
	/// a <see cref="Task"/> or <see cref="ValueTask"/>; or, (b) it is an F# function which was declared as
	/// async. Note that this is not the same thing as an "awaitable" method, since xUnit.net does not
	/// recreate the compiler's await machinery at runtime.
	/// </summary>
	/// <param name="method">The method to test</param>
	/// <returns>Returns <c>true</c> if the method is async; returns <c>false</c> otherwise.</returns>
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
	/// <returns>Returns <c>true</c> if the method is async void; returns <c>false</c> otherwise.</returns>
	public static bool IsAsyncVoid(MethodInfo method) =>
		Guard.ArgumentNotNull(method).ReturnType == typeof(void) && method.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;

	/// <summary>
	/// Given an object, will attempt to convert instances of <see cref="Task"/> or
	/// <see cref="T:Microsoft.FSharp.Control.FSharpAsync`1"/> into <see cref="ValueTask"/>
	/// as appropriate. Will return <c>null</c> if the object is not a task of any supported type.
	/// Note that this list of supported tasks is purposefully identical to the list used
	/// by <see cref="IsAsync"/>.
	/// </summary>
	/// <param name="obj">The object to convert</param>
	/// <returns>Returns a <see cref="ValueTask"/> for the given object, if it's compatible;
	/// returns <c>null</c> otherwise.</returns>
	public static ValueTask? TryConvertToValueTask(object? obj)
	{
		if (obj is null)
			return null;

		if (obj is ValueTask valueTask)
			return valueTask;

		if (obj is Task task)
			return
				task.Status != TaskStatus.Created
					? new(task)
					: throw new InvalidOperationException("Test method returned a non-started Task (tasks must be started before being returned)");

		var type = obj.GetType();
		if (type.IsGenericType && type.GetGenericTypeDefinition().SafeName() == "Microsoft.FSharp.Control.FSharpAsync`1")
		{
			if (fSharpStartAsTaskOpenGenericMethod is null)
			{
				fSharpStartAsTaskOpenGenericMethod =
					type
						.Assembly
						.GetType("Microsoft.FSharp.Control.FSharpAsync")?
						.GetRuntimeMethods()
						.FirstOrDefault(m => m.Name == "StartAsTask");

				if (fSharpStartAsTaskOpenGenericMethod is null)
					throw new InvalidOperationException("Test returned an F# async result, but could not find 'Microsoft.FSharp.Control.FSharpAsync.StartAsTask'");
			}

			if (fSharpStartAsTaskOpenGenericMethod
					.MakeGenericMethod(type.GetGenericArguments()[0])
					.Invoke(null, [obj, null, null]) is Task fsharpTask)
				return new(fsharpTask);
		}

		return null;
	}
}
