using System;
using System.Text;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="TeamCityReporter" />.
	/// </summary>
	public class TeamCityReporterMessageHandler : FlowMappedTestMessageSink
	{
		readonly TeamCityDisplayNameFormatter displayNameFormatter;
		readonly IRunnerLogger logger;
		MessageMetadataCache metadataCache = new MessageMetadataCache();

		/// <summary>
		/// Initializes a new instance of the <see cref="TeamCityReporterMessageHandler" /> class.
		/// </summary>
		/// <param name="logger">The logger used to report messages</param>
		/// <param name="flowIdMapper">Optional code which maps a test collection name to a flow ID
		/// (the default behavior generates a new GUID for each test collection)</param>
		/// <param name="displayNameFormatter">Optional display name formatter</param>
		public TeamCityReporterMessageHandler(
			IRunnerLogger logger,
			Func<string, string>? flowIdMapper = null,
			TeamCityDisplayNameFormatter? displayNameFormatter = null)
				: base(flowIdMapper)
		{
			Guard.ArgumentNotNull(nameof(logger), logger);

			this.logger = logger;
			this.displayNameFormatter = displayNameFormatter ?? new TeamCityDisplayNameFormatter();

			Diagnostics.ErrorMessageEvent += HandleErrorMessage;

			Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
			Execution.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
			Execution.TestAssemblyStartingEvent += HandleTestAssemblyStarting;
			Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
			Execution.TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
			Execution.TestClassFinishedEvent += HandleTestClassFinished;
			Execution.TestClassStartingEvent += HandleTestClassStarting;
			Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
			Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;
			Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;
			Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
			Execution.TestFailedEvent += HandleTestFailed;
			Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
			Execution.TestPassedEvent += HandleTestPassed;
			Execution.TestSkippedEvent += HandleTestSkipped;
			Execution.TestStartingEvent += HandleTestStarting;
		}

		/// <summary>
		/// Handles instances of <see cref="IErrorMessage" />.
		/// </summary>
		protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var error = args.Message;
			LogError("FATAL ERROR", error);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestAssemblyCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<_TestAssemblyCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var metadata = metadataCache.TryGet(cleanupFailure);
			if (metadata != null)
				LogError($"Test Assembly Cleanup Failure ({metadata.AssemblyPath})", cleanupFailure);
			else
				LogError("Test Assembly Cleanup Failure (<unknown test assembly>)", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestAssemblyFinished" />.
		/// </summary>
		protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.TryRemove(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestAssemblyStarting" />.
		/// </summary>
		protected virtual void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.Set(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="ITestCaseCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			LogError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestClassCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<_TestClassCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var metadata = metadataCache.TryGet(cleanupFailure);
			if (metadata != null)
				LogError($"Test Class Cleanup Failure ({metadata.TestClass})", cleanupFailure);
			else
				LogError("Test Class Cleanup Failure (<unknown test class>)", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestClassFinished" />.
		/// </summary>
		protected virtual void HandleTestClassFinished(MessageHandlerArgs<_TestClassFinished> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.TryRemove(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestClassStarting" />.
		/// </summary>
		protected virtual void HandleTestClassStarting(MessageHandlerArgs<_TestClassStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.Set(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestCollectionCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<_TestCollectionCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var metadata = metadataCache.TryGet(cleanupFailure);
			if (metadata != null)
				LogError($"Test Collection Cleanup Failure ({metadata.TestCollectionDisplayName})", cleanupFailure);
			else
				LogError($"Test Collection Cleanup Failure (<unknown test collection>)", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestCollectionFinished" />.
		/// </summary>
		protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<_TestCollectionFinished> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testCollectionFinished = args.Message;
			var metadata = metadataCache.TryRemove(testCollectionFinished);
			if (metadata != null)
				logger.LogImportantMessage($"##teamcity[testSuiteFinished name='{metadata.TestCollectionDisplayName} ({testCollectionFinished.TestCollectionUniqueID})' flowId='{testCollectionFinished.TestCollectionUniqueID}']");
			else
				// TODO: Can we still report testSuiteFinished with an incorrect name, if the flow ID is correct?
				logger.LogImportantMessage($"##teamcity[message status='ERROR' text='Tried to report a completed test collection that was never reported as starting']");
		}

		/// <summary>
		/// Handles instances of <see cref="_TestCollectionStarting" />.
		/// </summary>
		protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<_TestCollectionStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testCollectionStarting = args.Message;
			metadataCache.Set(testCollectionStarting);

			logger.LogImportantMessage($"##teamcity[testSuiteStarted name='{testCollectionStarting.TestCollectionDisplayName} ({testCollectionStarting.TestCollectionUniqueID})' flowId='{testCollectionStarting.TestCollectionUniqueID}']");
		}

		/// <summary>
		/// Handles instances of <see cref="ITestCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			LogError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="ITestFailed" />.
		/// </summary>
		protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testFailed = args.Message;
			logger.LogImportantMessage($"##teamcity[testFailed name='{Escape(displayNameFormatter.DisplayName(testFailed.Test))}' details='{Escape(ExceptionUtility.CombineMessages(testFailed))}|r|n{Escape(ExceptionUtility.CombineStackTraces(testFailed))}' flowId='{ToFlowId(testFailed.TestCollection.DisplayName)}']");
			LogFinish(testFailed);
		}

		/// <summary>
		/// Handles instances of <see cref="ITestMethodCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			LogError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="ITestPassed" />.
		/// </summary>
		protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testPassed = args.Message;
			LogFinish(testPassed);
		}

		/// <summary>
		/// Handles instances of <see cref="ITestSkipped" />.
		/// </summary>
		protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testSkipped = args.Message;
			logger.LogImportantMessage($"##teamcity[testIgnored name='{Escape(displayNameFormatter.DisplayName(testSkipped.Test))}' message='{Escape(testSkipped.Reason)}' flowId='{ToFlowId(testSkipped.TestCollection.DisplayName)}']");
			LogFinish(testSkipped);
		}

		/// <summary>
		/// Handles instances of <see cref="ITestStarting" />.
		/// </summary>
		protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testStarting = args.Message;
			logger.LogImportantMessage($"##teamcity[testStarted name='{Escape(displayNameFormatter.DisplayName(testStarting.Test))}' flowId='{ToFlowId(testStarting.TestCollection.DisplayName)}']");
		}

		// Helpers

		void LogError(string messageType, IFailureInformation failureInfo)
		{
			var message = $"[{messageType}] {failureInfo.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(failureInfo)}";
			var stack = ExceptionUtility.CombineStackTraces(failureInfo);

			logger.LogImportantMessage($"##teamcity[message text='{Escape(message)}' errorDetails='{Escape(stack)}' status='ERROR']");
		}

		void LogFinish(ITestResultMessage testResult)
		{
			var formattedName = Escape(displayNameFormatter.DisplayName(testResult.Test));
			var flowId = ToFlowId(testResult.TestCollection.DisplayName);

			if (!string.IsNullOrWhiteSpace(testResult.Output))
				logger.LogImportantMessage($"##teamcity[testStdOut name='{formattedName}' out='{Escape(testResult.Output)}' flowId='{flowId}']");

			logger.LogImportantMessage($"##teamcity[testFinished name='{formattedName}' duration='{(int)(testResult.ExecutionTime * 1000M)}' flowId='{flowId}']");
		}

		static bool IsAscii(char ch) => ch <= '\x007f';

		static string? Escape(string? value)
		{
			if (value == null)
				return null;

			var sb = new StringBuilder(value.Length);
			for (var i = 0; i < value.Length; i++)
			{
				var ch = value[i];

				switch (ch)
				{
					case '|':
						sb.Append("||");
						break;
					case '\'':
						sb.Append("|'");
						break;
					case '\n':
						sb.Append("|n");
						break;
					case '\r':
						sb.Append("|r");
						break;
					case '[':
						sb.Append("|[");
						break;
					case ']':
						sb.Append("|]");
						break;
					default:
						if (IsAscii(ch))
							sb.Append(ch);
						else
						{
							sb.Append("|0x");
							sb.Append(((int)ch).ToString("x4"));
						}
						break;
				}
			}
			return sb.ToString();
		}
	}
}
