using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that write test results in JUnit format.
/// </summary>
public sealed class JUnitResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"junit.xml";

	string IConsoleResultWriter.Description =>
		"output results to JUnit (XML) file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating JUnit (XML) report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated JUnit (XML) report";

	string? IConsoleResultWriter.LegacyID =>
		"jUnit";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new JUnitResultWriterMessageHandler(fileName);
}
