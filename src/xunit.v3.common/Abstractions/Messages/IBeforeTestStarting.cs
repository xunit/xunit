namespace Xunit.Sdk;

/// <summary>
/// This message is sent during execution to indicate that the Before method of a
/// <see cref="T:Xunit.v3.IBeforeAfterTestAttribute"/> is about to execute.
/// </summary>
public interface IBeforeTestStarting : ITestMessage
{
	/// <summary>
	/// Gets the fully qualified type name of the <see cref="T:Xunit.v3.IBeforeAfterTestAttribute"/>.
	/// </summary>
	string AttributeName { get; }
}
