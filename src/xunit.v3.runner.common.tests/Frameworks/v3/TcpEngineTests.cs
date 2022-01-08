using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.v3;

public class TcpEngineTests
{
	public class StartupAndShutdownMessages
	{
		readonly List<_MessageSinkMessage> messages = new();
		readonly XunitProject project = new();
		readonly _IMessageSink spyMessageSink;

		public StartupAndShutdownMessages()
		{
			spyMessageSink = SpyMessageSink.Create(messages: messages);

			project.Add(new XunitProjectAssembly(project) { Assembly = typeof(StartupAndShutdownMessages).Assembly });
		}

		[Fact]
		public async ValueTask RunnerEngine_NoConnection()
		{
			var runnerEngine = new TcpRunnerEngine("1r", (_, _) => true, spyMessageSink);
			var port = runnerEngine.Start();

			await runnerEngine.DisposeAsync();

			Assert.Collection(
				messages.OfType<_DiagnosticMessage>().Select(dm => dm.Message),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Unknown to Initialized", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Initialized to Listening", msg),
				msg => Assert.Equal($"TcpRunnerEngine(1r): Listening on tcp://localhost:{port}/", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Listening to Disconnecting", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Disconnecting to Disconnected", msg)
			);
		}

		[Fact]
		public async ValueTask RunnerEngine_WithConnection_CleanShutdown()
		{
			var runnerEngine = new TcpRunnerEngine("1r", (_, _) => true, spyMessageSink);
			var runnerPort = runnerEngine.Start();
			project.Configuration.TcpPort = runnerPort;
			var executionEngine = new TcpExecutionEngine("1e", project, _NullMessageSink.Instance);
			var executionPort = await executionEngine.Start();
			await WaitForConnection(runnerEngine);

			await runnerEngine.DisposeAsync();
			await WaitForQuit(executionEngine);
			await executionEngine.DisposeAsync();

			Assert.Collection(
				messages.OfType<_DiagnosticMessage>().Select(dm => dm.Message),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Unknown to Initialized", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Initialized to Listening", msg),
				msg => Assert.Equal($"TcpRunnerEngine(1r): Listening on tcp://localhost:{runnerPort}/", msg),
				msg => Assert.Equal($"TcpRunnerEngine(1r): Connection accepted from tcp://localhost:{executionPort}/", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Listening to Negotiating", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Negotiating to Connected", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Connected to Disconnecting", msg),
				msg => Assert.Equal($"TcpRunnerEngine(1r): Disconnecting from tcp://localhost:{executionPort}/", msg),
				msg => Assert.Equal($"TcpRunnerEngine(1r): Disconnected from tcp://localhost:{executionPort}/", msg),
				msg => Assert.Equal("TcpRunnerEngine(1r): Engine state transition from Disconnecting to Disconnected", msg)
			);
		}

		[Fact]
		public async ValueTask RunnerEngine_WithConnection_UncleanShutdown()
		{
			var runnerEngine = new TcpRunnerEngine("1r", (_, _) => true, spyMessageSink);
			var runnerPort = runnerEngine.Start();
			project.Configuration.TcpPort = runnerPort;
			var executionEngine = new TcpExecutionEngine("1e", project, _NullMessageSink.Instance);
			var executionPort = await executionEngine.Start();
			await WaitForConnection(runnerEngine);

			await executionEngine.DisposeAsync();  // Shut down before asked to, simulates a crash
			await runnerEngine.DisposeAsync();

			// Mono seems to be not correctly cleaning up the socket, so we'll log this as a diagnostic message so we
			// can keep track of it and see what's going on, without failing or skipping the test.
			var messagesAsText = messages.OfType<_DiagnosticMessage>().Select(dm => dm.Message).ToList();
			if (messagesAsText.Count != 11)
				TestContext.Current?.SendDiagnosticMessage($"Got unexpected message count; expected 11, got {messagesAsText.Count}:{Environment.NewLine}{string.Join(Environment.NewLine, messagesAsText)}");

			Assert.Contains("TcpRunnerEngine(1r): Engine state transition from Unknown to Initialized", messagesAsText);
			Assert.Contains("TcpRunnerEngine(1r): Engine state transition from Initialized to Listening", messagesAsText);
			Assert.Contains($"TcpRunnerEngine(1r): Listening on tcp://localhost:{runnerPort}/", messagesAsText);
			Assert.Contains($"TcpRunnerEngine(1r): Connection accepted from tcp://localhost:{executionPort}/", messagesAsText);
			Assert.Contains("TcpRunnerEngine(1r): Engine state transition from Listening to Negotiating", messagesAsText);
			Assert.Contains("TcpRunnerEngine(1r): Engine state transition from Negotiating to Connected", messagesAsText);
			Assert.Contains("TcpRunnerEngine(1r): Engine state transition from Connected to Disconnecting", messagesAsText);
			Assert.Contains(messagesAsText, msg => msg.StartsWith("BufferedTcpClient(runner::1r): abnormal termination of pipe"));
			Assert.Contains($"TcpRunnerEngine(1r): Disconnecting from tcp://localhost:{executionPort}/", messagesAsText);
			Assert.Contains($"TcpRunnerEngine(1r): Disconnected from tcp://localhost:{executionPort}/", messagesAsText);
			Assert.Contains("TcpRunnerEngine(1r): Engine state transition from Disconnecting to Disconnected", messagesAsText);
		}

