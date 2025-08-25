#pragma warning disable xUnit3001 // This class is not created at runtime by the framework

using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public class SpyRunnerReporter(
	bool canBeEnvironmentallyEnabled = false,
	string description = "Spy runner reporter",
	bool forceNoLogo = false,
	bool isEnvironmentallyEnabled = false,
	string? runnerSwitch = "spy") :
		IRunnerReporter
{
	readonly SpyRunnerReporterMessageHandler messageHandler = new();

	public bool CanBeEnvironmentallyEnabled =>
		canBeEnvironmentallyEnabled;

	public string Description =>
		description;

	public bool ForceNoLogo =>
		forceNoLogo;

	public bool HandlerCreated { get; private set; } = false;

	public bool HandlerDisposed =>
		messageHandler.Disposed;

	public bool IsEnvironmentallyEnabled =>
		isEnvironmentallyEnabled;

	public IReadOnlyCollection<IMessageSinkMessage> Messages =>
		messageHandler.Messages;

	public string? RunnerSwitch =>
		runnerSwitch;

	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink)
	{
		HandlerCreated = true;

		return new(messageHandler);
	}

	class SpyRunnerReporterMessageHandler : IRunnerReporterMessageHandler
	{
		public bool Disposed { get; private set; } = false;

		public List<IMessageSinkMessage> Messages = [];

		public ValueTask DisposeAsync()
		{
			Disposed = true;

			return default;
		}

		public bool OnMessage(IMessageSinkMessage message)
		{
			Messages.Add(message);

			return true;
		}
	}
}
