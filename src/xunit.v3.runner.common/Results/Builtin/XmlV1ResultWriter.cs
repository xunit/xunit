using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> that write test results in xUnit.net XML v1 format.
/// </summary>
public sealed class XmlV1ResultWriter : IConsoleResultWriter
{
	/// <inheritdoc/>
	public string Description =>
		"output results to xUnit.net v1 (XML) file";

	/// <inheritdoc/>
	public string? LegacyID =>
		"xmlV1";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new XmlV1ResultWriterMessageHandler(fileName);
}
