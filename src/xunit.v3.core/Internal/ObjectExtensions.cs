using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.Internal
{
	static class ObjectExtensions
	{
		static readonly MethodInfo? awaitTaskMethod = typeof(ObjectExtensions).GetMethod(nameof(AwaitTask), BindingFlags.NonPublic | BindingFlags.Static);
		static readonly MethodInfo? awaitValueTaskMethod = typeof(ObjectExtensions).GetMethod(nameof(AwaitValueTask), BindingFlags.NonPublic | BindingFlags.Static);

		/// <summary/>
		public static ValueTask<object?>? AsValueTask(this object? value)
		{
			if (value == null || awaitTaskMethod == null || awaitValueTaskMethod == null)
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
	}
}
