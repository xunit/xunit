using System.Text;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="TeamCityReporter" />.
	/// </summary>
	public class TeamCityReporterMessageHandler : TestMessageSink
	{
		readonly IRunnerLogger logger;
		readonly MessageMetadataCache metadataCache = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="TeamCityReporterMessageHandler" /> class.
		/// </summary>
		/// <param name="logger">The logger used to report messages</param>
		public TeamCityReporterMessageHandler(IRunnerLogger logger)
		{
			Guard.ArgumentNotNull(nameof(logger), logger);

			this.logger = logger;

			Diagnostics.ErrorMessageEvent += HandleErrorMessage;

			Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
			Execution.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
			Execution.TestAssemblyStartingEvent += HandleTestAssemblyStarting;

			Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
			Execution.TestCaseFinishedEvent += HandleTestCaseFinished;
			Execution.TestCaseStartingEvent += HandleTestCaseStarting;

			Execution.TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
			Execution.TestClassFinishedEvent += HandleTestClassFinished;
			Execution.TestClassStartingEvent += HandleTestClassStarting;

			Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
			Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;
			Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;

			Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
			Execution.TestMethodFinishedEvent += HandleTestMethodFinished;
			Execution.TestMethodStartingEvent += HandleTestMethodStarting;

			Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
			Execution.TestFailedEvent += HandleTestFailed;
			Execution.TestPassedEvent += HandleTestPassed;
			Execution.TestSkippedEvent += HandleTestSkipped;
			Execution.TestStartingEvent += HandleTestStarting;
		}

		/// <summary>
		/// Handles instances of <see cref="_ErrorMessage" />.
		/// </summary>
		protected virtual void HandleErrorMessage(MessageHandlerArgs<_ErrorMessage> args)
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
			var metadata = metadataCache.TryGetAssemblyMetadata(cleanupFailure);
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
		/// Handles instances of <see cref="_TestCaseCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<_TestCaseCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var metadata = metadataCache.TryGetTestCaseMetadata(args.Message);
			if (metadata != null)
				LogError($"Test Case Cleanup Failure ({metadata.TestCaseDisplayName})", cleanupFailure);
			else
				LogError("Test Case Cleanup Failure (<unknown test case>)", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestCaseFinished" />.
		/// </summary>
		protected virtual void HandleTestCaseFinished(MessageHandlerArgs<_TestCaseFinished> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.TryRemove(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestCaseStarting" />.
		/// </summary>
		protected virtual void HandleTestCaseStarting(MessageHandlerArgs<_TestCaseStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.Set(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestClassCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<_TestClassCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var metadata = metadataCache.TryGetClassMetadata(cleanupFailure);
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
			var metadata = metadataCache.TryGetCollectionMetadata(cleanupFailure);
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
		/// Handles instances of <see cref="_TestCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<_TestCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var testMetadata = metadataCache.TryGetTestMetadata(cleanupFailure);
			var formattedName = Escape(testMetadata != null ? testMetadata.TestDisplayName : "<unknown test>");

			LogError($"Test Cleanup Failure ({formattedName})", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestFailed" />.
		/// </summary>
		protected virtual void HandleTestFailed(MessageHandlerArgs<_TestFailed> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testFailed = args.Message;
			var testMetadata = metadataCache.TryGetTestMetadata(testFailed);
			var formattedName = Escape(testMetadata != null ? testMetadata.TestDisplayName : "<unknown test>");
			var details = $"{Escape(ExceptionUtility.CombineMessages(testFailed))}|r|n{Escape(ExceptionUtility.CombineStackTraces(testFailed))}";

			logger.LogImportantMessage($"##teamcity[testFailed name='{formattedName}' details='{details}' flowId='{testFailed.TestCollectionUniqueID}']");

			LogFinish(testFailed);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestMethodCleanupFailure" />.
		/// </summary>
		protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<_TestMethodCleanupFailure> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var cleanupFailure = args.Message;
			var metadata = metadataCache.TryGetMethodMetadata(args.Message);
			if (metadata != null)
				LogError($"Test Method Cleanup Failure ({metadata.TestMethod})", cleanupFailure);
			else
				LogError("Test Method Cleanup Failure (<unknown test method>)", cleanupFailure);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestMethodFinished" />.
		/// </summary>
		protected virtual void HandleTestMethodFinished(MessageHandlerArgs<_TestMethodFinished> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.TryRemove(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestMethodStarting" />.
		/// </summary>
		protected virtual void HandleTestMethodStarting(MessageHandlerArgs<_TestMethodStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			metadataCache.Set(args.Message);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestPassed" />.
		/// </summary>
		protected virtual void HandleTestPassed(MessageHandlerArgs<_TestPassed> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testPassed = args.Message;

			LogFinish(testPassed);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestSkipped" />.
		/// </summary>
		protected virtual void HandleTestSkipped(MessageHandlerArgs<_TestSkipped> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testSkipped = args.Message;
			var testMetadata = metadataCache.TryGetTestMetadata(testSkipped);
			var formattedName = Escape(testMetadata != null ? testMetadata.TestDisplayName : "<unknown test>");

			logger.LogImportantMessage($"##teamcity[testIgnored name='{formattedName}' message='{Escape(testSkipped.Reason)}' flowId='{testSkipped.TestCollectionUniqueID}']");

			LogFinish(testSkipped);
		}

		/// <summary>
		/// Handles instances of <see cref="_TestStarting" />.
		/// </summary>
		protected virtual void HandleTestStarting(MessageHandlerArgs<_TestStarting> args)
		{
			Guard.ArgumentNotNull(nameof(args), args);

			var testStarting = args.Message;
			var formattedName = Escape(testStarting.TestDisplayName);

			logger.LogImportantMessage($"##teamcity[testStarted name='{formattedName}' flowId='{testStarting.TestCollectionUniqueID}']");

			metadataCache.Set(testStarting);
		}

		// Helpers

		void LogError(string messageType, _IErrorMetadata errorMetadata)
		{
			var message = $"[{messageType}] {errorMetadata.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(errorMetadata)}";
			var stack = ExceptionUtility.CombineStackTraces(errorMetadata);

			logger.LogImportantMessage($"##teamcity[message text='{Escape(message)}' errorDetails='{Escape(stack)}' status='ERROR']");
		}

		void LogFinish(_TestResultMessage testResult)
		{
			var testMetadata = metadataCache.TryGetTestMetadata(testResult);
			var formattedName = Escape(testMetadata != null ? testMetadata.TestDisplayName : "<unknown test>");
			var flowId = testResult.TestCollectionUniqueID;

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
