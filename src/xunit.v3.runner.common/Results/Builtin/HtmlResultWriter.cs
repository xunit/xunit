using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that write test results in HTML format.
/// </summary>
public sealed class HtmlResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"html";

	string IConsoleResultWriter.Description =>
		"output results to HTML file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating HTML report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated HTML report";

	string? IConsoleResultWriter.LegacyID =>
		"html";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new HtmlResultWriterMessageHandler(fileName);
}
