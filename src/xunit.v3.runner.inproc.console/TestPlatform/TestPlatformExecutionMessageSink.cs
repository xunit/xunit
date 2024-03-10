using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.InProc.SystemConsole.TestPlatform;

internal sealed class TestPlatformExecutionMessageSink : _IMessageSink, IDataProducer
{
	readonly _IMessageSink innerSink;
	readonly MessageMetadataCache metadataCache = new();
	readonly RunTestExecutionRequest request;
	readonly ExecuteRequestContext requestContext;
	readonly TestSessionContext testSessionContext;

	public TestPlatformExecutionMessageSink(
		_IMessageSink innerSink,
		TestSessionContext testSessionContext,
		ExecuteRequestContext requestContext,
		RunTestExecutionRequest request)
	{
		this.innerSink = innerSink;
		this.testSessionContext = testSessionContext;
		this.requestContext = requestContext;
		this.request = request;
	}

	public Type[] DataTypesProduced =>
		[typeof(TestNodeUpdateMessage)];

	// TODO: Should this be the same as TestPlatformTestFramework?
	public string Description =>
		"Microsoft.Testing.Platform message converter for xUnit.net v3";

	// TODO: Should this be the same as TestPlatformTestFramework?
	public string DisplayName =>
		"xUnit.net";

	// TODO: Should this be the same as TestPlatformTestFramework?
	public string Uid =>
		"fa7e6681-c892-4741-9980-724bd818f1f1";

	public string Version => ThisAssembly.AssemblyVersion;

	public Task<bool> IsEnabledAsync() =>
		Task.FromResult(true);

	public bool OnMessage(_MessageSinkMessage message)
	{
		var result = innerSink.OnMessage(message);

		return
			message.DispatchWhen<_TestCaseFinished>(OnTestCaseFinished) &&
			message.DispatchWhen<_TestCaseStarting>(OnTestCaseStarting) &&
			message.DispatchWhen<_TestFailed>(OnTestFailed) &&
			message.DispatchWhen<_TestFinished>(OnTestFinished) &&
			message.DispatchWhen<_TestNotRun>(OnTestNotRun) &&
			message.DispatchWhen<_TestPassed>(OnTestPassed) &&
			message.DispatchWhen<_TestSkipped>(OnTestSkipped) &&
			message.DispatchWhen<_TestStarting>(OnTestStarting) &&
			result &&
			!requestContext.CancellationToken.IsCancellationRequested;
	}

	void OnTestCaseFinished(MessageHandlerArgs<_TestCaseFinished> args) =>
		metadataCache.TryRemove(args.Message);

	void OnTestCaseStarting(MessageHandlerArgs<_TestCaseStarting> args) =>
		metadataCache.Set(args.Message);

	void OnTestFailed(MessageHandlerArgs<_TestFailed> args) =>
		SendNodeUpdate(ToNode(args.Message, new FailedTestNodeStateProperty(new XunitException(args.Message))));

	void OnTestFinished(MessageHandlerArgs<_TestFinished> args) =>
		metadataCache.TryRemove(args.Message);

	// TODO: The way explicit tests are reported may change. https://github.com/microsoft/testfx/issues/2538
	void OnTestNotRun(MessageHandlerArgs<_TestNotRun> args) =>
		SendNodeUpdate(ToNode(args.Message, new SkippedTestNodeStateProperty("Test was not run because it's marked as explicit")));

	void OnTestPassed(MessageHandlerArgs<_TestPassed> args) =>
		SendNodeUpdate(ToNode(args.Message, PassedTestNodeStateProperty.CachedInstance));

	void OnTestSkipped(MessageHandlerArgs<_TestSkipped> args) =>
		SendNodeUpdate(ToNode(args.Message, new SkippedTestNodeStateProperty(args.Message.Reason)));

	void OnTestStarting(MessageHandlerArgs<_TestStarting> args)
	{
		metadataCache.Set(args.Message);
		SendNodeUpdate(ToNode(args.Message, InProgressTestNodeStateProperty.CachedInstance));
	}

	void SendNodeUpdate(TestNode testNode) =>
		requestContext.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(request.Session.SessionUid, testNode)).SpinWait();

	TestNode ToNode(
		_TestMessage test,
		params IProperty[] properties)
	{
		var testMetadata = metadataCache.TryGetTestMetadata(test);
		var testCaseMetadata = metadataCache.TryGetTestCaseMetadata(test);

		var result = new TestNode { Uid = test.TestUniqueID, DisplayName = testMetadata?.TestDisplayName ?? "<unknown test display name>" };

		foreach (var property in properties)
			result.Properties.Add(property);

		var sourceFile = testCaseMetadata?.SourceFilePath;
		var sourceLine = testCaseMetadata?.SourceLineNumber;
		if (sourceFile is not null && sourceLine.HasValue)
		{
			var linePosition = new LinePosition(sourceLine.Value, -1);
			var span = new LinePositionSpan(linePosition, linePosition);
			result.Properties.Add(new TestFileLocationProperty(sourceFile, span));
		}

		// TODO: Why is this ID sniffing rather than using capabilities? https://github.com/microsoft/testfx/issues/2546
		// Also note that the logic here was obtained by inspecting VSTestBridge. We presume that this information may
		// become more formalized once the official protocol support is published, especially the fact that VSTestBridge
		// leverages internal types from Microsoft.Testing.Platform like SerializableNamedKeyValuePairsStringProperty
		// for traits (we re-created it as TraitsProperty below)
		if (testSessionContext.Client.Id == WellKnownClients.VisualStudio)
		{
			result.Properties.Add(new KeyValuePairStringProperty("vstest.TestCase.Id", test.TestCaseUniqueID));

			var testClassWithNamespace = testCaseMetadata?.TestClassNameWithNamespace;
			if (testClassWithNamespace is not null)
				result.Properties.Add(new KeyValuePairStringProperty("vstest.TestCase.FullyQualifiedName", testClassWithNamespace));

			var traits = testMetadata?.Traits;
			if (traits is not null)
				result.Properties.Add(new TraitsProperty(traits));
		}

		return result;
	}

	// TODO: How does serialization work here? Is it automatic or do I need to add a serializer? https://github.com/microsoft/testfx/issues/2561
	public sealed class TraitsProperty : IProperty
	{
		public TraitsProperty(IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
		{
			Name = "traits";
			Pairs = traits.SelectMany(trait => trait.Value.Select(value => new KeyValuePair<string, string>(trait.Key, value))).ToArray();
		}

		public string Name { get; }

		public KeyValuePair<string, string>[] Pairs { get; }
	}

	[Serializable]
	public sealed class XunitException : Exception
	{
		public XunitException(_IErrorMetadata metadata) :
			base(ExceptionUtility.CombineMessages(metadata)) =>
				StackTrace = ExceptionUtility.CombineStackTraces(metadata);

		public override string? StackTrace { get; }
	}
}
