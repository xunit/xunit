using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[Collection(typeof(TraceCaptureTestOutputWriterTestsCollection))]
public sealed class TraceCaptureTestOutputWriterTests : IDisposable
{
	readonly TestOutputHelper testOutputHelper = new();
	readonly TraceCaptureTestOutputWriter writer;

	public TraceCaptureTestOutputWriterTests()
	{
		var test = Guard.NotNull("TestContext.Current.Test must point to an ITest instance", TestContext.Current.Test);

		testOutputHelper = new();
		testOutputHelper.Initialize(new SpyMessageBus(), test);

		var testContext = new MockTestContext(testOutputHelper);
		var testContextAccessor = new MockTestContextAccessor(testContext);

		writer = new(testContextAccessor);
	}

	public void Dispose()
	{
		writer.Dispose();
		testOutputHelper.Uninitialize();
	}

	[Fact]
	public void CapturesTrace()
	{
		Trace.WriteLine("This is a line of text from Trace.WriteLine");

		Assert.Equal("This is a line of text from Trace.WriteLine" + Environment.NewLine, testOutputHelper.Output);
	}

	[Fact]
	public void CapturesDebug()
	{
		Debug.WriteLine("This is a line of text from Debug.WriteLine");

#if DEBUG
		Assert.Equal("This is a line of text from Debug.WriteLine" + Environment.NewLine, testOutputHelper.Output);
#else
		Assert.Empty(testOutputHelper.Output);
#endif
	}

	[Fact]
	public void OutputIsInterleaved()
	{
		Trace.WriteLine("This is a line of text from Trace.WriteLine");
		testOutputHelper.WriteLine("This is a line of text from the output helper");
		Debug.WriteLine("This is a line of text from Debug.WriteLine");

		Assert.Equal(
			"This is a line of text from Trace.WriteLine" + Environment.NewLine +
			"This is a line of text from the output helper" + Environment.NewLine +
#if DEBUG
			"This is a line of text from Debug.WriteLine" + Environment.NewLine,
#else
			"",
#endif
			testOutputHelper.Output
		);
	}

	class MockTestContext(ITestOutputHelper testOutputHelper) :
		ITestContext
	{
		public IReadOnlyDictionary<string, TestAttachment>? Attachments => throw new NotImplementedException();
		public CancellationToken CancellationToken => throw new NotImplementedException();
		public ConcurrentDictionary<string, object?> KeyValueStorage => throw new NotImplementedException();
		public TestPipelineStage PipelineStage => throw new NotImplementedException();
		public ITest? Test => throw new NotImplementedException();
		public ITestAssembly? TestAssembly => throw new NotImplementedException();
		public TestEngineStatus? TestAssemblyStatus => throw new NotImplementedException();
		public ITestCase? TestCase => throw new NotImplementedException();
		public TestEngineStatus? TestCaseStatus => throw new NotImplementedException();
		public ITestClass? TestClass => throw new NotImplementedException();
		public object? TestClassInstance => throw new NotImplementedException();
		public TestEngineStatus? TestClassStatus => throw new NotImplementedException();
		public ITestCollection? TestCollection => throw new NotImplementedException();
		public TestEngineStatus? TestCollectionStatus => throw new NotImplementedException();
		public ITestMethod? TestMethod => throw new NotImplementedException();
		public TestEngineStatus? TestMethodStatus => throw new NotImplementedException();
		public ITestOutputHelper? TestOutputHelper => testOutputHelper;
		public TestResultState? TestState => throw new NotImplementedException();
		public TestEngineStatus? TestStatus => throw new NotImplementedException();
		public IReadOnlyList<string>? Warnings => throw new NotImplementedException();

		public void AddAttachment(string name, string value) => throw new NotImplementedException();
		public void AddAttachment(string name, string value, bool replaceExistingValue) => throw new NotImplementedException();
		public void AddAttachment(string name, byte[] value, string mediaType = "application/octet-stream") => throw new NotImplementedException();
		public void AddAttachment(string name, byte[] value, bool replaceExistingValue, string mediaType = "application/octet-stream") => throw new NotImplementedException();
		public void AddWarning(string message) => throw new NotImplementedException();
		public void CancelCurrentTest() => throw new NotImplementedException();
		public ValueTask<object?> GetFixture(Type fixtureType) => throw new NotImplementedException();
		public ValueTask<T?> GetFixture<T>() => throw new NotImplementedException();
		public void SendDiagnosticMessage(string message) => throw new NotImplementedException();
		public void SendDiagnosticMessage(string format, object? arg0) => throw new NotImplementedException();
		public void SendDiagnosticMessage(string format, object? arg0, object? arg1) => throw new NotImplementedException();
		public void SendDiagnosticMessage(string format, object? arg0, object? arg1, object? arg2) => throw new NotImplementedException();
		public void SendDiagnosticMessage(string format, params object?[] args) => throw new NotImplementedException();
	}

	class MockTestContextAccessor(ITestContext context) :
		ITestContextAccessor
	{
		public ITestContext Current => context;
	}
}

[CollectionDefinition(DisableParallelization = true)]
public class TraceCaptureTestOutputWriterTestsCollection { }