		[Fact]
		public async ValueTask ExecutionEngine_WithConnection()
		{
			var runnerEngine = new TcpRunnerEngine("1r", (_, _) => true, _NullMessageSink.Instance);
			var runnerPort = runnerEngine.Start();
			project.Configuration.TcpPort = runnerPort;
			var executionEngine = new TcpExecutionEngine("1e", project, spyMessageSink);
			await executionEngine.Start();
			await WaitForConnection(runnerEngine);

			await runnerEngine.DisposeAsync();
			await WaitForQuit(executionEngine);
			await executionEngine.DisposeAsync();

			Assert.Collection(
				messages.OfType<_DiagnosticMessage>().Select(dm => dm.Message),
				msg => Assert.Equal("TcpExecutionEngine(1e): Engine state transition from Unknown to Initialized", msg),
				msg => Assert.Equal("TcpExecutionEngine(1e): Engine state transition from Initialized to Connecting", msg),
				msg => Assert.Equal($"TcpExecutionEngine(1e): Connecting to tcp://localhost:{runnerPort}/", msg),
				msg => Assert.Equal($"TcpExecutionEngine(1e): Connected to tcp://localhost:{runnerPort}/", msg),
				msg => Assert.Equal("TcpExecutionEngine(1e): Engine state transition from Connecting to Negotiating", msg),
				msg => Assert.Equal("TcpExecutionEngine(1e): Engine state transition from Negotiating to Connected", msg),
				msg => Assert.Equal("TcpExecutionEngine(1e): Engine state transition from Connected to Disconnecting", msg),
				msg => Assert.Equal($"TcpExecutionEngine(1e): Disconnecting from tcp://localhost:{runnerPort}/", msg),
				msg => Assert.Equal($"TcpExecutionEngine(1e): Disconnected from tcp://localhost:{runnerPort}/", msg),
				msg => Assert.Equal("TcpExecutionEngine(1e): Engine state transition from Disconnecting to Disconnected", msg)
			);
		}
	}

	public class Quit
	{
		readonly XunitProject project = new();

		public Quit()
		{
			project.Add(new XunitProjectAssembly(project) { Assembly = typeof(StartupAndShutdownMessages).Assembly });
		}

		[Fact]
		public async ValueTask WhenRunnerSendsQuit_ExecutionEngineStops()
		{
			await using var runnerEngine = new TcpRunnerEngine("1r", (_, _) => true, _NullMessageSink.Instance);
			var runnerPort = runnerEngine.Start();
			project.Configuration.TcpPort = runnerPort;
			await using var executionEngine = new TcpExecutionEngine("1e", project, _NullMessageSink.Instance);
			await executionEngine.Start();
			await WaitForConnection(runnerEngine);

			runnerEngine.SendQuit();
			var success = await WaitForQuit(executionEngine, throwOnFailure: false);

			Assert.True(success, "Timed out waiting for the QUIT signal to arrive");
		}

		[Fact]
		public async ValueTask DisposingRunnerBeforeSendingQuit_SendsQuit()
		{
			var runnerEngine = new TcpRunnerEngine("1r", (_, _) => true, _NullMessageSink.Instance);
			var runnerPort = runnerEngine.Start();
			project.Configuration.TcpPort = runnerPort;
			await using var executionEngine = new TcpExecutionEngine("1e", project, _NullMessageSink.Instance);
			await executionEngine.Start();
			await WaitForConnection(runnerEngine);

			await runnerEngine.DisposeAsync();
			var success = await WaitForQuit(executionEngine, throwOnFailure: false);

			Assert.True(success, "Timed out waiting for the QUIT signal to arrive");
		}
	}

	static Task<bool> WaitForConnection(
		TcpRunnerEngine runnerEngine,
		bool throwOnFailure = true,
		TimeSpan? timeout = null) =>
			Task.Run(async () =>
			{
				var end = DateTimeOffset.Now.Add(timeout ?? TimeSpan.FromSeconds(5));

				while (true)
				{
					if (runnerEngine.State == TcpEngineState.Connected)
						return true;

					if (DateTimeOffset.Now > end)
					{
						if (throwOnFailure)
							throw new InvalidOperationException("Timed out waiting for execution engine connection");
						return false;
					}

					await Task.Delay(50);
				}
			});

	static Task<bool> WaitForQuit(
		TcpExecutionEngine executionEngine,
		bool throwOnFailure = true,
		TimeSpan? timeout = null) =>
			Task.Run(async () =>
			{
				var timerTask = Task.Delay(timeout ?? TimeSpan.FromSeconds(5));
				var waitForQuitTask = executionEngine.WaitForQuit();
				var finishedTask = await Task.WhenAny(waitForQuitTask, timerTask);

				if (throwOnFailure && finishedTask != waitForQuitTask)
					throw new InvalidOperationException("Timed out waiting for the QUIT signal to arrive");

				return finishedTask == waitForQuitTask;
			});
}
