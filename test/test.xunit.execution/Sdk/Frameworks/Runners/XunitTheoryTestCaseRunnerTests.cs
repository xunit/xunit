using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitTheoryTestCaseRunnerTests
{
    [Fact]
    public static async void EnumeratesDataAtRuntimeAndExecutesOneTestForEachDataRow()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableXunitTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithData", messageBus, "Display Name");

        var summary = await runner.RunAsync();

        Assert.NotEqual(0m, summary.Time);
        Assert.Equal(2, summary.Total);
        Assert.Equal(1, summary.Failed);
        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Equal($"Display Name(x: 42, y: {21.12:G17}, z: \"Hello\")", passed.Test.DisplayName);
        var failed = messageBus.Messages.OfType<ITestFailed>().Single();
        Assert.Equal("Display Name(x: 0, y: 0, z: \"World!\")", failed.Test.DisplayName);
    }

    [Fact]
    public static async void DiscovererWhichThrowsReturnsASingleFailedTest()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        var messageBus = new SpyMessageBus();
        var runner = TestableXunitTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithThrowingData", messageBus, "Display Name");

        var summary = await runner.RunAsync();

        Assert.Equal(0m, summary.Time);
        Assert.Equal(1, summary.Total);
        Assert.Equal(1, summary.Failed);
        var failed = messageBus.Messages.OfType<ITestFailed>().Single();
        Assert.Equal("Display Name", failed.Test.DisplayName);
        Assert.Equal("System.DivideByZeroException", failed.ExceptionTypes.Single());
        Assert.Equal("Attempted to divide by zero.", failed.Messages.Single());
        Assert.Contains("XunitTheoryTestCaseRunnerTests.ClassUnderTest.get_ThrowingData()", failed.StackTraces.Single());
    }

    [Fact]
    public static async void DisposesArguments()
    {
        ClassUnderTest.DataWasDisposed = false;
        var messageBus = new SpyMessageBus();
        var runner = TestableXunitTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithDisposableData", messageBus);

        await runner.RunAsync();

        Assert.True(ClassUnderTest.DataWasDisposed);
    }

    [Fact]
    public static async void OnlySkipsDataRowsWithSkipReason()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableXunitTheoryTestCaseRunner.Create<ClassUnderTest>("TestWithSomeDataSkipped", messageBus, "Display Name");

        var summary = await runner.RunAsync();

        Assert.NotEqual(0m, summary.Time);
        Assert.Equal(4, summary.Total);
        Assert.Equal(2, summary.Skipped);
        Assert.Equal(1, summary.Failed);
        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Equal($"Display Name(x: 1, y: {2.1:G17}, z: \"not skipped\")", passed.Test.DisplayName);
        var failed = messageBus.Messages.OfType<ITestFailed>().Single();
        Assert.Equal("Display Name(x: 0, y: 0, z: \"also not skipped\")", failed.Test.DisplayName);

        Assert.Contains(messageBus.Messages.OfType<ITestSkipped>(), skipped => skipped.Test.DisplayName == $"Display Name(x: 42, y: {21.12:G17}, z: \"Hello\")");
        Assert.Contains(messageBus.Messages.OfType<ITestSkipped>(), skipped => skipped.Test.DisplayName == "Display Name(x: 0, y: 0, z: \"World!\")");
    }

    [Fact]
    public async void ThrowingToString()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableXunitTheoryTestCaseRunner.Create<ClassWithThrowingToString>("Test", messageBus, "Display Name");

        var summary = await runner.RunAsync();
        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Equal("Display Name(c: TargetInvocationException was thrown formatting an object of type \"XunitTheoryTestCaseRunnerTests+ClassWithThrowingToString\")", passed.Test.DisplayName);
    }

    public class ClassWithThrowingToString
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
        var runner = TestableXunitTheoryTestCaseRunner.Create<ClassWithThrowingEnumerator>("Test", messageBus, "Display Name");

        var summary = await runner.RunAsync();
        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Equal("Display Name(c: [ClassWithThrowingEnumerator { }])", passed.Test.DisplayName);
    }

    public class ClassWithThrowingEnumerator
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

    class TestableXunitTheoryTestCaseRunner : XunitTheoryTestCaseRunner
    {
        TestableXunitTheoryTestCaseRunner(IXunitTestCase testCase,
                                          string displayName,
                                          string skipReason,
                                          object[] constructorArguments,
                                          IMessageSink diagnosticMessageSink,
                                          IMessageBus messageBus,
                                          ExceptionAggregator aggregator,
                                          CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource) { }

        public static TestableXunitTheoryTestCaseRunner Create<TClassUnderTest>(string methodName, IMessageBus messageBus, string displayName = null)
        {
            return new TestableXunitTheoryTestCaseRunner(
                Mocks.XunitTestCase<TClassUnderTest>(methodName),
                displayName,
                null,
                new object[0],
                SpyMessageSink.Create(),
                messageBus,
                new ExceptionAggregator(),
                new CancellationTokenSource()
            );
        }
    }
}
