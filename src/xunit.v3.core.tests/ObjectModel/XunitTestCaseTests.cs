using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseTests
{
	readonly XunitTestCase testCase;

	public XunitTestCaseTests()
	{
		var testMethod = TestData.XunitTestMethod<ClassUnderTest>(nameof(ClassUnderTest.Passing));
		testCase = new XunitTestCase(testMethod, "display-name", "unique-id", @explicit: false);
	}

	[Fact]
	public void Metadata()
	{
		Assert.Equal("display-name", testCase.TestCaseDisplayName);
		Assert.Equal("XunitTestCaseTests+ClassUnderTest", testCase.TestClassName);
		Assert.Equal("Passing", testCase.TestMethodName);
	}

	[Fact]
	public void Serialization()
	{
		var serialized = SerializationHelper.Serialize(testCase);
		var deserialized = SerializationHelper.Deserialize(serialized);

		Assert.IsType<XunitTestCase>(deserialized);
		Assert.Equivalent(testCase, deserialized);
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { }
	}
}
