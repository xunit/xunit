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

		// We loop here because the compiler can derive new types at build time from Task<T> or ValueTask<T>
		// so we need to walk the hierarchy so we can find the origin type. This behavior only seems to exist
		// in newer .NET platforms, and not with .NET Framework, regardless of compiler/SDK version.
		for (var type = value.GetType(); type is not null; type = type.BaseType)
			if (type.IsGenericType)
			{
				var genericTypeDefinition = type.GetGenericTypeDefinition();

				if (genericTypeDefinition == typeof(Task<>))
				{
					var taskReturnType = type.GetGenericArguments().Single();
					var awaitMethod = awaitTaskMethod.MakeGenericMethod(taskReturnType);
					return (ValueTask<object?>?)awaitMethod.Invoke(null, [value]);
				}

				if (genericTypeDefinition == typeof(ValueTask<>))
				{
					var taskReturnType = type.GetGenericArguments().Single();
					var awaitMethod = awaitValueTaskMethod.MakeGenericMethod(taskReturnType);
					return (ValueTask<object?>?)awaitMethod.Invoke(null, [value]);
				}
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

		return
			value is not null
				? value
				: throw new UnsetPropertyException(propertyName, @object.GetType());
	}

	/// <summary/>
	public static T ValidateNullablePropertyValue<T>(
		this object @object,
		T? value,
		string propertyName)
			where T : struct
	{
		Guard.ArgumentNotNull(@object);

		return
			value is not null
				? value.Value
				: throw new UnsetPropertyException(propertyName, @object.GetType());
	}
}
