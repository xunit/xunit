using System;
using Xunit;
using Xunit.v3;

public class DefaultTestCaseOrdererTests
{
	static readonly _ITestCase[] TestCases = new[] {
		Mocks.TestCase<ClassUnderTest>("Test1", uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.TestCase<ClassUnderTest>("Test2", uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.TestCase<ClassUnderTest>("Test3", uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.TestCase<ClassUnderTest>("Test4", uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.TestCase<ClassUnderTest>("Test3", uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.TestCase<ClassUnderTest>("Test5", uniqueID: $"test-case-{Guid.NewGuid():n}"),
		Mocks.TestCase<ClassUnderTest>("Test6", uniqueID: $"test-case-{Guid.NewGuid():n}")
	};

	[Fact]
	public static void OrderIsStable()
	{
		var orderer = new DefaultTestCaseOrderer();

		var result1 = orderer.OrderTestCases(TestCases);
		var result2 = orderer.OrderTestCases(TestCases);
		var result3 = orderer.OrderTestCases(TestCases);

		Assert.Equal(result1, result2);
		Assert.Equal(result2, result3);
	}

	[Fact]
	public static void OrderIsUnpredictable()
	{
		var orderer = new DefaultTestCaseOrderer();

		var result = orderer.OrderTestCases(TestCases);

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
	}
}
