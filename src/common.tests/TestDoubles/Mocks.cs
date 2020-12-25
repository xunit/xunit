using NSubstitute;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.v3
{
	// This file contains mocks that don't belong in the other major categories.
	public static partial class Mocks
	{
		static readonly object[] EmptyObjects = new object[0];

		public static readonly _IReflectionTypeInfo TypeObject = Reflector.Wrap(typeof(object));
		public static readonly _IReflectionTypeInfo TypeString = Reflector.Wrap(typeof(string));
		public static readonly _IReflectionTypeInfo TypeVoid = Reflector.Wrap(typeof(void));

		public static IRunnerReporter RunnerReporter(
			string? runnerSwitch = null,
			string? description = null,
			bool isEnvironmentallyEnabled = false,
			_IMessageSink? messageSink = null)
		{
			description ??= "The runner reporter description";
			messageSink ??= Substitute.For<_IMessageSink, InterfaceProxy<_IMessageSink>>();

			var result = Substitute.For<IRunnerReporter, InterfaceProxy<IRunnerReporter>>();
			result.Description.Returns(description);
			result.IsEnvironmentallyEnabled.ReturnsForAnyArgs(isEnvironmentallyEnabled);
			result.RunnerSwitch.Returns(runnerSwitch);
			result.CreateMessageHandler(null!).ReturnsForAnyArgs(messageSink);
			return result;
		}
	}
}
