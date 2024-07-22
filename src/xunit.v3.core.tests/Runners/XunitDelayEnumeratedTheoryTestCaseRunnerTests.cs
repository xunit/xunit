using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class XunitDelayEnumeratedTheoryTestCaseRunnerTests
{
	[Fact]
	public static async ValueTask EnumeratesDataAtRuntimeAndExecutesOneTestForEachDataRow()
	{
		var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.TestWithData));
		var runner = new TestableXunitDelayEnumeratedTheoryTestCaseRunner(testCase) { DisplayName = "Display Name" };

		var summary = await runner.RunAsync();

		Assert.NotEqual(0m, summary.Time);
		Assert.Equal(2, summary.Total);
		Assert.Equal(1, summary.Failed);
		var messages = runner.MessageBus.Messages;
		var passed = messages.OfType<ITestPassed>().Single();
		var passedStarting = messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == passed.TestUniqueID);
		Assert.Equal($"Display Name(x: 42, _1: {21.12:G17}, _2: \"Hello\")", passedStarting.TestDisplayName);
		var failed = messages.OfType<ITestFailed>().Single();
		var failedStarting = messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID);
		Assert.Equal("Display Name(x: 0, _1: 0, _2: \"World!\")", failedStarting.TestDisplayName);
	}

	[Fact]
	public static async ValueTask DiscovererWhichThrowsReturnsASingleFailedTest()
	{
		var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.TestWithThrowingData));
		var runner = new TestableXunitDelayEnumeratedTheoryTestCaseRunner(testCase) { DisplayName = "Display Name" };

		var summary = await runner.RunAsync();

		Assert.Equal(0m, summary.Time);
		Assert.Equal(1, summary.Total);
		Assert.Equal(1, summary.Failed);
		var messages = runner.MessageBus.Messages;
		var failed = messages.OfType<ITestFailed>().Single();
		var failedStarting = messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID);
		Assert.Equal("Display Name", failedStarting.TestDisplayName);
		Assert.Equal(typeof(DivideByZeroException).SafeName(), failed.ExceptionTypes.Single());
		Assert.Equal("Attempted to divide by zero.", failed.Messages.Single());
		Assert.Contains($"{nameof(ClassUnderTest)}.get_{nameof(ClassUnderTest.ThrowingData)}", failed.StackTraces.Single());
	}

	[Fact]
	public static async ValueTask DisposesArguments()
	{
		ClassUnderTest.DataWasDisposed = false;
		var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.TestWithDisposableData));
		var runner = new TestableXunitDelayEnumeratedTheoryTestCaseRunner(testCase);

		await runner.RunAsync();

		Assert.True(ClassUnderTest.DataWasDisposed);
	}

	[Fact]
	public static async ValueTask SkipsDataAttributesWithSkipReason()
	{
		var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.TestWithSomeDataSkipped));
		var runner = new TestableXunitDelayEnumeratedTheoryTestCaseRunner(testCase) { DisplayName = "Display Name" };

		var summary = await runner.RunAsync();

		var messages = runner.MessageBus.Messages;
		Assert.NotEqual(0m, summary.Time);
		Assert.Equal(6, summary.Total);
		Assert.Equal(3, summary.Skipped);
		Assert.Equal(2, summary.Failed);
		Assert.Collection(
			messages.OfType<ITestPassed>().Select(p => messages.OfType<ITestStarting>().Single(s => s.TestUniqueID == p.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal($"Display Name(x: 1, _1: {2.1:G17}, _2: \"not skipped\")", displayName)
		);
		Assert.Collection(
			messages.OfType<ITestFailed>().Select(p => messages.OfType<ITestStarting>().Single(s => s.TestUniqueID == p.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal("Display Name(x: 0, _1: 0, _2: \"also not skipped\")", displayName),
			displayName => Assert.Equal("Display Name(x: 0, _1: 0, _2: \"SomeData2 not skipped\")", displayName)
		);
		Assert.Collection(
			messages.OfType<ITestSkipped>().Select(p => messages.OfType<ITestStarting>().Single(s => s.TestUniqueID == p.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal("Display Name(x: 0, _1: 0, _2: \"World!\")", displayName),
			displayName => Assert.Equal($"Display Name(x: 18, _1: {36.48:G17}, _2: \"SomeData2 skipped\")", displayName),
			displayName => Assert.Equal($"Display Name(x: 42, _1: {21.12:G17}, _2: \"Hello\")", displayName)
		);
	}

	[Fact]
	public async ValueTask ThrowingToString()
	{
		var testCase = TestData.XunitTestCase<ClassWithThrowingToString>(nameof(ClassWithThrowingToString.Test));
		var runner = new TestableXunitDelayEnumeratedTheoryTestCaseRunner(testCase) { DisplayName = "Display Name" };

		await runner.RunAsync();

		var passed = runner.MessageBus.Messages.OfType<ITestPassed>().Single();
		var passedStarting = runner.MessageBus.Messages.OfType<ITestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
		Assert.Equal("Display Name(c: TargetInvocationException was thrown formatting an object of type \"XunitDelayEnumeratedTheoryTestCaseRunnerTests+ClassWithThrowingToString\")", passedStarting.TestDisplayName);
	}

	class ClassWithThrowingToString
	{
		public static IEnumerable<object[]> TestData()
		{
			yield return new object[] { new ClassWithThrowingToString() };
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public static void Test(ClassWithThrowingToString c)
		{
			Assert.NotNull(c);
		}

		public override string ToString()
		{
			throw new DivideByZeroException();
		}
	}

	[Fact]
	public async ValueTask ThrowingEnumerator()
	{
		var testCase = TestData.XunitTestCase<ClassWithThrowingEnumerator>(nameof(ClassWithThrowingEnumerator.Test));
		var runner = new TestableXunitDelayEnumeratedTheoryTestCaseRunner(testCase) { DisplayName = "Display Name" };

		var summary = await runner.RunAsync();
		var passed = runner.MessageBus.Messages.OfType<ITestPassed>().Single();
		var passedStarting = runner.MessageBus.Messages.OfType<ITestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
		Assert.Equal("Display Name(_: [ClassWithThrowingEnumerator { }])", passedStarting.TestDisplayName);
	}

	class ClassWithThrowingEnumerator
	{
		public static IEnumerable<object[]> TestData()
		{
			yield return new object[] { new ClassWithThrowingEnumerator[] { new ClassWithThrowingEnumerator() } };
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public void Test(ClassWithThrowingEnumerator[] _) { }

		public IEnumerator GetEnumerator()
		{
			throw new InvalidOperationException();
		}
	}

	class ClassUnderTest
	{
		public static bool DataWasDisposed;

		public static IEnumerable<object[]> DisposableData
		{
			get
			{
				var disposable = Substitute.For<IDisposable>();
				disposable.When(x => x.Dispose()).Do(_ => DataWasDisposed = true);

				yield return new object[] { disposable };
			}
		}

		public static IEnumerable<object[]> SomeData
		{
			get
			{
				yield return new object[] { 42, 21.12, "Hello" };
				yield return new object[] { 0, 0.0, "World!" };
			}
		}

		public static IEnumerable<ITheoryDataRow> SomeData2
		{
			get
			{
				yield return new TheoryDataRow<int, double, string>(0, 0.0, "SomeData2 not skipped");
				yield return new TheoryDataRow<int, double, string>(18, 36.48, "SomeData2 skipped") { Skip = "Skip this one row" };
			}
		}

		[Theory]
		[MemberData(nameof(SomeData))]
		public void TestWithData(int x, double _1, string _2)
		{
			Assert.NotEqual(0, x);
		}

		[Theory]
		[MemberData(nameof(DisposableData))]
		public void TestWithDisposableData(IDisposable _)
		{
			Assert.True(false);
		}

		[Theory]
		[InlineData(1, 2.1, "not skipped")]
		[MemberData(nameof(SomeData), Skip = "Skipped")]
		[MemberData(nameof(SomeData2))]
		[InlineData(0, 0.0, "also not skipped")]
		public void TestWithSomeDataSkipped(int x, double _1, string _2)
		{
			Assert.NotEqual(0, x);
		}

		public static IEnumerable<object[]> ThrowingData
		{
			get
			{
				throw new DivideByZeroException();
			}
		}

		[Theory]
		[MemberData(nameof(ThrowingData))]
		public void TestWithThrowingData(int _) { }
	}

	class TestableXunitDelayEnumeratedTheoryTestCaseRunner(IXunitTestCase testCase) :
		XunitDelayEnumeratedTheoryTestCaseRunner
	{
		public string? DisplayName = null;
		public readonly SpyMessageBus MessageBus = new();
		public readonly IXunitTestCase TestCase = Guard.ArgumentNotNull(testCase);

		public ValueTask<RunSummary> RunAsync() =>
			RunAsync(TestCase, MessageBus, new ExceptionAggregator(), new CancellationTokenSource(), DisplayName ?? TestCase.TestCaseDisplayName, TestCase.SkipReason, ExplicitOption.Off, [], []);
	}
}
