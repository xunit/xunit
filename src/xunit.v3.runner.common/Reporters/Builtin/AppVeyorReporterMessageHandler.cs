using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="AppVeyorReporter" />.
/// </summary>
public class AppVeyorReporterMessageHandler : DefaultRunnerReporterMessageHandler
{
	const int MaxLength = 4096;

	int assembliesInFlight;
	readonly ConcurrentDictionary<string, (string assemblyFileName, Dictionary<string, int> testMethods)> assemblyInfoByUniqueID = new();
	readonly string baseUri;
	AppVeyorClient? client;
	readonly object clientLock = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="AppVeyorReporterMessageHandler" /> class.
	/// </summary>
	/// <param name="logger">The logger used to report messages</param>
	/// <param name="baseUri">The base AppVeyor API URI</param>
	public AppVeyorReporterMessageHandler(
		IRunnerLogger logger,
		string baseUri)
			: base(logger)
	{
		Guard.ArgumentNotNull(baseUri);

		this.baseUri = baseUri.TrimEnd('/');
	}

	AppVeyorClient Client
	{
		get
		{
			lock (clientLock)
				client ??= new AppVeyorClient(Logger, baseUri);

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
				Logger.LogWarning(
					string.Format(
						CultureInfo.CurrentCulture,
						"{0} disposed with {1} assemblies in flight",
						nameof(AppVeyorReporterMessageHandler),
						assembliesInFlight
					)
				);
		}
	}

	/// <inheritdoc/>
	protected override void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestAssemblyFinished(args);

		lock (clientLock)
		{
			assembliesInFlight--;

			if (assembliesInFlight == 0)
			{
				client?.SafeDispose();
				client = null;
			}
		}
	}

	/// <inheritdoc/>
	protected override void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestAssemblyStarting(args);

		lock (clientLock)
		{
			assembliesInFlight++;

			// Use the TFM attrib to disambiguate
			var tfm = args.Message.TargetFramework;
			var assemblyFileName = Path.GetFileName(args.Message.AssemblyPath) ?? "<unknown filename>";
			if (!string.IsNullOrWhiteSpace(tfm))
				assemblyFileName = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", assemblyFileName, tfm);

			assemblyInfoByUniqueID[args.Message.AssemblyUniqueID] = (assemblyFileName, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
		}
	}

	/// <inheritdoc/>
	protected override void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
	{
		Guard.ArgumentNotNull(args);

		base.HandleTestStarting(args);

		var testName = args.Message.TestDisplayName;
		var testMethods = assemblyInfoByUniqueID[args.Message.AssemblyUniqueID].testMethods;

		lock (testMethods)
			if (testMethods.TryGetValue(testName, out var testIdx))
				testName = string.Format(CultureInfo.InvariantCulture, "{0} {1}", testName, testIdx);

		Client.AddTest(GetRequestMessage(
			testName,
			"xUnit",
			assemblyInfoByUniqueID[args.Message.AssemblyUniqueID].assemblyFileName,
			"Running"
		));
	}

	/// <inheritdoc/>
	protected override void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		Guard.ArgumentNotNull(args);

		var testPassed = args.Message;
		var metadata = MetadataCache.TryGetTestMetadata(testPassed);
		if (metadata is not null)
		{
			var testMethods = assemblyInfoByUniqueID[testPassed.AssemblyUniqueID].testMethods;

			Client.UpdateTest(GetRequestMessage(
				GetFinishedTestName(metadata.TestDisplayName, testMethods),
				"xUnit",
				assemblyInfoByUniqueID[testPassed.AssemblyUniqueID].assemblyFileName,
				"Passed",
				Convert.ToInt64(testPassed.ExecutionTime * 1000),
				stdOut: testPassed.Output
			));
		}
		// TODO: What to do when metadata lookup fails?

		base.HandleTestPassed(args);
	}

	/// <inheritdoc/>
	protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		Guard.ArgumentNotNull(args);

		var testSkipped = args.Message;
		var metadata = MetadataCache.TryGetTestMetadata(testSkipped);
		if (metadata is not null)
		{
			var testMethods = assemblyInfoByUniqueID[testSkipped.AssemblyUniqueID].testMethods;

			Client.UpdateTest(GetRequestMessage(
				GetFinishedTestName(metadata.TestDisplayName, testMethods),
				"xUnit",
				assemblyInfoByUniqueID[testSkipped.AssemblyUniqueID].assemblyFileName,
				"Skipped",
				Convert.ToInt64(testSkipped.ExecutionTime * 1000),
				stdOut: testSkipped.Output
			));
		}
		// TODO: What to do when metadata lookup fails?

		base.HandleTestSkipped(args);
	}

	/// <inheritdoc/>
	protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		Guard.ArgumentNotNull(args);

		var testFailed = args.Message;
		var metadata = MetadataCache.TryGetTestMetadata(testFailed);
		if (metadata is not null)
		{
			var testMethods = assemblyInfoByUniqueID[testFailed.AssemblyUniqueID].testMethods;

			Client.UpdateTest(GetRequestMessage(
				GetFinishedTestName(metadata.TestDisplayName, testMethods),
				"xUnit",
				assemblyInfoByUniqueID[testFailed.AssemblyUniqueID].assemblyFileName,
				"Failed",
				Convert.ToInt64(testFailed.ExecutionTime * 1000),
				ExceptionUtility.CombineMessages(testFailed),
				ExceptionUtility.CombineStackTraces(testFailed),
				testFailed.Output
			));
		}
		// TODO: What to do when metadata lookup fails?

		base.HandleTestFailed(args);
	}

	// AppVeyor API helpers

	static string GetFinishedTestName(
		string methodName,
		Dictionary<string, int> testMethods)
	{
		lock (testMethods)
		{
			var testName = methodName;

			if (testMethods.TryGetValue(methodName, out var number))
				testName = string.Format(CultureInfo.InvariantCulture, "{0} {1}", methodName, number);

			testMethods[methodName] = number + 1;
			return testName;
		}
	}

	// If this method is ever changed to support value types other than string and long, you must update
	// AppVeyorClient.ToJson() to ensure the types are identified and serialized correctly.
	static Dictionary<string, object?> GetRequestMessage(
		string testName,
		string testFramework,
		string fileName,
		string outcome,
		long? durationMilliseconds = null,
		string? errorMessage = null,
		string? errorStackTrace = null,
		string? stdOut = null) =>
			new()
			{
				{ "testName", testName },
				{ "testFramework", testFramework },
				{ "fileName", fileName },
				{ "outcome", outcome },
				{ "durationMilliseconds", durationMilliseconds },
				{ "ErrorMessage", errorMessage },
				{ "ErrorStackTrace", errorStackTrace },
				{ "StdOut", stdOut?.Length > MaxLength ? stdOut.Substring(0, MaxLength) : stdOut },
			};
}
