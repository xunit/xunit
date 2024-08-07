using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="IMessageSink"/> and <see cref="IDataProducer"/> in order to convert
/// xUnit.net v3 execution messages into Microsoft.Testing.Platform <see cref="TestNode"/> messages.
/// </summary>
/// <param name="innerSink">The inner sink to delegate messages to</param>
/// <param name="sessionUid">The MTP session UID</param>
/// <param name="testNodeMessageBus">The message bus for reporting MTP messages to</param>
/// <param name="trxCapability">Information class that knows whether TRX reporting is enabled</param>
/// <param name="outputDevice">The output device to write messages to</param>
/// <param name="showLiveOutput">A flag to indicate whether live output should be shown</param>
/// <param name="cancellationToken">The cancellation token provided by MTP</param>
[ExcludeFromCodeCoverage]
public class TestPlatformExecutionMessageSink(
	IMessageSink innerSink,
	SessionUid sessionUid,
	IMessageBus testNodeMessageBus,
	XunitTrxCapability trxCapability,
	IOutputDevice outputDevice,
	bool showLiveOutput,
	CancellationToken cancellationToken) :
		OutputDeviceDataProducerBase("execution message sink", "fa7e6681-c892-4741-9980-724bd818f1f1"), IMessageSink, IDataProducer
{
	readonly MessageMetadataCache metadataCache = new();

	/// <inheritdoc/>
	public Type[] DataTypesProduced =>
		[typeof(TestNodeUpdateMessage)];

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		var result = innerSink.OnMessage(message);

		return
			message.DispatchWhen<ITestAssemblyFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestAssemblyStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestCaseFinished>(args => metadataCache.TryRemove(args.Message)) &&
			message.DispatchWhen<ITestCaseStarting>(args => metadataCache.Set(args.Message)) &&
			message.DispatchWhen<ITestFailed>(args => SendTestResult(args.Message)) &&
			message.DispatchWhen<ITestFinished>(args => metadataCache.TryRemove(args.Message)) &&
			// We don't report anything for ITestNotRun, because we don't want to alter the user's expectations
			// of what happens for not run tests in Test Explorer. We want them to stay marked as "not run" (or
			// show their previous run value but not be highlighted as "run this time").
			message.DispatchWhen<ITestPassed>(args => SendTestResult(args.Message)) &&
			message.DispatchWhen<ITestSkipped>(args => SendTestResult(args.Message)) &&
			message.DispatchWhen<ITestStarting>(args => SendTestResult(args.Message)) &&
			message.DispatchWhen<ITestOutput>(OnLiveOutput) &&
			result &&
			!cancellationToken.IsCancellationRequested;
	}

	void OnLiveOutput(MessageHandlerArgs<ITestOutput> args)
	{
		if (!showLiveOutput)
			return;

		var testOutput = args.Message;
		var testMetadata = metadataCache.TryGetTestMetadata(testOutput);

		outputDevice.DisplayAsync(this, ToMessageWithColor(string.Format(CultureInfo.CurrentCulture, "OUTPUT: [{0}] {1}", testMetadata?.TestDisplayName ?? "<unknown test>", testOutput.Output.TrimEnd()), ConsoleColor.DarkGray)).SpinWait();
	}

	void SendTestResult(ITestMessage testMessage)
	{
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
					outputDevice.DisplayAsync(this, ToMessageWithColor(string.Format(CultureInfo.CurrentCulture, "WARNING: [{0}] {1}", testStarting.TestDisplayName, warning), ConsoleColor.Yellow)).SpinWait();
		}

		var testAssemblyMetadata = metadataCache.TryGetAssemblyMetadata(testMessage);
		var testCaseMetadata = metadataCache.TryGetTestCaseMetadata(testMessage);
		if (testAssemblyMetadata is not null && testCaseMetadata is not null && testStarting is not null)
		{
			result.AddMetadata(testCaseMetadata, testAssemblyMetadata.AssemblyName, testResult: testResult, traits: testStarting.Traits);

			if (trxCapability.IsTrxEnabled)
				result.AddTrxMetadata(testCaseMetadata, testMessage, testStarting.Traits);
		}

		result.SendUpdate(this, sessionUid, testNodeMessageBus);
	}
}
