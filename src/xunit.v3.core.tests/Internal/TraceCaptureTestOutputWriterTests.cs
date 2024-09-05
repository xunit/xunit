using System;
using System.Diagnostics;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.v3;

[Collection(typeof(TraceCaptureTestOutputWriterTestsCollection))]
public sealed class TraceCaptureTestOutputWriterTests : IDisposable
{
	readonly TraceCaptureTestOutputWriter writer;
	readonly IXunitTest test;
	readonly ITestContextAccessor testContextAccessor = Substitute.For<ITestContextAccessor>();
	readonly TestOutputHelper testOutputHelper = new();

	public TraceCaptureTestOutputWriterTests()
	{
		test = Guard.NotNull("TestContext.Current.Test must point to an IXunitTest instance", TestContext.Current.Test as IXunitTest);

		testOutputHelper = new();
		testOutputHelper.Initialize(new SpyMessageBus(), test);

		testContextAccessor = Substitute.For<ITestContextAccessor>();
		testContextAccessor.Current.TestOutputHelper.Returns(testOutputHelper);

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
}

[CollectionDefinition(DisableParallelization = true)]
public class TraceCaptureTestOutputWriterTestsCollection { }
