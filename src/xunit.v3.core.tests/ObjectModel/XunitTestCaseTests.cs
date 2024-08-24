using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	[Fact]
	public void ManagedTestCasePropertiesTest()
	{
		var type = typeof(GenericTestClass<,>);
		var testClass = TestData.XunitTestClass(type);
		var method1 = type.GetMethod("TestMethod1")!;
		var testMethod1 = TestData.XunitTestMethod(testClass, method1);
		var testCase1 = new XunitTestCase(testMethod1, "display name", "id", @explicit: false);

		Assert.Equal(typeof(GenericTestClass<,>).FullName, testCase1.TestClassName);
		Assert.Equal("TestMethod1", testCase1.TestMethodName);
		Assert.Collection(
			testCase1.TestMethodParameterTypes,
			p => Assert.Equal("X", p),
			p => Assert.Equal("Y", p),
			p => Assert.Equal(typeof(string[,]).FullName, p)
		);
		Assert.Equal("System.Void", testCase1.TestMethodReturnType);

		var method2 = type.GetMethod("TestMethod2")!;
		var testMethod2 = TestData.XunitTestMethod(testClass, method2);
		var testCase2 = new XunitTestCase(testMethod2, "display name", "id", @explicit: false);

		Assert.Equal(typeof(GenericTestClass<,>).FullName, testCase2.TestClassName);
		Assert.Equal("TestMethod2", testCase2.TestMethodName);
		Assert.Collection(
			testCase2.TestMethodParameterTypes,
			p => Assert.Equal("X", p),
			p => Assert.Equal("List`1", p),  // List<Y> ends up with an odd name here
			p => Assert.Equal("U", p),
			p => Assert.Equal("V", p),
			p => Assert.Equal(typeof(List<string>).FullName, p)
		);
		Assert.Equal(typeof(Task<int>).FullName, testCase2.TestMethodReturnType);
	}

#pragma warning disable xUnit1003 // Theory methods must have test data
#pragma warning disable xUnit1028 // Test method must have valid return type

	class GenericTestClass<X, Y>
	{
		[Theory]
		public void TestMethod1(X _1, Y _2, string[,] _3) { }

		[Theory]
		public Task<int> TestMethod2<U, V>(X _1, List<Y> _2, U _3, V _4, List<string> _5) => Task.FromResult(42);
	}

#pragma warning restore xUnit1028
#pragma warning restore xUnit1003
}
