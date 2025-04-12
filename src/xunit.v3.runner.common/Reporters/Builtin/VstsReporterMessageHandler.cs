using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="VstsReporter" />.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VstsReporterMessageHandler" /> class.
/// </remarks>
/// <param name="logger">The logger used to report messages</param>
/// <param name="baseUri">The base URI for talking to Azure DevOps/VSTS</param>
/// <param name="accessToken">The access token required to talk to Azure DevOps/VSTS</param>
/// <param name="buildId">The ID of build that's currently being run</param>
public class VstsReporterMessageHandler(
	IRunnerLogger logger,
	string baseUri,
	string accessToken,
	int buildId) :
		DefaultRunnerReporterMessageHandler(logger)
{
	const int MaxLength = 4096;

	readonly string accessToken = Guard.ArgumentNotNull(accessToken);
	int assembliesInFlight;
	readonly string baseUri = Guard.ArgumentNotNull(baseUri);
	readonly int buildId = buildId;
	VstsClient? client;
	readonly object clientLock = new();

	VstsClient Client
	{
		get
		{
			lock (clientLock)
				client ??= new VstsClient(Logger, baseUri, accessToken, buildId);

			return client;
		}
	}

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		await base.DisposeAsync();

		GC.SuppressFinalize(this);

		lock (clientLock)
		{
			client?.SafeDispose();
			client = null;

			if (assembliesInFlight != 0)
				Logger.LogWarning("{0} disposed with {1} assemblies in flight", nameof(VstsReporterMessageHandler), assembliesInFlight);
		}
	}

	/// <inheritdoc/>
	protected override void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
	{
		base.HandleTestAssemblyFinished(args);

		lock (clientLock)
		{
			assembliesInFlight--;

			if (assembliesInFlight == 0)
			{
				// Drain the queue
				client?.SafeDispose();
				client = null;
			}
		}
	}

	/// <inheritdoc/>
	protected override void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
	{
		base.HandleTestAssemblyStarting(args);

		lock (clientLock)
			assembliesInFlight++;
	}

	/// <inheritdoc/>
	protected override void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestStarting(args);

		var testStarting = args.Message;
		var assemblyMetadata = MetadataCache.TryGetAssemblyMetadata(testStarting);
		var classMetadata = MetadataCache.TryGetClassMetadata(testStarting);
		var methodMetadata = MetadataCache.TryGetMethodMetadata(testStarting);

		if (assemblyMetadata is not null && classMetadata is not null && methodMetadata is not null)
		{
			var testName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", classMetadata.TestClassName, methodMetadata.MethodName);

			VstsAddTest(testName, testStarting.TestDisplayName, assemblyMetadata.SimpleAssemblyName(), testStarting.TestUniqueID);
		}
	}

	/// <inheritdoc/>
	protected override void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		Guard.ArgumentNotNull(args);

		var testPassed = args.Message;

		VstsUpdateTest(testPassed.TestUniqueID, "Passed", Convert.ToInt64(testPassed.ExecutionTime * 1000), stdOut: testPassed.Output);

		base.HandleTestPassed(args);
	}

	/// <inheritdoc/>
	protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		Guard.ArgumentNotNull(args);

		var testSkipped = args.Message;

		VstsUpdateTest(testSkipped.TestUniqueID, "NotExecuted", Convert.ToInt64(testSkipped.ExecutionTime * 1000));

		base.HandleTestSkipped(args);
	}

	/// <inheritdoc/>
	protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		Guard.ArgumentNotNull(args);

		var testFailed = args.Message;

		VstsUpdateTest(
			testFailed.TestUniqueID,
			"Failed",
			Convert.ToInt64(testFailed.ExecutionTime * 1000),
			ExceptionUtility.CombineMessages(testFailed),
			ExceptionUtility.CombineStackTraces(testFailed),
			testFailed.Output
		);

		base.HandleTestFailed(args);
	}

	// If this method is ever changed to support value types other than string and DateTimeOffset, you must update
	// VstsClient.ToJson() to ensure the types are identified and serialized correctly.
	void VstsAddTest(
		string testName,
		string displayName,
		string fileName,
		string testUniqueID)
	{
		var body = new Dictionary<string, object?>
		{
			{ "testCaseTitle", displayName },
			{ "automatedTestName", testName },
			{ "automatedTestType", "UnitTest" },
			{ "automatedTestTypeId", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" }, // This is used in the sample response and also appears in web searches
			{ "automatedTestId", testUniqueID },
			{ "automatedTestStorage", fileName },
			{ "state", "InProgress" },
			{ "startedDate", DateTimeOffset.UtcNow }
		};

		Client.AddTest(body, testUniqueID);
	}

	// If this method is ever changed to support value types other than string and long, you must update
	// VstsClient.ToJson() to ensure the types are identified and serialized correctly.
	void VstsUpdateTest(
		string testUniqueID,
		string outcome,
		long? durationMilliseconds = null,
		string? errorMessage = null,
		string? errorStackTrace = null,
		string? stdOut = null)
	{
		var body = new Dictionary<string, object?>
		{
			{ "outcome", outcome },
			{ "durationInMs", durationMilliseconds },
			{ "state", "Completed" }
		};

		var msg = string.Format(CultureInfo.InvariantCulture, "{0}\n{1}\n{2}", errorMessage, errorStackTrace, TrimStdOut(stdOut)).Trim();

		if (!string.IsNullOrWhiteSpace(msg))
			body.Add("errorMessage", msg);

		Client.UpdateTest(body, testUniqueID);
	}

	static string? TrimStdOut(string? str) =>
		str?.Length > MaxLength ? str.Substring(0, MaxLength) : str;
}
