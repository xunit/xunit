using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test has finished executing.
/// </summary>
public interface ITestFinished : ITestResultMessage
{
	/// <summary>
	/// Gets any attachments that were added to the test result via <see cref="M:Xunit.TestContext.AddAttachment"/>.
	/// </summary>
	// Due to the potential serialization size of this information, it was decided to put this only
	// in ITestFinished and not in ITestResultMessage, because otherwise the information would be
	// duplicated on the wire.
	public IReadOnlyDictionary<string, TestAttachment> Attachments { get; }
}
