#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class Xunit2AcceptanceTests
{
    public class EndToEndMessageInspection : AcceptanceTestV2
    {
        [Fact]
        public void NoTests()
        {
            List<IMessageSinkMessage> results = Run(typeof(NoTestsClass));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message =>
                {
                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
                    Assert.Equal(0, finished.TestsRun);
                    Assert.Equal(0, finished.TestsFailed);
                    Assert.Equal(0, finished.TestsSkipped);
                }
            );
        }

        [Fact]
        public void SinglePassingTest()
        {
            List<IMessageSinkMessage> results = Run(typeof(SinglePassingTestClass));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message =>
                {
                    var collectionStarting = Assert.IsAssignableFrom<ITestCollectionStarting>(message);
                    Assert.NotNull(collectionStarting.TestCollection);
                    // TODO: There will need to be more tests here eventually...
                },
                message =>
                {
                    var classStarting = Assert.IsAssignableFrom<ITestClassStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", classStarting.TestClass.Class.Name);
                },
                message =>
                {
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", testMethodStarting.TestClass.Class.Name);
                    Assert.Equal("TestMethod", testMethodStarting.TestMethod.Method.Name);
                },
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass.TestMethod", testCaseStarting.TestCase.DisplayName);
                },
                message =>
                {
                    var starting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal(starting.TestCase.DisplayName, starting.Test.DisplayName);
                },
                message =>
                {
                    var classConstructionStarting = Assert.IsAssignableFrom<ITestClassConstructionStarting>(message);
                    Assert.Equal(classConstructionStarting.TestCase.DisplayName, classConstructionStarting.Test.DisplayName);
                },
                message =>
                {
                    var classConstructionFinished = Assert.IsAssignableFrom<ITestClassConstructionFinished>(message);
                    Assert.Equal(classConstructionFinished.TestCase.DisplayName, classConstructionFinished.Test.DisplayName);
                },
                message =>
                {
                    var testPassed = Assert.IsAssignableFrom<ITestPassed>(message);
                    Assert.Equal(testPassed.TestCase.DisplayName, testPassed.Test.DisplayName);
                    Assert.NotEqual(0M, testPassed.ExecutionTime);
                },
                message =>
                {
                    var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
                    Assert.Equal(testFinished.TestCase.DisplayName, testFinished.Test.DisplayName);
                },
                message =>
                {
                    var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
                    Assert.Equal(1, testCaseFinished.TestsRun);
                    Assert.Equal(0, testCaseFinished.TestsFailed);
                    Assert.Equal(0, testCaseFinished.TestsSkipped);
                    Assert.NotEqual(0M, testCaseFinished.ExecutionTime);
                },
                message =>
                {
                    var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", testMethodFinished.TestClass.Class.Name);
                    Assert.Equal("TestMethod", testMethodFinished.TestMethod.Method.Name);
                },
                message =>
                {
                    var classFinished = Assert.IsAssignableFrom<ITestClassFinished>(message);
                    Assert.Equal(1, classFinished.TestsRun);
                    Assert.Equal(0, classFinished.TestsFailed);
                    Assert.Equal(0, classFinished.TestsSkipped);
                    Assert.NotEqual(0M, classFinished.ExecutionTime);
                },
                message =>
                {
                    var collectionFinished = Assert.IsAssignableFrom<ITestCollectionFinished>(message);
                    Assert.NotNull(collectionFinished.TestCollection);
                    Assert.Equal(1, collectionFinished.TestsRun);
                    Assert.Equal(0, collectionFinished.TestsFailed);
                    Assert.Equal(0, collectionFinished.TestsSkipped);
                    Assert.NotEqual(0M, collectionFinished.ExecutionTime);
                    // TODO: There will need to be more tests here eventually...
                },
                message =>
                {
                    var assemblyFinished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
                    Assert.Equal(1, assemblyFinished.TestsRun);
                    Assert.Equal(0, assemblyFinished.TestsFailed);
                    Assert.Equal(0, assemblyFinished.TestsSkipped);
                    Assert.NotEqual(0M, assemblyFinished.ExecutionTime);
                }
            );
        }
    }

    public class SkippedTests : AcceptanceTestV2
    {
        [Fact]
        public void SingleSkippedTest()
        {
            List<IMessageSinkMessage> results = Run(typeof(SingleSkippedTestClass));

            var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
            Assert.Equal("Xunit2AcceptanceTests+SingleSkippedTestClass.TestMethod", skippedMessage.Test.DisplayName);
            Assert.Equal("This is a skipped test", skippedMessage.Reason);

            var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
            Assert.Equal(1, classFinishedMessage.TestsSkipped);

            var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
            Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
        }
    }

    [CollectionDefinition("Timeout Tests", DisableParallelization = true)]
    public class TimeoutTestsCollection { }

    [Collection("Timeout Tests")]
    public class TimeoutTests : AcceptanceTestV2
    {
        // This test is a little sketchy, because it relies on the execution of the acceptance test to happen in less time
        // than the timeout. The timeout is set arbitrarily high in order to give some padding to the timing, but even on
        // a Core i7-7820HK, the execution time is ~ 400 milliseconds for what should be about 10 milliseconds of wait
        // time. If this test becomes flaky, a higher value than 10000 could be considered.
        [Fact]
        public void TimedOutTest()
        {
            var stopwatch = Stopwatch.StartNew();
            var results = Run(typeof(ClassUnderTest));
            stopwatch.Stop();

            var passedMessage = Assert.Single(results.OfType<ITestPassed>());
            Assert.Equal("Xunit2AcceptanceTests+TimeoutTests+ClassUnderTest.ShortRunningTest", passedMessage.Test.DisplayName);

            var failedMessage = Assert.Single(results.OfType<ITestFailed>());
            Assert.Equal("Xunit2AcceptanceTests+TimeoutTests+ClassUnderTest.LongRunningTest", failedMessage.Test.DisplayName);
            Assert.Equal("Test execution timed out after 10 milliseconds", failedMessage.Messages.Single());

            Assert.True(stopwatch.ElapsedMilliseconds < 10000, "Elapsed time should be less than 10 seconds");
        }

        class ClassUnderTest
        {
            [Fact(Timeout = 10)]
            public Task LongRunningTest() => Task.Delay(10000);

            [Fact(Timeout = 10000)]
            public void ShortRunningTest() => Task.Delay(10);
        }
    }

    public class NonStartedTasks : AcceptanceTestV2
    {
        [Fact]
        public void TestWithUnstartedTaskThrows()
        {
            var stopwatch = Stopwatch.StartNew();
            var results = Run(typeof(ClassUnderTest));
            stopwatch.Stop();

            var failedMessage = Assert.Single(results.OfType<ITestFailed>());
            Assert.Equal("Xunit2AcceptanceTests+NonStartedTasks+ClassUnderTest.NonStartedTask", failedMessage.Test.DisplayName);
            Assert.Equal("Test method returned a non-started Task (tasks must be started before being returned)", failedMessage.Messages.Single());
        }

        class ClassUnderTest
        {
            [Fact]
            public Task NonStartedTask() => new Task(() => { Thread.Sleep(1000); });
        }
    }

    public class FailingTests : AcceptanceTestV2
    {
        [Fact]
        public void SingleFailingTest()
        {
            List<IMessageSinkMessage> results = Run(typeof(SingleFailingTestClass));

            var failedMessage = Assert.Single(results.OfType<ITestFailed>());
            Assert.Equal(typeof(TrueException).FullName, failedMessage.ExceptionTypes.Single());

            var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
            Assert.Equal(1, classFinishedMessage.TestsFailed);

            var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
            Assert.Equal(1, collectionFinishedMessage.TestsFailed);
        }
    }

    public class ClassFailures : AcceptanceTestV2
    {
        [Fact]
        public void TestFailureResultsFromThrowingCtorInTestClass()
        {
            var messages = Run<ITestFailed>(typeof(ClassUnderTest_CtorFailure));

            Assert.Collection(messages,
                msg => Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single())
            );
        }

        [Fact]
        public void TestFailureResultsFromThrowingDisposeInTestClass()
        {
            var messages = Run<ITestFailed>(typeof(ClassUnderTest_DisposeFailure));

            Assert.Collection(messages,
                msg => Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single())
            );
        }

        [Fact]
        public void CompositeTestFailureResultsFromFailingTestsPlusThrowingDisposeInTestClass()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var messages = Run<ITestFailed>(typeof(ClassUnderTest_FailingTestAndDisposeFailure));

            var msg = Assert.Single(messages);
            Assert.Equal("System.AggregateException : One or more errors occurred." + Environment.NewLine +
                         "---- Assert.Equal() Failure" + Environment.NewLine +
                         "Expected: 2" + Environment.NewLine +
                         "Actual:   3" + Environment.NewLine +
                         "---- System.DivideByZeroException : Attempted to divide by zero.", Xunit.ExceptionUtility.CombineMessages(msg));
        }

        class ClassUnderTest_CtorFailure
        {
            public ClassUnderTest_CtorFailure()
            {
                throw new DivideByZeroException();
            }

            [Fact]
            public void TheTest() { }
        }

        class ClassUnderTest_DisposeFailure : IDisposable
        {
            public void Dispose()
            {
                throw new DivideByZeroException();
            }

            [Fact]
            public void TheTest() { }
        }

        class ClassUnderTest_FailingTestAndDisposeFailure : IDisposable
        {
            public void Dispose()
            {
                throw new DivideByZeroException();
            }

            [Fact]
            public void TheTest()
            {
                Assert.Equal(2, 3);
            }
        }
    }

    public class StaticClassSupport : AcceptanceTestV2
    {
        [Fact]
        public void TestsCanBeInStaticClasses()
        {
            var testMessages = Run<ITestResultMessage>(typeof(StaticClassUnderTest));

            var testMessage = Assert.Single(testMessages);
            Assert.Equal("Xunit2AcceptanceTests+StaticClassSupport+StaticClassUnderTest.Passing", testMessage.Test.DisplayName);
            Assert.IsAssignableFrom<ITestPassed>(testMessage);
        }

        static class StaticClassUnderTest
        {
            [Fact]
            public static void Passing() { }
        }
    }

    public class ErrorAggregation : AcceptanceTestV2
    {
        [Fact]
        public void EachTestMethodHasIndividualExceptionMessage()
        {
            var testMessages = Run<ITestFailed>(typeof(ClassUnderTest));

            var equalFailure = Assert.Single(testMessages, msg => msg.Test.DisplayName == "Xunit2AcceptanceTests+ErrorAggregation+ClassUnderTest.EqualFailure");
            Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

            var notNullFailure = Assert.Single(testMessages, msg => msg.Test.DisplayName == "Xunit2AcceptanceTests+ErrorAggregation+ClassUnderTest.NotNullFailure");
            Assert.Contains("Assert.NotNull() Failure", notNullFailure.Messages.Single());
        }

        class ClassUnderTest
        {
            [Fact]
            public void EqualFailure()
            {
                Assert.Equal(42, 40);
            }

            [Fact]
            public void NotNullFailure()
            {
                Assert.NotNull(null);
            }
        }
    }

    public class TestOrdering : AcceptanceTestV2
    {
        [Fact]
        public void OverrideOfOrderingAtCollectionLevel()
        {
            var testMessages = Run<ITestPassed>(typeof(TestClassUsingCollection));

            Assert.Collection(testMessages,
                message => Assert.Equal("Test1", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("Test2", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("Test3", message.TestCase.TestMethod.Method.Name)
            );
        }

        [CollectionDefinition("Ordered Collection")]
        [TestCaseOrderer("Xunit2AcceptanceTests+TestOrdering+AlphabeticalOrderer", "test.xunit.execution")]
        public class CollectionClass { }

        [Collection("Ordered Collection")]
        class TestClassUsingCollection
        {
            [Fact]
            public void Test1() { }

            [Fact]
            public void Test3() { }

            [Fact]
            public void Test2() { }
        }

        [Fact]
        public void OverrideOfOrderingAtClassLevel()
        {
            var testMessages = Run<ITestPassed>(typeof(TestClassWithoutCollection));

            Assert.Collection(testMessages,
                message => Assert.Equal("Test1", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("Test2", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("Test3", message.TestCase.TestMethod.Method.Name)
            );
        }

        [TestCaseOrderer("Xunit2AcceptanceTests+TestOrdering+AlphabeticalOrderer", "test.xunit.execution")]
        public class TestClassWithoutCollection
        {
            [Fact]
            public void Test1() { }

            [Fact]
            public void Test3() { }

            [Fact]
            public void Test2() { }
        }

        public class AlphabeticalOrderer : ITestCaseOrderer
        {
            public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
                where TTestCase : ITestCase
            {
                var result = testCases.ToList();
                result.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
                return result;
            }
        }
    }

    public class TestNonParallelOrdering : AcceptanceTestV2
    {
        [Fact]
        public void NonParallelCollectionsRunLast()
        {
            var testMessages = Run<ITestPassed>(new[] {
                typeof(TestClassNonParallelCollection),
                typeof(TestClassParallelCollection)
            });

            Assert.Collection(testMessages,
                message => Assert.Equal("Test1", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("Test2", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("IShouldBeLast1", message.TestCase.TestMethod.Method.Name),
                message => Assert.Equal("IShouldBeLast2", message.TestCase.TestMethod.Method.Name)
            );
        }

        [CollectionDefinition("Parallel Ordered Collection")]
        [TestCaseOrderer("Xunit2AcceptanceTests+TestOrdering+AlphabeticalOrderer", "test.xunit.execution")]
        public class CollectionClass { }

        [Collection("Parallel Ordered Collection")]
        class TestClassParallelCollection
        {
            [Fact]
            public void Test1() { }

            [Fact]
            public void Test2() { }
        }

        [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
        [TestCaseOrderer("Xunit2AcceptanceTests+TestOrdering+AlphabeticalOrderer", "test.xunit.execution")]
        public class TestClassNonParallelCollectionDefinition { }

        [Collection("Non-Parallel Collection")]
        class TestClassNonParallelCollection
        {
            [Fact]
            public void IShouldBeLast2() { }

            [Fact]
            public void IShouldBeLast1() { }
        }
    }

    public class CustomFacts : AcceptanceTestV2
    {
        [Fact]
        public void CanUseCustomFactAttribute()
        {
            var msgs = Run<ITestPassed>(typeof(ClassWithCustomFact));

            Assert.Collection(msgs,
                msg => Assert.Equal("Xunit2AcceptanceTests+CustomFacts+ClassWithCustomFact.Passing", msg.Test.DisplayName)
            );
        }

        class MyCustomFact : FactAttribute { }

        class ClassWithCustomFact
        {
            [MyCustomFact]
            public void Passing() { }
        }

        [Fact]
        public void CanUseCustomFactWithArrayParameters()
        {
            var msgs = Run<ITestPassed>(typeof(ClassWithCustomArrayFact));

            Assert.Collection(msgs,
                msg => Assert.Equal("Xunit2AcceptanceTests+CustomFacts+ClassWithCustomArrayFact.Passing", msg.Test.DisplayName)
            );
        }

        class MyCustomArrayFact : FactAttribute
        {
            public MyCustomArrayFact(params string[] values) { }
        }

        class ClassWithCustomArrayFact
        {
            [MyCustomArrayFact("1", "2", "3")]
            public void Passing() { }
        }

        [Fact]
        public void CannotMixMultipleFactDerivedAttributes()
        {
            var msgs = Run<ITestFailed>(typeof(ClassWithMultipleFacts)).ToList();

            Assert.Collection(msgs,
                msg =>
                {
                    Assert.Equal("Xunit2AcceptanceTests+CustomFacts+ClassWithMultipleFacts.Passing", msg.Test.DisplayName);
                    Assert.Equal("System.InvalidOperationException", msg.ExceptionTypes.Single());
                    Assert.Equal("Test method 'Xunit2AcceptanceTests+CustomFacts+ClassWithMultipleFacts.Passing' has multiple [Fact]-derived attributes", msg.Messages.Single());
                }
            );
        }

        class ClassWithMultipleFacts
        {
            [Fact]
            [MyCustomFact]
            public void Passing() { }
        }
    }

    public class TestOutput : AcceptanceTestV2
    {
        [Fact]
        public void SendOutputMessages()
        {
            var msgs = Run(typeof(ClassUnderTest));

            var idxOfTestPassed = msgs.FindIndex(msg => msg is ITestPassed);
            Assert.True(idxOfTestPassed >= 0, "Test should have passed");

            var idxOfFirstTestOutput = msgs.FindIndex(msg => msg is ITestOutput);
            Assert.True(idxOfFirstTestOutput >= 0, "Test should have output");
            Assert.True(idxOfFirstTestOutput < idxOfTestPassed, "Test output messages should precede test result");

            Assert.Collection(msgs.OfType<ITestOutput>(),
                msg =>
                {
                    var outputMessage = Assert.IsAssignableFrom<ITestOutput>(msg);
                    Assert.Equal("This is output in the constructor" + Environment.NewLine, outputMessage.Output);
                },
                msg =>
                {
                    var outputMessage = Assert.IsAssignableFrom<ITestOutput>(msg);
                    Assert.Equal("This is test output" + Environment.NewLine, outputMessage.Output);
                },
                msg =>
                {
                    var outputMessage = Assert.IsAssignableFrom<ITestOutput>(msg);
                    Assert.Equal("This is output in Dispose" + Environment.NewLine, outputMessage.Output);
                }
            );
        }

        class ClassUnderTest : IDisposable
        {
            readonly ITestOutputHelper output;

            public ClassUnderTest(ITestOutputHelper output)
            {
                this.output = output;

                output.WriteLine("This is output in the constructor");
            }

            public void Dispose()
            {
                output.WriteLine("This is {0} in Dispose", "output");
            }

            [Fact]
            public void TestMethod()
            {
                output.WriteLine("This is test output");
            }

        }
    }

    public class AsyncLifetime : AcceptanceTestV2
    {
        [Fact]
        public void AsyncLifetimeAcceptanceTest()
        {
            var messages = Run<ITestPassed>(typeof(ClassWithAsyncLifetime));

            var message = Assert.Single(messages);
            AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync", "Dispose");
        }

        class ClassWithAsyncLifetime : IAsyncLifetime, IDisposable
        {
            protected readonly ITestOutputHelper output;

            public ClassWithAsyncLifetime(ITestOutputHelper output)
            {
                this.output = output;

                output.WriteLine("Constructor");
            }

            public virtual Task InitializeAsync()
            {
                output.WriteLine("InitializeAsync");
                return Task.FromResult(0);
            }

            public virtual void Dispose()
            {
                output.WriteLine("Dispose");
            }

            public virtual Task DisposeAsync()
            {
                output.WriteLine("DisposeAsync");
                return Task.FromResult(0);
            }

            [Fact]
            public virtual void TheTest()
            {
                output.WriteLine("Run Test");
            }
        }

        [Fact]
        public void ThrowingConstructor()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithAsyncLifetime_ThrowingCtor));

            var message = Assert.Single(messages);
            AssertOperations(message, "Constructor");
        }

        class ClassWithAsyncLifetime_ThrowingCtor : ClassWithAsyncLifetime
        {
            public ClassWithAsyncLifetime_ThrowingCtor(ITestOutputHelper output)
                : base(output)
            {
                throw new DivideByZeroException();
            }
        }

        [Fact]
        public void ThrowingInitializeAsync()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithAsyncLifetime_ThrowingInitializeAsync));

            var message = Assert.Single(messages);
            AssertOperations(message, "Constructor", "InitializeAsync", "Dispose");
        }

        class ClassWithAsyncLifetime_ThrowingInitializeAsync : ClassWithAsyncLifetime
        {
            public ClassWithAsyncLifetime_ThrowingInitializeAsync(ITestOutputHelper output) : base(output) { }

            public override async Task InitializeAsync()
            {
                await base.InitializeAsync();

                throw new DivideByZeroException();
            }
        }

        [Fact]
        public void ThrowingDisposeAsync()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithAsyncLifetime_ThrowingDisposeAsync));

            var message = Assert.Single(messages);
            AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync", "Dispose");
        }

        class ClassWithAsyncLifetime_ThrowingDisposeAsync : ClassWithAsyncLifetime
        {
            public ClassWithAsyncLifetime_ThrowingDisposeAsync(ITestOutputHelper output) : base(output) { }

            public override async Task DisposeAsync()
            {
                await base.DisposeAsync();

                throw new DivideByZeroException();
            }
        }

        [Fact]
        public void FailingTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithAsyncLifetime_FailingTest));

            var message = Assert.Single(messages);
            AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync", "Dispose");
        }

        class ClassWithAsyncLifetime_FailingTest : ClassWithAsyncLifetime
        {
            public ClassWithAsyncLifetime_FailingTest(ITestOutputHelper output) : base(output) { }

            public override void TheTest()
            {
                base.TheTest();

                throw new DivideByZeroException();
            }
        }

        void AssertOperations(ITestResultMessage result, params string[] operations)
        {
            Assert.Collection(
                result.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                operations.Select<string, Action<string>>(expected => actual => Assert.Equal(expected, actual)).ToArray()
            );
        }
    }

    class NoTestsClass { }

    class SinglePassingTestClass
    {
        [Fact]
        public void TestMethod() { }
    }

    class SingleSkippedTestClass
    {
        [Fact(Skip = "This is a skipped test")]
        public void TestMethod()
        {
            Assert.True(false);
        }
    }

    class SingleFailingTestClass
    {
        [Fact]
        public void TestMethod()
        {
            Assert.True(false);
        }
    }
}

#endif
