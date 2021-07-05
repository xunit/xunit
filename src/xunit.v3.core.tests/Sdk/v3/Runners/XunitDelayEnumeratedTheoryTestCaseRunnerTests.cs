using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitDelayEnumeratedTheoryTestCaseRunnerTests
{
	[Fact]
	public static async void EnumeratesDataAtRuntimeAndExecutesOneTestForEachDataRow()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithData", messageBus, "Display Name");

		var summary = await runner.RunAsync();

		Assert.NotEqual(0m, summary.Time);
		Assert.Equal(2, summary.Total);
		Assert.Equal(1, summary.Failed);
		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		var passedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == passed.TestUniqueID);
		Assert.Equal($"Display Name(x: 42, y: {21.12}, z: \"Hello\")", passedStarting.TestDisplayName);
		var failed = messageBus.Messages.OfType<_TestFailed>().Single();
		var failedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID);
		Assert.Equal("Display Name(x: 0, y: 0, z: \"World!\")", failedStarting.TestDisplayName);
	}

	[Fact]
	public static async void DiscovererWhichThrowsReturnsASingleFailedTest()
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
	public static async void DisposesArguments()
	{
		ClassUnderTest.DataWasDisposed = false;
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithDisposableData", messageBus);

		await runner.RunAsync();

		Assert.True(ClassUnderTest.DataWasDisposed);
	}

	[Fact]
	public static async void OnlySkipsDataRowsWithSkipReason()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithSomeDataSkipped", messageBus, "Display Name");

		var summary = await runner.RunAsync();

		Assert.NotEqual(0m, summary.Time);
		Assert.Equal(4, summary.Total);
		Assert.Equal(2, summary.Skipped);
		Assert.Equal(1, summary.Failed);
		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		var passedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == passed.TestUniqueID);
		Assert.Equal($"Display Name(x: 1, y: {2.1}, z: \"not skipped\")", passedStarting.TestDisplayName);
		var failed = messageBus.Messages.OfType<_TestFailed>().Single();
		var failedStarting = messageBus.Messages.OfType<_TestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID);
		Assert.Equal("Display Name(x: 0, y: 0, z: \"also not skipped\")", failedStarting.TestDisplayName);

		Assert.Contains(messageBus.Messages.OfType<_TestSkipped>(), skipped => messageBus.Messages.OfType<_TestStarting>().Single(s => s.TestUniqueID == skipped.TestUniqueID).TestDisplayName == $"Display Name(x: 42, y: {21.12}, z: \"Hello\")");
		Assert.Contains(messageBus.Messages.OfType<_TestSkipped>(), skipped => messageBus.Messages.OfType<_TestStarting>().Single(s => s.TestUniqueID == skipped.TestUniqueID).TestDisplayName == "Display Name(x: 0, y: 0, z: \"World!\")");
	}

	[Fact]
	public async void ThrowingToString()
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
	public async void ThrowingEnumerator()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableXunitDelayEnumeratedTheoryTestCaseRunner.Create<ClassWithThrowingEnumerator>("Test", messageBus, "Display Name");

		var summary = await runner.RunAsync();
		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		var passedStarting = messageBus.Messages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
		Assert.Equal("Display Name(c: [ClassWithThrowingEnumerator { }])", passedStarting.TestDisplayName);
	}

	class ClassWithThrowingEnumerator
	{
		public static IEnumerable<object[]> TestData()
		{
			yield return new object[] { new ClassWithThrowingEnumerator[] { new ClassWithThrowingEnumerator() } };
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public void Test(ClassWithThrowingEnumerator[] c) { }

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

		[Theory]
		[MemberData("SomeData")]
		public void TestWithData(int x, double y, string z)
		{
			Assert.NotEqual(x, 0);
		}

		[Theory]
		[MemberData("DisposableData")]
		public void TestWithDisposableData(IDisposable x)
		{
			Assert.True(false);
		}

		[Theory]
		[InlineData(1, 2.1, "not skipped")]
		[MemberData("SomeData", Skip = "Skipped")]
		[InlineData(0, 0.0, "also not skipped")]
		public void TestWithSomeDataSkipped(int x, double y, string z)
		{
			Assert.NotEqual(x, 0);
		}

		public static IEnumerable<object[]> ThrowingData
		{
			get
			{
				throw new DivideByZeroException();
			}
		}

		[Theory]
		[MemberData("ThrowingData")]
		public void TestWithThrowingData(int x) { }
	}

	class TestableXunitDelayEnumeratedTheoryTestCaseRunner : XunitDelayEnumeratedTheoryTestCaseRunner
	{
		TestableXunitDelayEnumeratedTheoryTestCaseRunner(
			IXunitTestCase testCase,
			string displayName,
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus) :
				base(testCase, displayName, null, new object[0], diagnosticMessageSink, messageBus, new ExceptionAggregator(), new CancellationTokenSource())
		{ }

		public static TestableXunitDelayEnumeratedTheoryTestCaseRunner Create<TClassUnderTest>(
			string methodName,
			IMessageBus messageBus,
			string displayName = "MockDisplayName") =>
				new TestableXunitDelayEnumeratedTheoryTestCaseRunner(
					TestData.XunitTestCase<TClassUnderTest>(methodName),
					displayName,
					SpyMessageSink.Create(),
					messageBus
				);
	}
}
