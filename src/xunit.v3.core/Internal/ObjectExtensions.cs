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
		public static Task<object?>? AsTask(this object? value)
		{
			if (value == null || awaitTaskMethod == null || awaitValueTaskMethod == null)
				return null;

			var type = value.GetType();
			if (!type.IsGenericType)
				return null;

			if (type.GetGenericTypeDefinition() == typeof(Task<>))
			{
				var taskReturnType = type.GetGenericArguments().Single();
				var awaitMethod = awaitTaskMethod.MakeGenericMethod(taskReturnType);
				return awaitMethod.Invoke(null, new[] { value }) as Task<object?>;
			}

			if (type.GetGenericTypeDefinition() == typeof(ValueTask<>))
			{
				var taskReturnType = type.GetGenericArguments().Single();
				var awaitMethod = awaitValueTaskMethod.MakeGenericMethod(taskReturnType);
				return awaitMethod.Invoke(null, new[] { value }) as Task<object?>;
			}

			return null;
		}

		static async Task<object?> AwaitTask<T>(Task<T> task) =>
			await task;

		static async Task<object?> AwaitValueTask<T>(ValueTask<T> task) =>
			await task;
	}
}
