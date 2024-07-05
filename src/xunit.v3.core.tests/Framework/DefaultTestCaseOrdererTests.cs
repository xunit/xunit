using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class DefaultTestCaseOrdererTests
{
	static readonly _ITestCase[] TestCases =
	[
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test1), uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test2), uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test3), uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test4), uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test5), uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test6), uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Test7), uniqueID: $"test-case-{Guid.NewGuid():n}"),
	];

	[Fact]
	public static void OrderIsStable()
	{
		var orderer = DefaultTestCaseOrderer.Instance;

		var result1 = orderer.OrderTestCases(TestCases);
		var result2 = orderer.OrderTestCases(TestCases);
		var result3 = orderer.OrderTestCases(TestCases);

		Assert.Equal(result1, result2);
		Assert.Equal(result2, result3);
	}

	[Fact]
	public static void OrderIsUnpredictable()
	{
		var orderer = DefaultTestCaseOrderer.Instance;

		var result = orderer.OrderTestCases(TestCases);

		// We assume 7 test cases are enough to make the likelihood of ordering directly in order
		// small enough. If this ever starts failing, we can just add an extra method.
		Assert.NotEqual(TestCases, result);
	}

	class ClassUnderTest
	{
		[Fact]
		public void Test1() { }

		[Fact]
		public void Test2() { }

		[Fact]
		public void Test3() { }

		[Fact]
		public void Test4() { }

		[Fact]
		public void Test5() { }

		[Fact]
		public void Test6() { }

		[Fact]
		public void Test7() { }
	}
}
