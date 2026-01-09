using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that write test results in Microsoft TRX format.
/// </summary>
public sealed class TrxResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"trx";

	string IConsoleResultWriter.Description =>
		"output results to TRX (XML) file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating TRX (XML) report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated TRX (XML) report";

	string? IConsoleResultWriter.LegacyID =>
		"trx";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new TrxResultWriterMessageHandler(fileName);
}
