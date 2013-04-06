using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Linq;

public class Xunit2AcceptanceTests
{
    public class EndToEndMessageInspection : AcceptanceTest
    {
        [Fact]
        public void NoTests()
        {
            List<ITestMessage> results = Run(typeof(NoTestsClass));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message =>
                {
                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
                    Assert.Equal(0, finished.TestsRun);
                    Assert.Equal(0, finished.TestsFailed);
                    Assert.Equal(0, finished.TestsSkipped);
                    Assert.Equal(0M, finished.ExecutionTime);
                }
            );
        }

        [Fact]
        public void SinglePassingTest()
        {
            List<ITestMessage> results = Run(typeof(SinglePassingTestClass));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message =>
                {
                    var collectionStarting = Assert.IsAssignableFrom<ITestCollectionStarting>(message);
                    // TODO: How do we represent collections?
                },
                message =>
                {
                    var classStarting = Assert.IsAssignableFrom<ITestClassStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", classStarting.ClassName);
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
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", testMethodStarting.ClassName);
                    Assert.Equal("TestMethod", testMethodStarting.MethodName);
                },
                message =>
                {
                    var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal("Xunit2AcceptanceTests+SinglePassingTestClass", testMethodFinished.ClassName);
                    Assert.Equal("TestMethod", testMethodFinished.MethodName);
                },
                message =>
                {
                    var testPassed = Assert.IsAssignableFrom<ITestPassed>(message);
                    Assert.Equal(testPassed.TestCase.DisplayName, testPassed.TestDisplayName);
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
                },
                message =>
                {
                    var classFinished = Assert.IsAssignableFrom<ITestClassFinished>(message);
                    Assert.Equal(1, classFinished.TestsRun);
                    Assert.Equal(0, classFinished.TestsFailed);
                    Assert.Equal(0, classFinished.TestsSkipped);
                },
                message =>
                {
                    var collectionFinished = Assert.IsAssignableFrom<ITestCollectionFinished>(message);
                    Assert.Equal(1, collectionFinished.TestsRun);
                    Assert.Equal(0, collectionFinished.TestsFailed);
                    Assert.Equal(0, collectionFinished.TestsSkipped);
                    // TODO: How do we represent collections?
                },
                message =>
                {
                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
                    Assert.Equal(1, finished.TestsRun);
                    Assert.Equal(0, finished.TestsFailed);
                    Assert.Equal(0, finished.TestsSkipped);
                }
            );
        }

    }

    public class SkippedTests : AcceptanceTest
    {
        [Fact]
        public void SingleSkippedTest()
        {
            List<ITestSkipped> results = Run<ITestSkipped>(typeof(SingleSkippedTestClass));

            var skippedMessage = Assert.Single(results);
            Assert.Equal("Xunit2AcceptanceTests+SingleSkippedTestClass.TestMethod", skippedMessage.TestDisplayName);
            Assert.Equal("This is a skipped test", skippedMessage.Reason);
        }
    }

    public class FailingTests : AcceptanceTest
    {
        [Fact]
        public void SingleFailingTest()
        {
            List<ITestFailed> results = Run<ITestFailed>(typeof(SingleFailingTestClass));

            var failedMessage = Assert.Single(results);
            Assert.Equal(typeof(TrueException).FullName, failedMessage.ExceptionType);
        }
    }

    public class Cancellation : AcceptanceTest
    {
        [Fact]
        public void ReturningFalseFromMessageSinkCancelsDiscovery()
        {
            bool alreadyCancelled = false;

            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg =>
            {
                if (msg is ITestCaseDiscoveryMessage)
                {
                    Assert.False(alreadyCancelled);
                    alreadyCancelled = true;
                    return false;
                }

                return true;
            });

            Assert.Empty(results);  // Should not have run any tests
        }

        [Fact]
        public void CancelDuringIAfterTestFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is IAfterTestFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringIAfterTestStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is IAfterTestStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringIBeforeTestFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is IBeforeTestFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringIBeforeTestStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is IBeforeTestStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestAssemblyStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestAssemblyStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestCaseFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestCaseFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestCaseStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestCaseStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestClassConstructionFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestClassConstructionFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestClassConstructionStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestClassConstructionStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestClassDisposeFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestClassDisposeFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestClassDisposeStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestClassDisposeStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestClassFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestClassFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                // TestMethod2
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestClassStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestClassStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestCollectionFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestCollectionFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                // TestMethod2
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestCollectionStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestCollectionStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestFailed()
        {
            List<ITestMessage> results = Run(typeof(FailingClassUnderTest), msg => !(msg is ITestFailed));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFailed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestMethodFinished()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestMethodFinished));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestMethodStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestMethodStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestPassed()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestPassed));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
                message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                message => Assert.IsAssignableFrom<ITestPassed>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestSkipped()
        {
            List<ITestMessage> results = Run(typeof(SkippingClassUnderTest), msg => !(msg is ITestSkipped));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestSkipped>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        [Fact]
        public void CancelDuringITestStarting()
        {
            List<ITestMessage> results = Run(typeof(PassingClassUnderTest), msg => !(msg is ITestStarting));

            Assert.Collection(results,
                message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
                message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
                message => Assert.IsAssignableFrom<ITestClassStarting>(message),

                // TestMethod1
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        class MyBeforeAfter : BeforeAfterTestAttribute { }

        class FailingClassUnderTest : IDisposable
        {
            public void Dispose() { }

            [Fact, MyBeforeAfter]
            public void TestMethod1() { Assert.True(false); }

            [Fact, MyBeforeAfter]
            public void TestMethod2() { Assert.True(false); }
        }

        class PassingClassUnderTest : IDisposable
        {
            public void Dispose() { }

            [Fact, MyBeforeAfter]
            public void TestMethod1() { }

            [Fact, MyBeforeAfter]
            public void TestMethod2() { }
        }

        class SkippingClassUnderTest : IDisposable
        {
            public void Dispose() { }

            [Fact(Skip = "No bueno"), MyBeforeAfter]
            public void TestMethod1() { Assert.True(false); }

            [Fact(Skip = "No soup for you!"), MyBeforeAfter]
            public void TestMethod2() { Assert.True(false); }
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
