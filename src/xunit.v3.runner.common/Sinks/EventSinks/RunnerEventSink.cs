using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Class that maps test runner messages to events.
	/// </summary>
	public class RunnerEventSink : _IMessageSink
	{
		/// <summary>
		/// Occurs when the runner is starting discovery for a given test assembly.
		/// </summary>
		public event MessageHandler<ITestAssemblyDiscoveryFinished>? TestAssemblyDiscoveryFinishedEvent;

		/// <summary>
		/// Occurs when the runner has finished discovery for a given test assembly.
		/// </summary>
		public event MessageHandler<ITestAssemblyDiscoveryStarting>? TestAssemblyDiscoveryStartingEvent;

		/// <summary>
		/// Occurs when the runner has finished executing the given test assembly.
		/// </summary>
		public event MessageHandler<ITestAssemblyExecutionFinished>? TestAssemblyExecutionFinishedEvent;

		/// <summary>
		/// Occurs when the runner is starting to execution the given test assembly.
		/// </summary>
		public event MessageHandler<ITestAssemblyExecutionStarting>? TestAssemblyExecutionStartingEvent;

		/// <summary>
		/// Occurs when the runner has finished executing all test assemblies.
		/// </summary>
		public event MessageHandler<TestExecutionSummaries>? TestExecutionSummariesEvent;

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return
				message.Dispatch(null, TestAssemblyDiscoveryFinishedEvent) &&
				message.Dispatch(null, TestAssemblyDiscoveryStartingEvent) &&
				message.Dispatch(null, TestAssemblyExecutionFinishedEvent) &&
				message.Dispatch(null, TestAssemblyExecutionStartingEvent) &&
				message.Dispatch(null, TestExecutionSummariesEvent);
		}
	}
}
