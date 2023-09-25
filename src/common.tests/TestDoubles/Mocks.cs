using NSubstitute;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

// This file contains mocks that don't belong in the other major categories.
public static partial class Mocks
{
	static readonly object[] EmptyObjects = new object[0];

	public const string DefaultTargetFramework = TestData.DefaultTargetFramework;
	public const string DefaultTestFrameworkDisplayName = "SomeTestFramework v12.34.56";

	public static readonly _IReflectionTypeInfo TypeObject = Reflector.Wrap(typeof(object));
	public static readonly _IReflectionTypeInfo TypeString = Reflector.Wrap(typeof(string));
	public static readonly _IReflectionTypeInfo TypeVoid = Reflector.Wrap(typeof(void));

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
