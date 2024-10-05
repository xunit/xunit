using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="TeamCityReporter" />.
/// </summary>
/// <param name="logger">The logger used to report messages</param>
/// <param name="rootFlowId">The root flow ID for reporting to TeamCity</param>
public class TeamCityReporterMessageHandler(
	IRunnerLogger logger,
	string? rootFlowId) :
		DefaultRunnerReporterMessageHandler(logger)
{
	readonly MessageMetadataCache metadataCache = new();

	/// <summary>
	/// Gets the current date &amp; time in UTC.
	/// </summary>
	protected virtual DateTimeOffset UtcNow =>
		DateTimeOffset.UtcNow;

	/// <summary>
	/// Handles instances of <see cref="IErrorMessage" />.
	/// </summary>
	protected override void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleErrorMessage(args);

		var error = args.Message;

		TeamCityLogError(error, "FATAL ERROR");
	}

	/// <summary>
	/// Handles instances of <see cref="ITestAssemblyCleanupFailure" />.
	/// </summary>
	protected override void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestAssemblyCleanupFailure(args);

		var cleanupFailure = args.Message;

		TeamCityLogError(cleanupFailure.AssemblyUniqueID, cleanupFailure, "Test Assembly Cleanup Failure ({0})", ToAssemblyName(cleanupFailure));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestAssemblyFinished" />.
	/// </summary>
	protected override void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestAssemblyFinished(args);

		var assemblyFinished = args.Message;

		TeamCityLogSuiteFinished(assemblyFinished.AssemblyUniqueID, ToAssemblyName(assemblyFinished));

		metadataCache.TryRemove(assemblyFinished);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestAssemblyStarting" />.
	/// </summary>
	protected override void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestAssemblyStarting(args);

		var assemblyStarting = args.Message;

		metadataCache.Set(assemblyStarting);

		TeamCityLogSuiteStarted(assemblyStarting.AssemblyUniqueID, ToAssemblyName(assemblyStarting), rootFlowId);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCaseCleanupFailure" />.
	/// </summary>
	protected override void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCaseCleanupFailure(args);

		var cleanupFailure = args.Message;

		TeamCityLogError(cleanupFailure.TestCollectionUniqueID, cleanupFailure, "Test Case Cleanup Failure ({0})", ToTestCaseName(cleanupFailure));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCaseFinished" />.
	/// </summary>
	protected override void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCaseFinished(args);

		metadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCaseStarting" />.
	/// </summary>
	protected override void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCaseStarting(args);

		metadataCache.Set(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestClassCleanupFailure" />.
	/// </summary>
	protected override void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestClassCleanupFailure(args);

		var cleanupFailure = args.Message;

		TeamCityLogError(cleanupFailure.TestCollectionUniqueID, cleanupFailure, "Test Class Cleanup Failure ({0})", ToTestClassName(cleanupFailure));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestClassFinished" />.
	/// </summary>
	protected override void HandleTestClassFinished(MessageHandlerArgs<ITestClassFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestClassFinished(args);

		metadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestClassStarting" />.
	/// </summary>
	protected override void HandleTestClassStarting(MessageHandlerArgs<ITestClassStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestClassStarting(args);

		metadataCache.Set(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCollectionCleanupFailure" />.
	/// </summary>
	protected override void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCollectionCleanupFailure(args);

		var cleanupFailure = args.Message;

		TeamCityLogError(cleanupFailure.TestCollectionUniqueID, cleanupFailure, "Test Collection Cleanup Failure ({0})", ToTestCollectionName(cleanupFailure));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCollectionFinished" />.
	/// </summary>
	protected override void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCollectionFinished(args);

		var testCollectionFinished = args.Message;

		TeamCityLogSuiteFinished(testCollectionFinished.TestCollectionUniqueID, ToTestCollectionName(testCollectionFinished));

		metadataCache.TryRemove(testCollectionFinished);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCollectionStarting" />.
	/// </summary>
	protected override void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCollectionStarting(args);

		var testCollectionStarting = args.Message;

		metadataCache.Set(testCollectionStarting);

		TeamCityLogSuiteStarted(testCollectionStarting.TestCollectionUniqueID, ToTestCollectionName(testCollectionStarting), testCollectionStarting.AssemblyUniqueID);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestCleanupFailure" />.
	/// </summary>
	protected override void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestCleanupFailure(args);

		var cleanupFailure = args.Message;

		TeamCityLogError(cleanupFailure.TestCollectionUniqueID, cleanupFailure, "Test Cleanup Failure ({0})", ToTestName(cleanupFailure));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestFailed" />.
	/// </summary>
	protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestFailed(args);

		var testFailed = args.Message;

		TeamCityLogMessage(
			testFailed.TestCollectionUniqueID,
			"testFailed",
			"name='{0}' details='{1}|r|n{2}'",
			TeamCityEscape(ToTestName(testFailed)),
			TeamCityEscape(ExceptionUtility.CombineMessages(testFailed)),
			TeamCityEscape(ExceptionUtility.CombineStackTraces(testFailed))
		);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestFinished" />.
	/// </summary>
	protected override void HandleTestFinished(MessageHandlerArgs<ITestFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestFinished(args);

		var testFinished = args.Message;

		var formattedName = TeamCityEscape(ToTestName(testFinished));
		var flowId = testFinished.TestCollectionUniqueID;

		if (!string.IsNullOrWhiteSpace(testFinished.Output))
			TeamCityLogMessage(flowId, "testStdOut", "name='{0}' out='{1}' tc:tags='tc:parseServiceMessagesInside']", formattedName, TeamCityEscape(testFinished.Output));

		TeamCityLogMessage(flowId, "testFinished", "name='{0}' duration='{1}'", formattedName, (int)(testFinished.ExecutionTime * 1000M));

		metadataCache.TryRemove(testFinished);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestMethodCleanupFailure" />.
	/// </summary>
	protected override void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestMethodCleanupFailure(args);

		var cleanupFailure = args.Message;

		TeamCityLogError(cleanupFailure.TestCollectionUniqueID, cleanupFailure, "Test Method Cleanup Failure ({0})", ToTestMethodName(cleanupFailure));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestMethodFinished" />.
	/// </summary>
	protected override void HandleTestMethodFinished(MessageHandlerArgs<ITestMethodFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestMethodFinished(args);

		metadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestMethodStarting" />.
	/// </summary>
	protected override void HandleTestMethodStarting(MessageHandlerArgs<ITestMethodStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestMethodStarting(args);

		metadataCache.Set(args.Message);
	}

	/// <summary>
	/// Handles instances of <see cref="ITestSkipped" />.
	/// </summary>
	protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestSkipped(args);

		var testSkipped = args.Message;

		TeamCityLogMessage(testSkipped.TestCollectionUniqueID, "testIgnored", "name='{0}' message='{1}'", TeamCityEscape(ToTestName(testSkipped)), TeamCityEscape(testSkipped.Reason));
	}

	/// <summary>
	/// Handles instances of <see cref="ITestStarting" />.
	/// </summary>
	protected override void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestStarting(args);

		var testStarting = args.Message;

		metadataCache.Set(testStarting);

		TeamCityLogMessage(testStarting.TestCollectionUniqueID, "testStarted", "name='{0}'", TeamCityEscape(ToTestName(testStarting)));
	}

	// Helpers

	/// <summary>
	/// Escapes a string to be sent in a TeamCity message.
	/// </summary>
	/// <param name="value">The value to be escaped</param>
	/// <returns>The escaped value</returns>
	[return: NotNullIfNotNull(nameof(value))]
	public static string? TeamCityEscape(string? value)
	{
		if (value is null)
			return null;

		var sb = new StringBuilder(value.Length);

		for (var i = 0; i < value.Length; i++)
		{
			var ch = value[i];

			switch (ch)
			{
				case '\\':
					sb.Append("|0x005C");
					break;
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
					if (ch < '\x007f')
						sb.Append(ch);
					else
					{
						sb.Append("|0x");
						sb.Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
					}
					break;
			}
		}

		return sb.ToString();
	}

	void TeamCityLogError(
		IErrorMetadata errorMetadata,
		string messageType) =>
			TeamCityLogError(string.Empty, errorMetadata, "{0}", messageType);

	void TeamCityLogError(
		string flowId,
		IErrorMetadata errorMetadata,
		string messageTypeFormat,
		params object?[] args)
	{
		var message = string.Format(
			CultureInfo.InvariantCulture,
			"[{0}] {1}: {2}",
			string.Format(CultureInfo.InvariantCulture, messageTypeFormat, args),
			errorMetadata.ExceptionTypes[0],
			ExceptionUtility.CombineMessages(errorMetadata)
		);
		var stackTrace = ExceptionUtility.CombineStackTraces(errorMetadata);

		TeamCityLogMessage(flowId, "message", "status='ERROR' text='{0}' errorDetails='{1}'", TeamCityEscape(message), TeamCityEscape(stackTrace));
	}

	void TeamCityLogMessage(
		string flowId,
		string messageType,
		string extraMetadataFormat = "",
		params object?[] args) =>
			Logger.LogRaw(
				"##teamcity[{0} timestamp='{1}+0000'{2}{3}]",
				messageType,
				TeamCityEscape(UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture)),
				flowId.Length != 0 ? string.Format(CultureInfo.InvariantCulture, " flowId='{0}'", TeamCityEscape(flowId)) : "",
				extraMetadataFormat.Length != 0 ? " " + string.Format(CultureInfo.InvariantCulture, extraMetadataFormat, args) : ""
			);

	void TeamCityLogSuiteFinished(
		string flowId,
		string name)
	{
		TeamCityLogMessage(flowId, "testSuiteFinished", "name='{0}'", TeamCityEscape(name));
		TeamCityLogMessage(flowId, "flowFinished");
	}

	void TeamCityLogSuiteStarted(
		string flowId,
		string name,
		string? parentFlowId = null)
	{
		TeamCityLogMessage(flowId, "flowStarted", parentFlowId is null ? "" : "parent='{0}'", TeamCityEscape(parentFlowId));
		TeamCityLogMessage(flowId, "testSuiteStarted", "name='{0}'", TeamCityEscape(name));
	}

	string ToAssemblyName(ITestAssemblyMessage message)
	{
		var metadata = metadataCache.TryGetAssemblyMetadata(message);

		return
			metadata is not null
				? metadata.AssemblyPath ?? metadata.SimpleAssemblyName()
				: "<unknown test assembly>";
	}

	string ToTestCaseName(ITestCaseMessage message) =>
		metadataCache.TryGetTestCaseMetadata(message)?.TestCaseDisplayName ?? "<unknown test case>";

	string ToTestClassName(ITestClassMessage message) =>
		metadataCache.TryGetClassMetadata(message)?.TestClassName ?? "<unknown test class>";

	string ToTestCollectionName(ITestCollectionMessage message)
	{
		var metadata = metadataCache.TryGetCollectionMetadata(message);

		return
			metadata is not null
				? string.Format(CultureInfo.InvariantCulture, "{0} ({1})", metadata.TestCollectionDisplayName, message.TestCollectionUniqueID)
				: "<unknown test collection>";
	}

	string ToTestMethodName(ITestMethodMessage message)
	{
		var testMethodMetadata = metadataCache.TryGetMethodMetadata(message);
		if (testMethodMetadata is null)
			return "<unknown test method>";

		var testClassMetadata = metadataCache.TryGetClassMetadata(message);
		return
			testClassMetadata is not null
				? string.Format(CultureInfo.InvariantCulture, "{0}.{1}", testClassMetadata.TestClassName, testMethodMetadata.MethodName)
				: testMethodMetadata.MethodName;
	}

	string ToTestName(ITestMessage message) =>
		// TODO: Is there a way to get just the component pieces of the display name?
		// That way we could construct the method name and arguments separately.
		metadataCache.TryGetTestMetadata(message)?.TestDisplayName ?? "<unknown test>";
}
