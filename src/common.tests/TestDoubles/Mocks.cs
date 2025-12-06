#if !XUNIT_AOT

using NSubstitute;
using Xunit.Runner.Common;

// This file contains mocks that don't belong in the other major categories.
public static partial class Mocks
{
	public static IRunnerReporter RunnerReporter(
		string? runnerSwitch = null,
		string? description = null,
		bool isEnvironmentallyEnabled = false,
		IRunnerReporterMessageHandler? messageHandler = null)
	{
		description ??= "The runner reporter description";
		messageHandler ??= Substitute.For<IRunnerReporterMessageHandler, InterfaceProxy<IRunnerReporterMessageHandler>>();

		var result = Substitute.For<IRunnerReporter, InterfaceProxy<IRunnerReporter>>();
		result.Description.Returns(description);
		result.IsEnvironmentallyEnabled.ReturnsForAnyArgs(isEnvironmentallyEnabled);
		result.RunnerSwitch.Returns(runnerSwitch);
		result.CreateMessageHandler(null!, null!).ReturnsForAnyArgs(messageHandler);
		return result;
	}
}

#endif
