namespace Xunit.Sdk;

// TODO: It's certainly convenient that we reuse ITestCaseMetadata here, but is that appropriate? All the
// metadata interfaces are centered around execution, not discovery.

/// <summary>
/// This message indicates that a test case had been found during the discovery process.
/// </summary>
public interface ITestCaseDiscovered : ITestCaseMessage, ITestCaseMetadata
{
	/// <summary>
	/// Gets the serialized value of the test case, which allows it to be transferred across
	/// process boundaries.
	/// </summary>
	string Serialization { get; }
}
