using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseTests
{
	readonly XunitTestMethod testMethod;

	public XunitTestCaseTests() =>
		testMethod = TestData.XunitTestMethod<ClassUnderTest>(nameof(ClassUnderTest.Passing));

	[Fact]
	public void GuardClauses()
	{
		Assert.Throws<ArgumentNullException>("testMethod", () => new XunitTestCase(null!, "", "", false));
		Assert.Throws<ArgumentNullException>("testCaseDisplayName", () => new XunitTestCase(testMethod, null!, "", false));
		Assert.Throws<ArgumentNullException>("uniqueID", () => new XunitTestCase(testMethod, "", null!, false));
	}

	[Fact]
	public void Metadata()
	{
		var testCase = new XunitTestCase(testMethod, "display-name", "unique-id", @explicit: false);

		Assert.Equal("display-name", testCase.TestCaseDisplayName);
		Assert.Equal("XunitTestCaseTests+ClassUnderTest", testCase.TestClassName);
		Assert.Equal("Passing", testCase.TestMethodName);
	}

	[Theory]
	[InlineData(null, null)]
	[InlineData("skipUnless", null)]
	[InlineData(null, "skipWhen")]
	public void Serialization(
		string? skipUnless,
		string? skipWhen)
	{
		var testCase = new XunitTestCase(
			testMethod,
			"display-name",
			"unique-id",
			@explicit: false,
			"skipReason",
			typeof(XunitTestCaseTests),
			skipUnless,
			skipWhen,
			new() { ["Foo"] = ["Bar", "Baz"] },
			[42],
			"filePath.cs",
			2112,
			2600
		);

		var serialized = SerializationHelper.Serialize(testCase);
		var deserialized = SerializationHelper.Deserialize(serialized);

		Assert.IsType<XunitTestCase>(deserialized);
		Assert.Equivalent(testCase, deserialized);
	}

	[Fact]
	public void StaticallySkipped()
	{
		var testCase = new XunitTestCase(testMethod, "display-name", "unique-id", @explicit: false, skipReason: "Skipped");

		Assert.Equal("Skipped", testCase.SkipReason);
		Assert.Equal("Skipped", ((ITestCaseMetadata)testCase).SkipReason);
	}

	[Fact]
	public void DynamicallySkipped_SkipUnless()
	{
		var testCase = new XunitTestCase(testMethod, "display-name", "unique-id", @explicit: false, skipReason: "Skipped", skipUnless: "PropertyName");

		Assert.Equal("Skipped", testCase.SkipReason);
		Assert.Null(((ITestCaseMetadata)testCase).SkipReason);
	}

	[Fact]
	public void DynamicallySkipped_SkipWhen()
	{
		var testCase = new XunitTestCase(testMethod, "display-name", "unique-id", @explicit: false, skipReason: "Skipped", skipWhen: "PropertyName");

		Assert.Equal("Skipped", testCase.SkipReason);
		Assert.Null(((ITestCaseMetadata)testCase).SkipReason);
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { }
	}
}
