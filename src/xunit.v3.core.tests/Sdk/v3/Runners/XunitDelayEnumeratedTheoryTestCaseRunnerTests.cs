using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitDelayEnumeratedTheoryTestCaseRunnerTests
{
	[Fact]
	public static async ValueTask EnumeratesDataAtRuntimeAndExecutesOneTestForEachDataRow()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithData", messageBus, "Display Name");

		var summary = await runner.RunAsync();

		Assert.NotEqual(0m, summary.Time);
		Assert.Equal(2, summary.Total);
		Assert.Equal(1, summary.Failed);
		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		var passedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == passed.TestUniqueID);
		Assert.Equal($"Display Name(x: 42, _1: {21.12:G17}, _2: \"Hello\")", passedStarting.TestDisplayName);
		var failed = messageBus.Messages.OfType<_TestFailed>().Single();
		var failedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID);
		Assert.Equal("Display Name(x: 0, _1: 0, _2: \"World!\")", failedStarting.TestDisplayName);
	}

	[Fact]
	public static async ValueTask DiscovererWhichThrowsReturnsASingleFailedTest()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithThrowingData", messageBus, "Display Name");

		var summary = await runner.RunAsync();

		Assert.Equal(0m, summary.Time);
		Assert.Equal(1, summary.Total);
		Assert.Equal(1, summary.Failed);
		var failed = messageBus.Messages.OfType<_TestFailed>().Single();
		var failedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID);
		Assert.Equal("Display Name", failedStarting.TestDisplayName);
		Assert.Equal("System.DivideByZeroException", failed.ExceptionTypes.Single());
		Assert.Equal("Attempted to divide by zero.", failed.Messages.Single());
		Assert.Contains("ClassUnderTest.get_ThrowingData", failed.StackTraces.Single());
	}

	[Fact]
	public static async ValueTask DisposesArguments()
	{
		ClassUnderTest.DataWasDisposed = false;
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithDisposableData", messageBus);

		await runner.RunAsync();

		Assert.True(ClassUnderTest.DataWasDisposed);
	}

	[Fact]
	public static async ValueTask SkipsDataAttributesWithSkipReason()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithSomeDataSkipped", messageBus, "Display Name");

		var summary = await runner.RunAsync();

		Assert.NotEqual(0m, summary.Time);
		Assert.Equal(6, summary.Total);
		Assert.Equal(3, summary.Skipped);
		Assert.Equal(2, summary.Failed);
		Assert.Collection(
			messageBus.Messages.OfType<_TestPassed>().Select(p => messageBus.Messages.OfType<_TestStarting>().Single(s => s.TestUniqueID == p.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal($"Display Name(x: 1, _1: {2.1:G17}, _2: \"not skipped\")", displayName)
		);
		Assert.Collection(
			messageBus.Messages.OfType<_TestFailed>().Select(p => messageBus.Messages.OfType<_TestStarting>().Single(s => s.TestUniqueID == p.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal("Display Name(x: 0, _1: 0, _2: \"also not skipped\")", displayName),
			displayName => Assert.Equal("Display Name(x: 0, _1: 0, _2: \"SomeData2 not skipped\")", displayName)
		);
		Assert.Collection(
			messageBus.Messages.OfType<_TestSkipped>().Select(p => messageBus.Messages.OfType<_TestStarting>().Single(s => s.TestUniqueID == p.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal("Display Name(x: 0, _1: 0, _2: \"World!\")", displayName),
			displayName => Assert.Equal($"Display Name(x: 18, _1: {36.48:G17}, _2: \"SomeData2 skipped\")", displayName),
			displayName => Assert.Equal($"Display Name(x: 42, _1: {21.12:G17}, _2: \"Hello\")", displayName)
		);
	}

	[Fact]
	public async ValueTask ThrowingToString()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassWithThrowingToString>("Test", messageBus, "Display Name");

		await runner.RunAsync();

		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		var passedStarting = messageBus.Messages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
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
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassWithThrowingEnumerator>("Test", messageBus, "Display Name");

		var summary = await runner.RunAsync();
		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		var passedStarting = messageBus.Messages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
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
				yield return new TheoryDataRow(0, 0.0, "SomeData2 not skipped");
				yield return new TheoryDataRow(18, 36.48, "SomeData2 skipped") { Skip = "Skip this one row" };
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

	class TestableXunitDelayEnumeratedTheoryTestCaseRunner : XunitDelayEnumeratedTheoryTestCaseRunner
	{
		readonly string displayName;
		readonly IMessageBus messageBus;
		readonly IXunitTestCase testCase;

		TestableXunitDelayEnumeratedTheoryTestCaseRunner(
			IXunitTestCase testCase,
			string displayName,
			IMessageBus messageBus)
		{
			this.testCase = testCase;
			this.displayName = displayName;
			this.messageBus = messageBus;
		}

		public static TestableXunitDelayEnumeratedTheoryTestCaseRunner Create<TClassUnderTest>(
			string methodName,
			IMessageBus messageBus,
			string displayName = "MockDisplayName") =>
				new(TestData.XunitTestCase<TClassUnderTest>(methodName), displayName, messageBus);

		public ValueTask<RunSummary> RunAsync() =>
			RunAsync(testCase, messageBus, new ExceptionAggregator(), new CancellationTokenSource(), displayName, skipReason: null, ExplicitOption.Off, Array.Empty<object>(), Array.Empty<object>());
	}
}
