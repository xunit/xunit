using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.MicrosoftTestingPlatform;

/// <summary>
/// Implementation of <see cref="IMessageSink"/> and <see cref="IDataProducer"/> in order to convert
/// xUnit.net v3 execution messages into Microsoft.Testing.Platform <see cref="TestNode"/> messages.
/// </summary>
/// <param name="innerSink">The inner sink to delegate messages to</param>
/// <param name="assemblyFullName">The full name of the test assembly (via <see cref="Assembly.FullName"/>)</param>
/// <param name="sessionUid">The MTP session UID</param>
/// <param name="testNodeMessageBus">The message bus for reporting MTP messages to</param>
/// <param name="trxCapability">Information class that knows whether TRX reporting is enabled</param>
/// <param name="outputDevice">The output device to write messages to</param>
/// <param name="configuration">The MTP configuration</param>
/// <param name="showLiveOutput">A flag to indicate whether live output should be shown</param>
/// <param name="serverMode">A flag to indicate if we're running in server mode (inside Test Explorer)</param>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TestPlatformExecutionMessageSink(
	IMessageSink innerSink,
	string assemblyFullName,
	SessionUid sessionUid,
	IMessageBus testNodeMessageBus,
	XunitTrxCapability trxCapability,
	IOutputDevice outputDevice,
	IConfiguration configuration,
	bool showLiveOutput,
	bool serverMode) :
		OutputDeviceDataProducerBase("execution message sink", "fa7e6681-c892-4741-9980-724bd818f1f1"), IMessageSink, IDataProducer
{
	readonly MessageMetadataCache metadataCache = new();
	readonly ConcurrentDictionary<string, TestNode> testNodesByTestID = [];
	readonly ConcurrentDictionary<string, ConcurrentBag<ITestStarting>> testsByAssemblyID = [];
	readonly ConcurrentDictionary<string, ConcurrentBag<ITestStarting>> testsByCaseID = [];
	readonly ConcurrentDictionary<string, ConcurrentBag<ITestStarting>> testsByClassID = [];
	readonly ConcurrentDictionary<string, ConcurrentBag<ITestStarting>> testsByCollectionID = [];
	readonly ConcurrentDictionary<string, ConcurrentBag<ITestStarting>> testsByMethodID = [];
	readonly ConcurrentDictionary<string, ITestStarting> testsByTestID = [];

	/// <inheritdoc/>
	public Type[] DataTypesProduced =>
		[typeof(TestNodeUpdateMessage)];

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		var result = innerSink.OnMessage(message);

		return
			message.DispatchWhen<ITestAssemblyCleanupFailure>(args => OnTestAssemblyCleanupFailure(args.Message)) &&
			message.DispatchWhen<ITestAssemblyFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestAssemblyStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestCaseCleanupFailure>(args => OnTestCaseCleanupFailure(args.Message)) &&
			message.DispatchWhen<ITestCaseFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestCaseStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestClassCleanupFailure>(args => OnTestClassCleanupFailure(args.Message)) &&
			message.DispatchWhen<ITestClassFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestClassStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestCleanupFailure>(args => OnTestCleanupFailure(args.Message)) &&
			message.DispatchWhen<ITestCollectionCleanupFailure>(args => OnTestCollectionCleanupFailure(args.Message)) &&
			message.DispatchWhen<ITestCollectionFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestCollectionStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestFailed>(args => SendTestNodeUpdate(args.Message)) &&
			message.DispatchWhen<ITestFinished>(args => OnTestFinished(args.Message)) &&
			message.DispatchWhen<ITestMethodCleanupFailure>(args => OnTestMethodCleanupFailure(args.Message)) &&
			message.DispatchWhen<ITestMethodFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestMethodStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestNotRun>(args => SendTestNodeUpdate(args.Message)) &&
			message.DispatchWhen<ITestPassed>(args => SendTestNodeUpdate(args.Message)) &&
			message.DispatchWhen<ITestSkipped>(args => SendTestNodeUpdate(args.Message)) &&
			message.DispatchWhen<ITestStarting>(args => OnTestStarting(args.Message)) &&
			message.DispatchWhen<ITestOutput>(args => OnLiveOutput(args.Message)) &&
			result;
	}

	void OnLiveOutput(ITestOutput testOutput)
	{
		if (!showLiveOutput)
			return;

		var testMetadata = metadataCache.TryGetTestMetadata(testOutput);

		outputDevice.DisplayAsync(
			this,
			ToMessageWithColor(
				string.Format(
					CultureInfo.CurrentCulture,
					"OUTPUT: [{0}] {1}",
					testMetadata?.TestDisplayName ?? "<unknown test>",
					testOutput.Output.TrimEnd()
				),
				ConsoleColor.DarkGray
			),
			TestContext.Current.CancellationToken
		).SpinWait();
	}

	void OnTestAssemblyCleanupFailure(ITestAssemblyCleanupFailure failure)
	{
		var header = string.Format(
			CultureInfo.InvariantCulture,
			"[Test Assembly Cleanup Failure ({0})]",
			metadataCache.TryGetAssemblyMetadata(failure)?.AssemblyPath ?? "<unknown test assembly>"
		);

		if (testsByAssemblyID.TryGetValue(failure.AssemblyUniqueID, out var tests))
			SendError(header, tests, failure);
		else
			SendError(header, failure);
	}

	void OnTestCaseCleanupFailure(ITestCaseCleanupFailure failure)
	{
		if (testsByCaseID.TryGetValue(failure.TestCaseUniqueID, out var tests))
			SendError("[Test Case Cleanup Failure]", tests, failure);
		else
			SendError("[Test Case Cleanup Failure]", failure, failure.TestCaseUniqueID);
	}

	void OnTestClassCleanupFailure(ITestClassCleanupFailure failure)
	{
		var header = string.Format(
			CultureInfo.InvariantCulture,
			"[Test Class Cleanup Failure ({0})]",
			metadataCache.TryGetClassMetadata(failure)?.TestClassName ?? "<unknown test class>"
		);

		if (failure.TestClassUniqueID is not null && testsByClassID.TryGetValue(failure.TestClassUniqueID, out var tests))
			SendError(header, tests, failure);
		else
			SendError(header, failure);
	}

	void OnTestCleanupFailure(ITestCleanupFailure failure)
	{
		if (testsByTestID.TryGetValue(failure.TestUniqueID, out var test))
			SendError("[Test Cleanup Failure]", [test], failure);
		else
			SendError("[Test Cleanup Failure]", failure, failure.TestCaseUniqueID);
	}

	void OnTestCollectionCleanupFailure(ITestCollectionCleanupFailure failure)
	{
		var header = string.Format(
			CultureInfo.InvariantCulture,
			"[Test Collection Cleanup Failure ({0})]",
			metadataCache.TryGetCollectionMetadata(failure)?.TestCollectionDisplayName ?? "<unknown test collection>"
		);

		if (testsByCollectionID.TryGetValue(failure.TestCollectionUniqueID, out var tests))
			SendError(header, tests, failure);
		else
			SendError(header, failure);
	}

	void OnTestFinished(ITestFinished testFinished)
	{
		var testUniqueID = testFinished.TestUniqueID;

		if (testNodesByTestID.TryRemove(testUniqueID, out var testNode))
		{
			if (testFinished.Attachments.Count != 0)
				try
				{
					var basePath = Path.Combine(configuration.GetTestResultDirectory(), testUniqueID);
					Directory.CreateDirectory(basePath);

					foreach (var kvp in testFinished.Attachments)
					{
						var localFilePath = default(string);

						try
						{
							var attachmentType = kvp.Value.AttachmentType;

							if (attachmentType == TestAttachmentType.String)
							{
								localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(kvp.Key, "text/plain"));
								File.WriteAllText(localFilePath, kvp.Value.AsString());
							}
							else if (attachmentType == TestAttachmentType.ByteArray)
							{
								var (byteArray, mediaType) = kvp.Value.AsByteArray();
								localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(kvp.Key, mediaType));
								File.WriteAllBytes(localFilePath, byteArray);
							}
							else
								outputDevice.DisplayAsync(
									this,
									ToMessageWithColor(
										string.Format(
											CultureInfo.CurrentCulture,
											"[{0}] Unknown test attachment type '{1}' for attachment '{2}'",
											testNode.DisplayName,
											attachmentType,
											kvp.Key
										),
										ConsoleColor.Yellow
									),
									TestContext.Current.CancellationToken
								).SpinWait();

							if (localFilePath is not null)
								testNode.Properties.Add(new FileArtifactProperty(new FileInfo(localFilePath), kvp.Key));
						}
						catch (Exception ex)
						{
							outputDevice.DisplayAsync(
								this,
								ToMessageWithColor(
									string.Format(
										CultureInfo.CurrentCulture,
										"[{0}] Exception while adding attachment '{1}' in '{2}': {3}",
										testNode.DisplayName,
										kvp.Key,
										localFilePath,
										ex
									),
									ConsoleColor.Yellow
								),
								TestContext.Current.CancellationToken
							).SpinWait();
						}
					}
				}
				catch (Exception ex)
				{
					outputDevice.DisplayAsync(
						this,
						ToMessageWithColor(
							string.Format(
								CultureInfo.CurrentCulture,
								"[{0}] Exception while adding attachments: {1}",
								testNode.DisplayName,
								ex
							),
							ConsoleColor.Yellow
						),
						TestContext.Current.CancellationToken
					).SpinWait();
				}

			testNode.SendUpdate(this, sessionUid, testNodeMessageBus);
		}

		metadataCache.TryRemove(testFinished);
	}

	void OnTestMethodCleanupFailure(ITestMethodCleanupFailure failure)
	{
		var methodMetadata = metadataCache.TryGetMethodMetadata(failure);
		var classMetadata = metadataCache.TryGetClassMetadata(failure);
		var displayName =
			methodMetadata is not null && classMetadata is not null
				? string.Format(CultureInfo.InvariantCulture, "{0}.{1}", classMetadata.TestClassName, methodMetadata.MethodName)
				: "<unknown test method>";
		var header = string.Format(CultureInfo.InvariantCulture, "[Test Method Cleanup Failure ({0})]", displayName);

		if (failure.TestMethodUniqueID is not null && testsByMethodID.TryGetValue(failure.TestMethodUniqueID, out var tests))
			SendError(header, tests, failure);
		else
			SendError(header, failure);
	}

	void OnTestStarting(ITestStarting testStarting)
	{
		testsByAssemblyID.TryAdd(testStarting.AssemblyUniqueID, testStarting);
		testsByCaseID.TryAdd(testStarting.TestCaseUniqueID, testStarting);
		if (testStarting.TestClassUniqueID is not null)
			testsByClassID.TryAdd(testStarting.TestClassUniqueID, testStarting);
		testsByCollectionID.TryAdd(testStarting.TestCollectionUniqueID, testStarting);
		if (testStarting.TestMethodUniqueID is not null)
			testsByMethodID.TryAdd(testStarting.TestMethodUniqueID, testStarting);
		testsByTestID.TryAdd(testStarting.TestUniqueID, testStarting);

		SendTestNodeUpdate(testStarting);
	}

	void SendError(
		string header,
		IReadOnlyCollection<ITestStarting> tests,
		IErrorMetadata errorMetadata)
	{
		var exception = new XunitException(errorMetadata, header);

		foreach (var test in tests)
			SendError(exception, test.TestCaseUniqueID, test.TestDisplayName);
	}

	void SendError(
		string header,
		IErrorMetadata errorMetadata,
		string? testCaseUniqueID = null) =>
			SendError(new XunitException(errorMetadata, header), testCaseUniqueID);

	void SendError(
		Exception exception,
		string? testCaseUniqueID,
		string? displayName = null)
	{
		var testCaseMetadata =
			testCaseUniqueID is not null
				? metadataCache.TryGetTestCaseMetadata(testCaseUniqueID)
				: null;

		var errorNode = new TestNode
		{
			Uid = testCaseUniqueID ?? Guid.NewGuid().ToString(),
			DisplayName = displayName ?? testCaseMetadata?.TestCaseDisplayName ?? "Non-test cleanup failure"
		};
		errorNode.Properties.Add(new ErrorTestNodeStateProperty(exception));

		if (testCaseMetadata is not null)
			errorNode.AddMetadata(testCaseMetadata, assemblyFullName);

		errorNode.SendUpdate(this, sessionUid, testNodeMessageBus);
	}

	void SendTestNodeUpdate(ITestMessage testMessage)
	{
		// If we're running inside Visual Studio, we don't want to report the test as skipped,
		// because we want their UI to continue to say "not run" (or report the last run status).
		// We only way to report the test as skipped when running from non-server mode ("dotnet test").
		if (serverMode && testMessage is ITestNotRun)
			return;

		var testStarting = testMessage as ITestStarting;
		if (testStarting is null)
			testStarting = metadataCache.TryGetTestMetadata(testMessage) as ITestStarting;
		else
			metadataCache.Set(testStarting);

		var result = new TestNode
		{
			Uid = testMessage.TestCaseUniqueID,
			DisplayName = testStarting?.TestDisplayName ?? "<unknown test display name>",
		};

		var nodeState = testMessage switch
		{
			ITestFailed failed => failed.Cause switch
			{
				FailureCause.Assertion => new FailedTestNodeStateProperty(new XunitException(failed)),
				FailureCause.Timeout => new TimeoutTestNodeStateProperty(new XunitException(failed)),
				_ => new ErrorTestNodeStateProperty(new XunitException(failed)),
			},
			ITestNotRun => new SkippedTestNodeStateProperty("Not run (due to explicit test filtering)"),
			ITestPassed => PassedTestNodeStateProperty.CachedInstance,
			ITestSkipped skipped => new SkippedTestNodeStateProperty(skipped.Reason),
			ITestStarting => InProgressTestNodeStateProperty.CachedInstance,
			_ => default(IProperty),
		};
		if (nodeState is not null)
			result.Properties.Add(nodeState);

		var testResult = testMessage as ITestResultMessage;
		if (testStarting is not null && testResult is not null)
		{
			result.Properties.Add(new TimingProperty(new TimingInfo(testStarting.StartTime, testResult.FinishTime, TimeSpan.FromSeconds((double)testResult.ExecutionTime))));

			if (testResult.Warnings is not null)
				foreach (var warning in testResult.Warnings)
					outputDevice.DisplayAsync(
						this,
						ToMessageWithColor(
							string.Format(
								CultureInfo.CurrentCulture,
								"WARNING: [{0}] {1}",
								testStarting.TestDisplayName,
								warning
							),
							ConsoleColor.Yellow
						),
						TestContext.Current.CancellationToken
					).SpinWait();
		}

		var testAssemblyMetadata = metadataCache.TryGetAssemblyMetadata(testMessage);
		var testCaseMetadata = metadataCache.TryGetTestCaseMetadata(testMessage);
		if (testAssemblyMetadata is not null && testCaseMetadata is not null && testStarting is not null)
		{
			result.AddMetadata(testCaseMetadata, testAssemblyMetadata.AssemblyName, testResult: testResult, traits: testStarting.Traits);

			if (trxCapability.IsTrxEnabled)
				result.AddTrxMetadata(testCaseMetadata, testMessage, testStarting.Traits);
		}

		// Don't send test results, because we need to wait for test finished for attachments
		if (testResult is null)
			result.SendUpdate(this, sessionUid, testNodeMessageBus);

		if (testMessage is not ITestStarting)
			testNodesByTestID.TryAdd(testMessage.TestUniqueID, result);
	}
}
