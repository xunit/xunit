using System;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.v3;

[Collection(typeof(ConsoleCaptureTestOutputWriterTestsCollection))]
public sealed class ConsoleCaptureTestOutputWriterTests : IDisposable
{
	readonly ConsoleCaptureTestOutputWriter writer;
	readonly IXunitTest test;
	readonly ITestContextAccessor testContextAccessor = Substitute.For<ITestContextAccessor>();
	readonly TestOutputHelper testOutputHelper = new();

	public ConsoleCaptureTestOutputWriterTests()
	{
		test = Guard.NotNull("TestContext.Current.Test must point to an IXunitTest instance", TestContext.Current.Test as IXunitTest);

		testOutputHelper = new();
		testOutputHelper.Initialize(new SpyMessageBus(), test);

		testContextAccessor = Substitute.For<ITestContextAccessor>();
		testContextAccessor.Current.TestOutputHelper.Returns(testOutputHelper);

		writer = new(testContextAccessor, captureError: true, captureOut: true);
	}

	public void Dispose()
	{
		writer.Dispose();
		testOutputHelper.Uninitialize();
	}

	[Fact]
	public void CapturesConsoleOut()
	{
		Console.WriteLine("This is a line of text from Console.WriteLine");

		Assert.Equal("This is a line of text from Console.WriteLine" + Environment.NewLine, testOutputHelper.Output);
	}

	[Fact]
	public void CapturesConsoleError()
	{
		Console.Error.WriteLine("This is a line of text from Console.Error.WriteLine");

		Assert.Equal("This is a line of text from Console.Error.WriteLine" + Environment.NewLine, testOutputHelper.Output);
	}

	[Fact]
	public void OutputIsInterleaved()
	{
		Console.WriteLine("This is a line of text from Console.WriteLine");
		testOutputHelper.WriteLine("This is a line of text from the output helper");
		Console.Error.WriteLine("This is a line of text from Console.Error.WriteLine");

		Assert.Equal(
			"This is a line of text from Console.WriteLine" + Environment.NewLine +
			"This is a line of text from the output helper" + Environment.NewLine +
			"This is a line of text from Console.Error.WriteLine" + Environment.NewLine,
			testOutputHelper.Output
		);
	}
}

[CollectionDefinition(DisableParallelization = true)]
public class ConsoleCaptureTestOutputWriterTestsCollection { }
