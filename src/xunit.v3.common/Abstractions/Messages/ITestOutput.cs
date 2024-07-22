namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a line of output was provided for a test.
/// </summary>
public interface ITestOutput : ITestMessage
{
	/// <summary>
	/// Gets the line of output.
	/// </summary>
	string Output { get; }
}
