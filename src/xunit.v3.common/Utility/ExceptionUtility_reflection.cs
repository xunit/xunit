using System.Collections.Concurrent;
using System.Reflection;

namespace Xunit.Sdk;

partial class ExceptionUtility
{
	static readonly ConcurrentDictionary<Type, MethodInfo?> innerExceptionsPropertyByType = new();

	static FailureCause GetFailureCause(Exception ex)
	{
		var interfaces = ex.GetType().GetInterfaces();

		return
			interfaces.Any(i => i.Name == "ITestTimeoutException")
				? FailureCause.Timeout
				: interfaces.Any(i => i.Name == "IAssertionException")
					? FailureCause.Assertion
					: FailureCause.Exception;
	}

	static IEnumerable<Exception>? GetInnerExceptions(Exception ex)
	{
		if (ex is AggregateException aggEx)
			return aggEx.InnerExceptions;

		var prop = innerExceptionsPropertyByType.GetOrAdd(
			ex.GetType(),
			t => t.GetProperties().FirstOrDefault(p => p.Name == "InnerExceptions" && p.CanRead)?.GetGetMethod()
		);

		return prop?.Invoke(ex, null) as IEnumerable<Exception>;
	}
}
