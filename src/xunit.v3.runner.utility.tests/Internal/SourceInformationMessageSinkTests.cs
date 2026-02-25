using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class SourceInformationMessageSinkTests
{
	[Fact]
	public void TestCaseDiscovered_WhenNull_Overwrites()
	{
		var provider = new SpySourceInformationProvider { OnGetSourceInformation = (c, m) => new SourceInformation("file-path", 42) };
		var message = TestData.TestCaseDiscovered();
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsType<ITestCaseDiscovered>(Assert.Single(spySink.Messages), exactMatch: false);
		Assert.Equal("file-path", result.SourceFilePath);
		Assert.Equal(42, result.SourceLineNumber);
	}

	[Fact]
	public void TestCaseStarting_WhenNull_Overwrites()
	{
		var provider = new SpySourceInformationProvider { OnGetSourceInformation = (c, m) => new SourceInformation("file-path", 42) };
		var message = TestData.TestCaseStarting();
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsType<ITestCaseStarting>(Assert.Single(spySink.Messages), exactMatch: false);
		Assert.Equal("file-path", result.SourceFilePath);
		Assert.Equal(42, result.SourceLineNumber);
	}

	[Fact]
	public void TestCaseDiscovered_WhenNonNull_DoesNotOverwrite()
	{
		var provider = new SpySourceInformationProvider { OnGetSourceInformation = (c, m) => new SourceInformation("file-path", 42) };
		var message = TestData.TestCaseDiscovered(sourceFilePath: "other-path", sourceLineNumber: 2112);
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsType<ITestCaseDiscovered>(Assert.Single(spySink.Messages), exactMatch: false);
		Assert.Equal("other-path", result.SourceFilePath);
		Assert.Equal(2112, result.SourceLineNumber);
	}

	[Fact]
	public void TestCaseStarting_WhenNonNull_DoesNotOverwrite()
	{
		var provider = new SpySourceInformationProvider { OnGetSourceInformation = (c, m) => new SourceInformation("file-path", 42) };
		var message = TestData.TestCaseStarting(sourceFilePath: "other-path", sourceLineNumber: 2112);
		var spySink = SpyMessageSink.Capture();
		var sink = new SourceInformationMessageSink(spySink, provider);

		sink.OnMessage(message);

		var result = Assert.IsType<ITestCaseStarting>(Assert.Single(spySink.Messages), exactMatch: false);
		Assert.Equal("other-path", result.SourceFilePath);
		Assert.Equal(2112, result.SourceLineNumber);
	}
}
