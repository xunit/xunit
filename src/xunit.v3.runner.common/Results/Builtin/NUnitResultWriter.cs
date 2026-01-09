using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that write test results in NUnit v2.5 format.
/// </summary>
public sealed class NUnitResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"nunit";

	string IConsoleResultWriter.Description =>
		"output results to NUnit v3 (XML) file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating NUnit v3 (XML) report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated NUnit v3 (XML) report";

	string? IConsoleResultWriter.LegacyID =>
		"nUnit";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new NUnitResultWriterMessageHandler(fileName);
}
