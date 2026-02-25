namespace Xunit.Sdk;

partial class AsyncUtility
{
	static readonly HashSet<string?> taskGenericTypes =
	[
		"System.Threading.Tasks.Task`1"
	];

	/// <summary>
	/// Given an object, will attempt to convert instances of <see cref="Task"/> into <see cref="ValueTask"/>
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

		return null;
	}
}
