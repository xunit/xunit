using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;
using DiagnosticMessage = Xunit.Runner.Common.DiagnosticMessage;
using ErrorMessage = Xunit.Runner.Common.ErrorMessage;

namespace Xunit.Runner.InProc.SystemConsole;

// The signatures of everything in this class are used at runtime via reflection, and as such must not
// be modified without creating forks inside InProcessTestProcessLauncher.

/// <summary/>
/// <summary/>
public static class ConsoleRunnerInProcess
{
	/// <summary/>
	public static async Task Find(
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		XunitProjectAssembly assembly,
		CancellationTokenSource cancellationTokenSource)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(assembly);
		var testAssembly = Guard.ArgumentNotNull(assembly.Assembly);

		var startStop = await StartStop.Start(testAssembly, messageSink, diagnosticMessageSink);

		try
		{
			var automatedMode = assembly.Configuration.SynchronousMessageReportingOrDefault ? AutomatedMode.Sync : AutomatedMode.Async;
			var projectRunner = new ProjectAssemblyRunner(testAssembly, automatedMode, cancellationTokenSource);

			await projectRunner.Discover(assembly, startStop.PipelineStartup, messageSink, diagnosticMessageSink);
		}
		finally
		{
			await startStop.SafeDisposeAsync();
		}
	}

	/// <summary/>
	public static TestAssemblyInfo GetTestAssemblyInfo(Assembly testAssembly)
	{
		var testFramework = ExtensibilityPointFactory.GetTestFramework(testAssembly);

		return new(
			// Technically these next two are the versions of xunit.v3.runner.inproc.console and not xunit.v3.core; however,
			// since they're all compiled and versioned together, we'll take the path of least resistance.
			coreFramework: new Version(ThisAssembly.AssemblyVersion),
			coreFrameworkInformational: ThisAssembly.AssemblyInformationalVersion,
			targetFramework: testAssembly.GetTargetFramework(),
			testFramework: testFramework.TestFrameworkDisplayName
		);
	}

	/// <summary/>
	public static async Task Run(
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		XunitProjectAssembly assembly,
		CancellationTokenSource cancellationTokenSource)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(assembly);
		var testAssembly = Guard.ArgumentNotNull(assembly.Assembly);

		var startStop = await StartStop.Start(testAssembly, messageSink, diagnosticMessageSink);

		try
		{
			var automatedMode = assembly.Configuration.SynchronousMessageReportingOrDefault ? AutomatedMode.Sync : AutomatedMode.Async;
			var projectRunner = new ProjectAssemblyRunner(testAssembly, automatedMode, cancellationTokenSource);
			var logger = new DecodingRunnerLogger(messageSink, diagnosticMessageSink);
			var testCaseIDsToRun = assembly.TestCasesToRun.Count == 0 ? null : assembly.TestCasesToRun.ToHashSet();

			await projectRunner.Run(assembly, messageSink, diagnosticMessageSink, logger, startStop.PipelineStartup, testCaseIDsToRun);
		}
		finally
		{
			await startStop.SafeDisposeAsync();
		}
	}

	sealed class StartStop : IAsyncDisposable
	{
		readonly IMessageSink? messageSink;
		readonly IMessageSink? diagnosticMessageSink;
		readonly TraceAssertOverrideListener overrideListener;

		public StartStop(
			Assembly testAssembly,
			IMessageSink? messageSink,
			IMessageSink? diagnosticMessageSink,
			ITestPipelineStartup? pipelineStartup)
		{
			this.messageSink = messageSink;
			this.diagnosticMessageSink = diagnosticMessageSink;

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			overrideListener = new TraceAssertOverrideListener();
			PipelineStartup = pipelineStartup;

			SerializationHelper.Instance.AddRegisteredSerializers(testAssembly, []);
		}

		public ITestPipelineStartup? PipelineStartup { get; }

		public async ValueTask DisposeAsync()
		{
			SerializationHelper.ResetInstance();

			overrideListener.Dispose();

			AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

			if (PipelineStartup is not null)
				await PipelineStartup.StopAsync();
		}

		void OnUnhandledException(
			object sender,
			UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
				messageSink?.OnMessage(ErrorMessage.FromException(ex));
			else
				diagnosticMessageSink?.OnMessage(new DiagnosticMessage("Error of unknown type thrown in application domain"));
		}

		public static async ValueTask<StartStop> Start(
			Assembly testAssembly,
			IMessageSink? messageSink,
			IMessageSink? diagnosticMessageSink) =>
				new StartStop(
					testAssembly,
					messageSink,
					diagnosticMessageSink,
					await ProjectAssemblyRunner.InvokePipelineStartup(testAssembly, diagnosticMessageSink)
				);
	}
}
