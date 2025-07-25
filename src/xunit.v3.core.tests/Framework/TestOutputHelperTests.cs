using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestOutputHelperTests
{
	public static IEnumerable<TheoryDataRow<string, string>> InvalidStrings_TestData =>
	[
		// Escaping not required
		( " \t  ", " \t  " ),
		( "Hello World", "Hello World" ),
		( "\uD800\uDC00", "\uD800\uDC00" ),
		( "ANSI \u001b[3mitalic\u001b[0m \u001b[36mcyan\u001b[0m", "ANSI \u001b[3mitalic\u001b[0m \u001b[36mcyan\u001b[0m" ),
		( "ANSI \x1b[30;47mblack on white\x1b[0m default colors", "ANSI \x1b[30;47mblack on white\x1b[0m default colors" ),
		( "ANSI \x1b[;47mdefault on white\x1b[m", "ANSI \x1b[;47mdefault on white\x1b[m" ),
		( "ANSI \x1b[38;5;4mblue\x1b[0m", "ANSI \x1b[38;5;4mblue\x1b[0m" ),
		( "ANSI \x1b[;;mmissing arguments\x1b[m", "ANSI \x1b[;;mmissing arguments\x1b[m" ),

		// Escaping required
		( "\0", "\\0" ),
		( "\u0001", "\\x01" ),
		( "\u001F", "\\x1f" ),
		( "\uD800", "\\xd800" ),
		( "\uDC00", "\\xdc00" ),
		( "\uDC00\uD800", "\\xdc00\\xd800" ),
		( "\u001bfoo", "\\x1bfoo" ),
		( "\u001b3m invalid ANSI sequence #1", "\\x1b3m invalid ANSI sequence #1" ),
		( "incomplete ANSI sequence #2 \u001b[3", "incomplete ANSI sequence #2 \\x1b[3" ),
		( "incomplete ANSI sequence #3 \u001b[", "incomplete ANSI sequence #3 \\x1b[" ),
		( "incomplete ANSI sequence #4 \u001b", "incomplete ANSI sequence #4 \\x1b" ),
		( "incomplete ANSI sequence #5 \x1b[128", "incomplete ANSI sequence #5 \\x1b[128" ),
		( "\x1b[this is no ansi\x1b]", "\\x1b[this is no ansi\\x1b]" ),
		( "\x1b^ANSI privacy message\x1b\\ 1234", "\\x1b^ANSI privacy message\\x1b\\ 1234" ),
		( "\x1b[2J clear display", "\\x1b[2J clear display" ),
	];

	[Theory]
	[MemberData(nameof(InvalidStrings_TestData))]
	public void WriteLine(
		string outputText,
		string expected)
	{
		var output = new TestOutputHelper();
		var messageBus = new SpyMessageBus();
		var test = Substitute.For<ITest>();
		test.UniqueID.Returns("test-id");
		test.TestCase.UniqueID.Returns("case-id");
		test.TestCase.TestMethod!.UniqueID.Returns("method-id");
		test.TestCase.TestClass!.UniqueID.Returns("class-id");
		test.TestCase.TestCollection.UniqueID.Returns("coll-id");
		test.TestCase.TestCollection.TestAssembly.UniqueID.Returns("asm-id");
		output.Initialize(messageBus, test);

		output.WriteLine(outputText);

		var message = Assert.Single(messageBus.Messages);
		var outputMessage = Assert.IsType<ITestOutput>(message, exactMatch: false);
		Assert.Equal("asm-id", outputMessage.AssemblyUniqueID);
		Assert.Equal(expected + Environment.NewLine, outputMessage.Output);
		Assert.Equal("case-id", outputMessage.TestCaseUniqueID);
		Assert.Equal("class-id", outputMessage.TestClassUniqueID);
		Assert.Equal("coll-id", outputMessage.TestCollectionUniqueID);
		Assert.Equal("method-id", outputMessage.TestMethodUniqueID);
		Assert.Equal("test-id", outputMessage.TestUniqueID);
		Assert.Equal(expected + Environment.NewLine, output.Output);
	}

	[Fact]
	public void LinesAreBufferedBasedOnEnvironmentNewLine()
	{
		var output = new TestOutputHelper();
		var messageBus = new SpyMessageBus();
		var test = Mocks.Test();
		output.Initialize(messageBus, test);

		output.Write("1");
		output.Write("2");

		Assert.Empty(messageBus.Messages);

		output.Write($"3{Environment.NewLine}4{Environment.NewLine}5");

		Assert.Collection(
			messageBus.Messages.OfType<ITestOutput>(),
			message => Assert.Equal($"123{Environment.NewLine}", message.Output),
			message => Assert.Equal($"4{Environment.NewLine}", message.Output)
		);
		messageBus.Messages.Clear();

		output.Uninitialize();

		var message = Assert.Single(messageBus.Messages.OfType<ITestOutput>());
		Assert.Equal($"5{Environment.NewLine}", message.Output);
	}
}
