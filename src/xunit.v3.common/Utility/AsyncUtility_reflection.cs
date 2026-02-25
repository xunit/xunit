using System.Reflection;

namespace Xunit.Sdk;

partial class AsyncUtility
{
	static MethodInfo? fSharpStartAsTaskOpenGenericMethod;
	static readonly HashSet<string?> taskGenericTypes =
	[
		"Microsoft.FSharp.Control.FSharpAsync`1",
		"System.Threading.Tasks.Task`1"
	];

	/// <summary>
	/// Given an object, will attempt to convert instances of <see cref="Task"/> or
	/// <see cref="T:Microsoft.FSharp.Control.FSharpAsync`1"/> into <see cref="ValueTask"/>
	/// as appropriate. Will return <see langword="null"/> if the object is not a task of any supported type.
	/// Note that this list of supported tasks is purposefully identical to the list used
	/// by <see cref="IsAsync"/>.
	/// </summary>
	/// <param name="obj">The object to convert</param>
	/// <returns>Returns a <see cref="ValueTask"/> for the given object, if it's compatible;
	/// returns <see langword="null"/> otherwise.</returns>
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
			fSharpStartAsTaskOpenGenericMethod ??=
				type
					.Assembly
					.GetType("Microsoft.FSharp.Control.FSharpAsync")?
					.GetRuntimeMethods()
					.FirstOrDefault(m => m.Name == "StartAsTask")
						?? throw new InvalidOperationException("Test returned an F# async result, but could not find 'Microsoft.FSharp.Control.FSharpAsync.StartAsTask'");

			if (fSharpStartAsTaskOpenGenericMethod
					.MakeGenericMethod(type.GetGenericArguments()[0])
					.Invoke(null, [obj, null, null]) is Task fsharpTask)
				return new(fsharpTask);
		}

		return null;
	}
}
