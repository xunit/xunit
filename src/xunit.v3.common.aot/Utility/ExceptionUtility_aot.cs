using System.Collections.Concurrent;

namespace Xunit.Sdk;

partial class ExceptionUtility
{
	static readonly ConcurrentDictionary<Type, bool> isXunitExceptionByType = new();

	static FailureCause GetFailureCause(Exception ex) =>
		ex.GetType().FullName == "Xunit.Sdk.TestTimeoutException"
			? FailureCause.Timeout
			: isXunitExceptionByType.GetOrAdd(ex.GetType(), IsXunitException)
				? FailureCause.Assertion
				: FailureCause.Exception;

	static IEnumerable<Exception>? GetInnerExceptions(Exception ex) =>
		(ex as AggregateException)?.InnerExceptions;

	static bool IsXunitException(Type? type)
	{
		for (; type is not null; type = type.BaseType)
			if (type.FullName == "Xunit.Sdk.XunitException")
				return true;

		return false;
	}
}
