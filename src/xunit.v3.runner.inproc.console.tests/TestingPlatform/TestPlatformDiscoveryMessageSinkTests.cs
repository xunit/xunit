using System.Linq;
using System.Threading;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.TestHost;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;

public class TestPlatformDiscoveryMessageSinkTests
{
	public static readonly string AssemblyFullName = Guard.NotNull("Invalid assembly FullName", typeof(TestPlatformDiscoveryMessageSinkTests).Assembly.FullName);

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void DelegatesMessages(bool returnValue)
	{
		var message = TestData.DiagnosticMessage();
		var classUnderTest = TestableTestPlatformDiscoveryMessageSink.Create();
		classUnderTest.InnerSink.Callback = _ => returnValue;

		var result = classUnderTest.OnMessage(message);

		Assert.Equal(returnValue, result);
		var received = Assert.Single(classUnderTest.InnerSink.Messages);
		Assert.Same(message, received);
	}

	[Fact]
	public void ReturnsFalseWhenCancellationTokenIsCancelled()
	{
		var message = TestData.DiagnosticMessage();
		var classUnderTest = TestableTestPlatformDiscoveryMessageSink.Create();
		classUnderTest.InnerSink.Callback = _ => true;
		classUnderTest.CancellationTokenSource.Cancel();

		var result = classUnderTest.OnMessage(message);

		Assert.False(result);
	}

	public class MessageMapping
	{
		[Fact]
		public void ITestCaseDiscovered()
		{
			var discovered = TestData.TestCaseDiscovered(
				sourceFilePath: "/path/to/file.cs",
				sourceLineNumber: 42,
				testClassName: "test-class",
				testMethodName: "test-method",
				testMethodParameterTypesVSTest: ["System.Int32", "System.String"],
				testMethodReturnTypeVSTest: "System.Void",
				testClassNamespace: "ns",
				traits: TestData.DefaultTraits
			);
			var classUnderTest = TestableTestPlatformDiscoveryMessageSink.Create();

			classUnderTest.OnMessage(discovered);

			var message = Assert.Single(classUnderTest.TestNodeMessageBus.PublishedData);
			var updateMessage = Assert.IsType<TestNodeUpdateMessage>(message);
			var testNode = updateMessage.TestNode;
			Assert.Equal("test-case-display-name", testNode.DisplayName);
			Assert.Equal("test-case-id", testNode.Uid.Value);

			var testMethodProperty = testNode.Properties.Single<TestMethodIdentifierProperty>();
			Assert.Equal(AssemblyFullName, testMethodProperty.AssemblyFullName);
			Assert.Equal("test-method", testMethodProperty.MethodName);
			Assert.Equal("ns", testMethodProperty.Namespace);
			Assert.Equivalent(new[] { "System.Int32", "System.String" }, testMethodProperty.ParameterTypeFullNames);
			Assert.Equal("System.Void", testMethodProperty.ReturnTypeFullName);
			Assert.Equal("test-class", testMethodProperty.TypeName);

			var testLocationProperty = testNode.Properties.Single<TestFileLocationProperty>();
			Assert.Equal("/path/to/file.cs", testLocationProperty.FilePath);
			Assert.Equal(42, testLocationProperty.LineSpan.Start.Line);
			Assert.Equal(-1, testLocationProperty.LineSpan.Start.Column);
			Assert.Equal(42, testLocationProperty.LineSpan.End.Line);
			Assert.Equal(-1, testLocationProperty.LineSpan.End.Column);

			var testMetadataProperties = testNode.Properties.OfType<TestMetadataProperty>();
			Assert.Collection(
				testMetadataProperties.OrderBy(p => p.Key).ThenBy(p => p.Value).Select(p => $"'{p.Key}' = '{p.Value}'"),
				keyValue => Assert.Equal("'biff' = 'bang'", keyValue),
				keyValue => Assert.Equal("'foo' = 'bar'", keyValue),
				keyValue => Assert.Equal("'foo' = 'baz'", keyValue)
			);
		}
	}

	class TestableTestPlatformDiscoveryMessageSink(
		SpyMessageSink innerSink,
		SessionUid sessionUid,
		SpyTestPlatformMessageBus testNodeMessageBus,
		CancellationTokenSource cancellationTokenSource) :
			TestPlatformDiscoveryMessageSink(innerSink, AssemblyFullName, sessionUid, testNodeMessageBus, cancellationTokenSource.Token)
	{
		public CancellationTokenSource CancellationTokenSource { get; } = cancellationTokenSource;
		public SpyMessageSink InnerSink { get; } = innerSink;
		public SpyTestPlatformMessageBus TestNodeMessageBus { get; } = testNodeMessageBus;

		public static TestableTestPlatformDiscoveryMessageSink Create() =>
			new(SpyMessageSink.Capture(), new(), new(), new());
	}
}
