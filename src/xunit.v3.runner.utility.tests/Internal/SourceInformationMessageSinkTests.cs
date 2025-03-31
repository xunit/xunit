using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class SourceInformationMessageSinkTests
{
	[Fact]
	public void TestCaseDiscovered_WhenNull_Overwrites()
	{
		var provider = Substitute.For<ISourceInformationProvider>();
		provider.GetSourceInformation(null, null).ReturnsForAnyArgs(new SourceInformation("file-path", 42));
		var message = TestData.TestCaseDiscovered<SourceInformationMessageSinkTests>(nameof(TestCaseDiscovered_WhenNull_Overwrites));
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsAssignableFrom<ITestCaseDiscovered>(Assert.Single(spySink.Messages));
		Assert.Equal("file-path", result.SourceFilePath);
		Assert.Equal(42, result.SourceLineNumber);
	}

	[Fact]
	public void TestCaseStarting_WhenNull_Overwrites()
	{
		var provider = Substitute.For<ISourceInformationProvider>();
		provider.GetSourceInformation(null, null).ReturnsForAnyArgs(new SourceInformation("file-path", 42));
		var message = TestData.TestCaseStarting<SourceInformationMessageSinkTests>(nameof(TestCaseStarting_WhenNull_Overwrites));
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsAssignableFrom<ITestCaseStarting>(Assert.Single(spySink.Messages));
		Assert.Equal("file-path", result.SourceFilePath);
		Assert.Equal(42, result.SourceLineNumber);
	}

	[Fact]
	public void TestCaseDiscovered_WhenNonNull_DoesNotOverwrite()
	{
		var provider = Substitute.For<ISourceInformationProvider>();
		provider.GetSourceInformation(null, null).ReturnsForAnyArgs(new SourceInformation("file-path", 42));
		var message = TestData.TestCaseDiscovered<SourceInformationMessageSinkTests>(nameof(TestCaseDiscovered_WhenNonNull_DoesNotOverwrite), sourceFilePath: "other-path", sourceLineNumber: 2112);
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsAssignableFrom<ITestCaseDiscovered>(Assert.Single(spySink.Messages));
		Assert.Equal("other-path", result.SourceFilePath);
		Assert.Equal(2112, result.SourceLineNumber);
	}

	[Fact]
	public void TestCaseStarting_WhenNonNull_DoesNotOverwrite()
	{
		var provider = Substitute.For<ISourceInformationProvider>();
		provider.GetSourceInformation(null, null).ReturnsForAnyArgs(new SourceInformation("file-path", 42));
		var message = TestData.TestCaseStarting<SourceInformationMessageSinkTests>(nameof(TestCaseStarting_WhenNonNull_DoesNotOverwrite), sourceFilePath: "other-path", sourceLineNumber: 2112);
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsAssignableFrom<ITestCaseStarting>(Assert.Single(spySink.Messages));
		Assert.Equal("other-path", result.SourceFilePath);
		Assert.Equal(2112, result.SourceLineNumber);
	}
}
