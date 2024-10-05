using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This interface represents a reporter which is invoked by a test runner
/// during test execution. The report can be explicitly invoked by a command
/// line switch or implicitly invoked by being environmentally enabled (for
/// example, a reporter that emits messages for TeamCity).
/// </summary>
public interface IRunnerReporter
{
	/// <summary>
	/// Gets a value which indicates if it's possible for this reporter to be environmentally
	/// enabled.
	/// </summary>
	/// <remarks>
	/// Note that this differs from <see cref="IsEnvironmentallyEnabled"/> which checks to see whether
	/// the conditions currently exist to environmentally enable the reporter. This value is used when
	/// constructing the console runner help output that lists which runners might be environmentally
	/// enabled.
	/// </remarks>
	bool CanBeEnvironmentallyEnabled { get; }

	/// <summary>
	/// Gets the description of the reporter. This is typically used when showing
	/// the user the invocation option for the reporter.
	/// </summary>
	string Description { get; }

	/// <summary>
	/// Gets a value which indicates whether this runner wishes to force no logo.
	/// Useful for runners which are designed for purely parseable output
	/// (for example, <see cref="JsonReporter"/>).
	/// </summary>
	bool ForceNoLogo { get; }

	/// <summary>
	/// Gets a value which indicates whether the reporter should be
	/// environmentally enabled.
	/// </summary>
	/// <remarks>
	/// When a runner reporter is environmentally enabled in Microsoft Testing Platform
	/// CLI mode (or <c>dotnet test</c>), by default all realtime output is filtered except
	/// calls to <see cref="IRunnerLogger.LogRaw"/> (unless the user has specified the
	/// <c>--xunit-info</c> switch). Environmentally enabled reporters that require
	/// realtime console output (for example, to send formatted messages to a CI system
	/// which parses the console output) must use <see cref="IRunnerLogger.LogRaw"/> to
	/// ensure those messages will always be visible. All other messages will always be
	/// output to the Microsoft Testing Platform diagnostic logs, which are enabled via
	/// the <c>--diagnostic</c> switch.
	/// </remarks>
	bool IsEnvironmentallyEnabled { get; }

	/// <summary>
	/// Gets a value which indicates a runner switch which can be used
	/// to explicitly enable the runner. If the return value is <c>null</c>,
	/// then the reported can only be environmentally enabled (implicitly).
	/// This value is used either as a command line switch (with the console or
	/// .NET CLI runner) or as a runner configuration value (with the MSBuild runner).
	/// </summary>
	/// <remarks>
	/// Runner switches are only used in xUnit.net native CLI mode. When Microsoft
	/// Testing Platform CLI mode is enabled, reporters are only supported via environmental
	/// enablement, since MTP generally controls all the normal output.
	/// </remarks>
	string? RunnerSwitch { get; }

	/// <summary>
	/// Creates a message handler that will report messages for the given
	/// test assembly.
	/// </summary>
	/// <param name="logger">The logger used to send result messages to</param>
	/// <param name="diagnosticMessageSink">An optional message sink that diagnostic messages can be sent to.</param>
	/// <returns>The message handler that handles the messages</returns>
	ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink);
}
