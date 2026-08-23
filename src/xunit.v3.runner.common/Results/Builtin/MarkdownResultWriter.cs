using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IConsoleResultWriter"/> and <see cref="IMicrosoftTestingPlatformResultWriter"/>
/// that writes test results in Markdown format.
/// </summary>
public sealed class MarkdownResultWriter : IConsoleResultWriter, IMicrosoftTestingPlatformResultWriter
{
	string IMicrosoftTestingPlatformResultWriter.DefaultFileExtension =>
		"md";

	string IConsoleResultWriter.Description =>
		"output results to Markdown file";

	string IMicrosoftTestingPlatformResultWriter.Description =>
		"Enable generating Markdown report";

	string IMicrosoftTestingPlatformResultWriter.FileNameDescription =>
		"The name of the generated Markdown report";

	/// <inheritdoc/>
	public async ValueTask<IResultWriterMessageHandler> CreateMessageHandler(
		string fileName,
		IMessageSink? diagnosticMessageSink) =>
			new MarkdownResultWriterMessageHandler(fileName);
}
