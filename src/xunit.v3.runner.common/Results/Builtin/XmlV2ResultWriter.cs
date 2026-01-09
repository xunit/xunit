using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that write test results in xUnit.net XML v2 format.
/// </summary>
public sealed class XmlV2ResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"xunit";

	string IConsoleResultWriter.Description =>
		"output results to xUnit.net v2+ (XML) file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating xUnit.net v2+ (XML) report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated xUnit.net v2+ (XML) report";

	string? IConsoleResultWriter.LegacyID =>
		"xml";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new XmlV2ResultWriterMessageHandler(fileName);
}
