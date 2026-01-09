using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a result writer that can run report results to an output file of the user's choosing.
/// </summary>
public interface IResultWriter
{
	/// <summary>
	/// Creates a message handler that will handle the realtime messages from test execution.
	/// </summary>
	/// <param name="fileName">The file to write the results to.</param>
	/// <param name="diagnosticMessageSink">An optional message sink that diagnostic messages
	/// can be sent to.</param>
	/// <returns>The message handler that handles the messages</returns>
	ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink);
}
