namespace Xunit.Sdk;

/// <summary>
/// This message is sent during execution to indicate that the After method of a
/// <see cref="T:Xunit.v3.IBeforeAfterTestAttribute"/> just finished executing.
/// </summary>
public interface IAfterTestFinished : ITestMessage
{
	/// <summary>
	/// Gets the fully qualified type name of the <see cref="T:Xunit.v3.IBeforeAfterTestAttribute"/>.
	/// </summary>
	string AttributeName { get; }
}
