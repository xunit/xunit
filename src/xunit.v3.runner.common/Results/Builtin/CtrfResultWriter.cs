using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that write test results in CTRF format.
/// </summary>
public sealed class CtrfResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"ctrf";

	string IConsoleResultWriter.Description =>
		"output results to CTRF (JSON) file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating CTRF (JSON) report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated CTRF (JSON) report";

	string? IConsoleResultWriter.LegacyID =>
		"ctrf";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new CtrfResultWriterMessageHandler(fileName);
}
