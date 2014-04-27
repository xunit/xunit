using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class Xunit2AcceptanceTests
{
    public class EndToEndMessageInspection : AcceptanceTest
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
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", classStarting.ClassName);
                },
                message =>
                {
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", testMethodStarting.ClassName);
                    Assert.Equal("TestMethod", testMethodStarting.MethodName);
                },
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass.TestMethod", testCaseStarting.TestCase.DisplayName);
                },
                message =>
                {
                    var starting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal(starting.TestCase.DisplayName, starting.TestDisplayName);
                },
                message =>
                {
                    var classConstructionStarting = Assert.IsAssignableFrom<ITestClassConstructionStarting>(message);
                    Assert.Equal(classConstructionStarting.TestCase.DisplayName, classConstructionStarting.TestDisplayName);
                },
                message =>
                {
                    var classConstructionFinished = Assert.IsAssignableFrom<ITestClassConstructionFinished>(message);
                    Assert.Equal(classConstructionFinished.TestCase.DisplayName, classConstructionFinished.TestDisplayName);
                },
                message =>
                {
                    var testPassed = Assert.IsAssignableFrom<ITestPassed>(message);
                    Assert.Equal(testPassed.TestCase.DisplayName, testPassed.TestDisplayName);
                    Assert.NotEqual(0M, testPassed.ExecutionTime);
                },
                message =>
                {
                    var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
                    Assert.Equal(testFinished.TestCase.DisplayName, testFinished.TestDisplayName);
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
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", testMethodFinished.ClassName);
                    Assert.Equal("TestMethod", testMethodFinished.MethodName);
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

    public class SkippedTests : AcceptanceTest
    {
        [Fact]
        public void SingleSkippedTest()
        {
            List<IMessageSinkMessage> results = Run(typeof(SingleSkippedTestClass));

            var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
            Assert.Equal("Xunit2AcceptanceTests+SingleSkippedTestClass.TestMethod", skippedMessage.TestDisplayName);
            Assert.Equal("This is a skipped test", skippedMessage.Reason);

            var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
            Assert.Equal(1, classFinishedMessage.TestsSkipped);

            var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
            Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
        }
    }

    public class FailingTests : AcceptanceTest
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

    public class ClassFailures : AcceptanceTest
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
            var messages = Run<ITestFailed>(typeof(ClassUnderTest_FailingTestAndDisposeFailure));

            var msg = Assert.Single(messages);
            Assert.Equal("System.AggregateException : One or more errors occurred." + Environment.NewLine +
                         "---- Assert.Equal() Failure" + Environment.NewLine +
                         "Expected: 2" + Environment.NewLine +
                         "Actual:   3" + Environment.NewLine +
                         "---- System.DivideByZeroException : Attempted to divide by zero.", ExceptionUtility.CombineMessages(msg));
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

    public class StaticClassSupport : AcceptanceTest
    {
        [Fact]
        public void TestsCanBeInStaticClasses()
        {
            var testMessages = Run<ITestResultMessage>(typeof(StaticClassUnderTest));

            var testMessage = Assert.Single(testMessages);
            Assert.Equal("Xunit2AcceptanceTests+StaticClassSupport+StaticClassUnderTest.Passing", testMessage.TestDisplayName);
            Assert.IsAssignableFrom<ITestPassed>(testMessage);
        }

        static class StaticClassUnderTest
        {
            [Fact]
            public static void Passing() { }
        }
    }

    public class ErrorAggregation : AcceptanceTest
    {
        [Fact]
        public void EachTestMethodHasIndividualExceptionMessage()
        {
            var testMessages = Run<ITestFailed>(typeof(ClassUnderTest));

            var equalFailure = Assert.Single(testMessages, msg => msg.TestDisplayName == "Xunit2AcceptanceTests+ErrorAggregation+ClassUnderTest.EqualFailure");
            Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

            var notNullFailure = Assert.Single(testMessages, msg => msg.TestDisplayName == "Xunit2AcceptanceTests+ErrorAggregation+ClassUnderTest.NotNullFailure");
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

    public class TestOrdering : AcceptanceTest
    {
        [Fact]
        public void OverrideOfOrderingAtCollectionLevel()
        {
            var testMessages = Run<ITestPassed>(typeof(TestClassUsingCollection));

            Assert.Collection(testMessages,
                message => Assert.Equal("Test1", message.TestCase.Method.Name),
                message => Assert.Equal("Test2", message.TestCase.Method.Name),
                message => Assert.Equal("Test3", message.TestCase.Method.Name)
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
                message => Assert.Equal("Test1", message.TestCase.Method.Name),
                message => Assert.Equal("Test2", message.TestCase.Method.Name),
                message => Assert.Equal("Test3", message.TestCase.Method.Name)
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
                result.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Method.Name, y.Method.Name));
                return result;
            }
        }
    }

    public class CustomFacts : AcceptanceTest
    {
        [Fact]
        public void CanUseCustomFactAttribute()
        {
            var msgs = Run<ITestPassed>(typeof(ClassWithCustomFact));

            Assert.Collection(msgs,
                msg => Assert.Equal("Xunit2AcceptanceTests+CustomFacts+ClassWithCustomFact.Passing", msg.TestDisplayName)
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
                msg => Assert.Equal("Xunit2AcceptanceTests+CustomFacts+ClassWithCustomArrayFact.Passing", msg.TestDisplayName)
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