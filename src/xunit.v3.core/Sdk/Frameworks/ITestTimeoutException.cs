using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// This is a marker interface implemented to indicate that the exception is the result
	/// of a test timeout, resulting in a failure cause of <see cref="FailureCause.Timeout"/>.
	/// </summary>
	public interface ITestTimeoutException
	{ }
}
