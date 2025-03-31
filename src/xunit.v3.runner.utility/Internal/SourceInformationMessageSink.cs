using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

// Wraps a message sink to augment source information when it's missing

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class SourceInformationMessageSink(
	IMessageSink innerSink,
	ISourceInformationProvider sourceInformationProvider) :
		IMessageSink
{
	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is ITestCaseDiscovered discovered && discovered.SourceFilePath is null && discovered.SourceLineNumber is null)
			message = discovered.WithSourceInfo(sourceInformationProvider);
		else if (message is ITestCaseStarting starting && starting.SourceFilePath is null && starting.SourceLineNumber is null)
			message = starting.WithSourceInfo(sourceInformationProvider);

		return innerSink.OnMessage(message);
	}
}
