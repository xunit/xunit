using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class Xunit2AcceptanceTests
{
    public class EndToEndMessageInspection : Xunit2AcceptanceTest
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
                    var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal(testStarting.TestCase.DisplayName, testStarting.TestDisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestClassConstructionStarting>(message);
                    Assert.Equal(testStarting.TestCase.DisplayName, testStarting.TestDisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestClassConstructionFinished>(message);
                    Assert.Equal(testStarting.TestCase.DisplayName, testStarting.TestDisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal(testStarting.TestCase.DisplayName, testStarting.TestDisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal(testStarting.TestCase.DisplayName, testStarting.TestDisplayName);
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
                    Assert.Equal(0M, testCaseFinished.ExecutionTime);
                },
                message =>
                {
                    var classFinished = Assert.IsAssignableFrom<ITestClassFinished>(message);
                    Assert.Equal(1, classFinished.TestsRun);
                    Assert.Equal(0, classFinished.TestsFailed);
                    Assert.Equal(0, classFinished.TestsSkipped);
                    Assert.Equal(0M, classFinished.ExecutionTime);
                },
                message =>
                {
                    var collectionFinished = Assert.IsAssignableFrom<ITestCollectionFinished>(message);
                    Assert.Equal(1, collectionFinished.TestsRun);
                    Assert.Equal(0, collectionFinished.TestsFailed);
                    Assert.Equal(0, collectionFinished.TestsSkipped);
                    Assert.Equal(0M, collectionFinished.ExecutionTime); // TODO: Measure time?
                    // TODO: How do we represent collections?
                },
                message =>
                {
                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
                    Assert.Equal(1, finished.TestsRun);
                    Assert.Equal(0, finished.TestsFailed);
                    Assert.Equal(0, finished.TestsSkipped);
                    Assert.Equal(0M, finished.ExecutionTime); // TODO: Measure time?
                }
            );
        }

    }

    public class SkippedTests : Xunit2AcceptanceTest
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

    public class FailingTests : Xunit2AcceptanceTest
    {
        [Fact]
        public void SingleFailingTest()
        {
            List<ITestFailed> results = Run<ITestFailed>(typeof(SingleFailingTestClass));

            var failedMessage = Assert.Single(results);
            System.Exception failedMessageException = failedMessage.Exception;
            Assert.IsType<TrueException>(failedMessageException);
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
