using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestOutputHelperTests
{
	public static IEnumerable<object[]> InvalidStrings_TestData()
	{
		// Valid
		yield return new object[] { " \r \n \t  ", " \r \n \t  " };
		yield return new object[] { "Hello World", "Hello World" };
		yield return new object[] { "\uD800\uDC00", "\uD800\uDC00" };

		// Invalid
		yield return new object[] { "\0", "\\0" };
		yield return new object[] { (char)1, "\\x01" };
		yield return new object[] { (char)31, "\\x1f" };
		yield return new object[] { "\uD800", "\\xd800" };
		yield return new object[] { "\uDC00", "\\xdc00" };
		yield return new object[] { "\uDC00\uD800", "\\xdc00\\xd800" };
	}

	[Theory]
	[MemberData(nameof(InvalidStrings_TestData))]
	public void WriteLine(string outputText, string expected)
	{
		var output = new TestOutputHelper();
		var messageBus = new SpyMessageBus();
		var test = Substitute.For<_ITest>();
		test.UniqueID.Returns("test-id");
		test.TestCase.UniqueID.Returns("case-id");
		test.TestCase.TestMethod!.UniqueID.Returns("method-id");
		test.TestCase.TestMethod!.TestClass.UniqueID.Returns("class-id");
		test.TestCase.TestCollection.UniqueID.Returns("coll-id");
		test.TestCase.TestCollection.TestAssembly.UniqueID.Returns("asm-id");
		output.Initialize(messageBus, test);

		output.WriteLine(outputText);

		var message = Assert.Single(messageBus.Messages);
		var outputMessage = Assert.IsType<_TestOutput>(message);
		Assert.Equal("asm-id", outputMessage.AssemblyUniqueID);
		Assert.Equal(expected + Environment.NewLine, outputMessage.Output);
		Assert.Equal("case-id", outputMessage.TestCaseUniqueID);
		Assert.Equal("class-id", outputMessage.TestClassUniqueID);
		Assert.Equal("coll-id", outputMessage.TestCollectionUniqueID);
		Assert.Equal("method-id", outputMessage.TestMethodUniqueID);
		Assert.Equal("test-id", outputMessage.TestUniqueID);
		Assert.Equal(expected + Environment.NewLine, output.Output);
	}
}
