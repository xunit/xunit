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
			[typeof(NotImplementedException), typeof(NotSupportedException)],
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

		var serialized = SerializationHelper.Instance.Serialize(testCase);
		var deserialized = SerializationHelper.Instance.Deserialize(serialized);

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
		var type = typeof(XunitTestCaseTestsNamespace.ParentClass.GenericTestClass<,>);
		var testClass = TestData.XunitTestClass(type);
		var method1 = type.GetMethod("TestMethod1")!;
		var testMethod1 = TestData.XunitTestMethod(testClass, method1);
		var testCase1 = new XunitTestCase(testMethod1, "display name", "id", @explicit: false);

		Assert.Equal(typeof(XunitTestCaseTestsNamespace.ParentClass.GenericTestClass<,>).FullName, testCase1.TestClassName);
		Assert.Equal("TestMethod1", testCase1.TestMethodName);
		Assert.Collection(
			testCase1.TestMethodParameterTypesVSTest,
			p => Assert.Equal("!0", p),
			p => Assert.Equal("!1", p),
			p => Assert.Equal(typeof(string[,]).FullName, p)
		);
		Assert.Equal("System.Void", testCase1.TestMethodReturnTypeVSTest);

		var method2 = type.GetMethod("TestMethod2")!;
		var testMethod2 = TestData.XunitTestMethod(testClass, method2);
		var testCase2 = new XunitTestCase(testMethod2, "display name", "id", @explicit: false);

		Assert.Equal(typeof(XunitTestCaseTestsNamespace.ParentClass.GenericTestClass<,>).FullName, testCase2.TestClassName);
		Assert.Equal("TestMethod2", testCase2.TestMethodName);
		Assert.Collection(
			testCase2.TestMethodParameterTypesVSTest,
			p => Assert.Equal("!0", p),
			p => Assert.Equal("XunitTestCaseTestsNamespace.ParentClass+MyList`1<!1>", p),
			p => Assert.Equal("!!0", p),
			p => Assert.Equal("!!1", p),
			p => Assert.Equal("XunitTestCaseTestsNamespace.ParentClass+MyList`1<System.String>", p)
		);
		Assert.Equal("System.Threading.Tasks.Task`1<System.Int32>", testCase2.TestMethodReturnTypeVSTest);
	}
}

namespace XunitTestCaseTestsNamespace
{
#pragma warning disable xUnit1003 // Theory methods must have test data
#pragma warning disable xUnit1028 // Test method must have valid return type

	internal class ParentClass
	{
		internal class GenericTestClass<X, Y>
		{
			[Theory]
			public void TestMethod1(X _1, Y _2, string[,] _3) { }

			[Theory]
			public Task<int> TestMethod2<U, V>(X _1, MyList<Y> _2, U _3, V _4, MyList<string> _5) => Task.FromResult(42);
		}

		public class MyList<T> : List<T> { }
	}

#pragma warning restore xUnit1028
#pragma warning restore xUnit1003
}
