using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class ObjectExtensions
{
	static readonly MethodInfo? awaitTaskMethod = typeof(ObjectExtensions).GetMethod(nameof(AwaitTask), BindingFlags.NonPublic | BindingFlags.Static);
	static readonly MethodInfo? awaitValueTaskMethod = typeof(ObjectExtensions).GetMethod(nameof(AwaitValueTask), BindingFlags.NonPublic | BindingFlags.Static);

	/// <summary/>
	public static ValueTask<object?>? AsValueTask(this object? value)
	{
		if (value is null || awaitTaskMethod is null || awaitValueTaskMethod is null)
			return null;

		var type = value.GetType();
		if (!type.IsGenericType)
			return null;

		var genericTypeDefinition = type.GetGenericTypeDefinition();

		if (genericTypeDefinition == typeof(Task<>))
		{
			var taskReturnType = type.GetGenericArguments().Single();
			var awaitMethod = awaitTaskMethod.MakeGenericMethod(taskReturnType);
			return (ValueTask<object?>?)awaitMethod.Invoke(null, new[] { value });
		}

		if (genericTypeDefinition == typeof(ValueTask<>))
		{
			var taskReturnType = type.GetGenericArguments().Single();
			var awaitMethod = awaitValueTaskMethod.MakeGenericMethod(taskReturnType);
			return (ValueTask<object?>?)awaitMethod.Invoke(null, new[] { value });
		}

		return null;
	}

	static async ValueTask<object?> AwaitTask<T>(Task<T> task) =>
		await task;

	static async ValueTask<object?> AwaitValueTask<T>(ValueTask<T> task) =>
		await task;

	/// <summary/>
	public static T ValidateNullablePropertyValue<T>(
		this object @object,
		T? value,
		string propertyName)
			where T : class
	{
		Guard.ArgumentNotNull(@object);

		if (value is null)
			throw new UnsetPropertyException(propertyName, @object.GetType());

		return value;
	}

	/// <summary/>
	public static T ValidateNullablePropertyValue<T>(
		this object @object,
		T? value,
		string propertyName)
			where T : struct
	{
		Guard.ArgumentNotNull(@object);

		if (value is null)
			throw new UnsetPropertyException(propertyName, @object.GetType());

		return value.Value;
	}
}
