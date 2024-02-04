using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class ExecutionSinkTests
{
    public class Cancellation
    {
        readonly IMessageSinkWithTypes innerSink;
        readonly IMessageSinkMessage testMessage;

        public Cancellation()
        {
            innerSink = Substitute.For<IMessageSinkWithTypes>();
            innerSink.OnMessageWithTypes(null, null).ReturnsForAnyArgs(true);

            testMessage = Substitute.For<IMessageSinkMessage>();
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { CancelThunk = () => true });

            var result = sink.OnMessage(testMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { CancelThunk = () => false });

            var result = sink.OnMessage(testMessage);

            Assert.True(result);
        }
    }

    public class FailSkips
    {
        readonly IMessageSinkWithTypes innerSink;
        readonly ExecutionSink sink;

        public FailSkips()
        {
            innerSink = Substitute.For<IMessageSinkWithTypes>();

            var options = new ExecutionSinkOptions { FailSkips = true };
            sink = new ExecutionSink(innerSink, options);
        }

        [Fact]
        public void OnITestSkipped_TransformsToITestFailed()
        {
            var inputMessage = Mocks.TestSkipped("The skipped test", "The skip reason");

            sink.OnMessage(inputMessage);

            var outputMessage = innerSink.Captured(x => x.OnMessageWithTypes(null, null)).Arg<ITestFailed>();
            Assert.Equal(inputMessage.Test, outputMessage.Test);
            Assert.Equal(0M, inputMessage.ExecutionTime);
            Assert.Empty(inputMessage.Output);
            Assert.Equal("FAIL_SKIP", outputMessage.ExceptionTypes.Single());
            Assert.Equal("The skip reason", outputMessage.Messages.Single());
            Assert.Empty(outputMessage.StackTraces.Single());
        }

        [Fact]
        public void OnITestCollectionFinished_CountsSkipsAsFails()
        {
            var inputMessage = Mocks.TestCollectionFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);

            sink.OnMessage(inputMessage);

            var outputMessage = innerSink.Captured(x => x.OnMessageWithTypes(null, null)).Arg<ITestCollectionFinished>();
            Assert.Equal(24, outputMessage.TestsRun);
            Assert.Equal(11, outputMessage.TestsFailed);
            Assert.Equal(0, outputMessage.TestsSkipped);
        }

        [Fact]
        public void OnITestAssemblyFinished_CountsSkipsAsFails()
        {
            var inputMessage = Mocks.TestAssemblyFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);

            sink.OnMessage(inputMessage);

            var outputMessage = innerSink.Captured(x => x.OnMessageWithTypes(null, null)).Arg<ITestAssemblyFinished>();
            Assert.Equal(24, outputMessage.TestsRun);
            Assert.Equal(11, outputMessage.TestsFailed);
            Assert.Equal(0, outputMessage.TestsSkipped);
        }
    }

    public class LongRunningTestDetection
    {
        [Fact]
        public async void ShortRunningTests_NoMessages()
        {
            var events = new List<LongRunningTestsSummary>();
            using var sink = new TestableExecutionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
            var testCase1 = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase1));
            await sink.AdvanceClockAsync(100);
            sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Empty(events);
        }

        [Fact]
        public async void LongRunningTest_Once_WithCallback()
        {
            var events = new List<LongRunningTestsSummary>();
            using var sink = new TestableExecutionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
            var testCase = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
            var receivedTestCasePair = Assert.Single(@event.TestCases);
            Assert.Same(testCase, receivedTestCasePair.Key);
            Assert.Equal(TimeSpan.FromMilliseconds(1500), receivedTestCasePair.Value);
        }

        [Fact]
        public async void OnlyIncludesLongRunningTests()
        {
            var events = new List<LongRunningTestsSummary>();
            using var sink = new TestableExecutionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
            var testCase1 = Substitute.For<ITestCase>();
            var testCase2 = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase1));
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseStarting(testCase2));  // Started later, hasn't run long enough
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));
            sink.OnMessage(new TestCaseFinished(testCase2, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
            var receivedTestCasePair = Assert.Single(@event.TestCases);
            Assert.Same(testCase1, receivedTestCasePair.Key);
            Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
        }

        [Fact]
        public async void LongRunningTest_Twice_WithCallback()
        {
            var events = new List<LongRunningTestsSummary>();
            using var sink = new TestableExecutionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
            var testCase = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1000);
            await sink.AdvanceClockAsync(500);
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Collection(events,
                @event =>
                {
                    Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
                    var receivedTestCasePair = Assert.Single(@event.TestCases);
                    Assert.Same(testCase, receivedTestCasePair.Key);
                    Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
                },
                @event =>
                {
                    Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
                    var receivedTestCasePair = Assert.Single(@event.TestCases);
                    Assert.Same(testCase, receivedTestCasePair.Key);
                    Assert.Equal(TimeSpan.FromSeconds(2), receivedTestCasePair.Value);
                }
            );
        }

        [Fact]
        public async void LongRunningTest_Once_WithDiagnosticMessageSink()
        {
            var events = new List<IDiagnosticMessage>();
            var diagSink = Substitute.For<IMessageSink>();
            diagSink.WhenForAnyArgs(x => x.OnMessage(null))
                    .Do(callInfo =>
                    {
                        var message = callInfo.Arg<IMessageSinkMessage>();
                        if (message is IDiagnosticMessage diagnosticMessage)
                            events.Add(diagnosticMessage);
                    });
            using var sink = new TestableExecutionSink(longRunningSeconds: 1, diagnosticMessageSink: diagSink);
            var testCase = Substitute.For<ITestCase>();
            testCase.DisplayName.Returns("My test display name");

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", @event.Message);
        }

        [Fact]
        public async void LongRunningTest_Twice_WithDiagnosticMessageSink()
        {
            var events = new List<IDiagnosticMessage>();
            var diagSink = Substitute.For<IMessageSink>();
            diagSink.WhenForAnyArgs(x => x.OnMessage(null))
                    .Do(callInfo =>
                    {
                        var message = callInfo.Arg<IMessageSinkMessage>();
                        if (message is IDiagnosticMessage diagnosticMessage)
                            events.Add(diagnosticMessage);
                    });
            using var sink = new TestableExecutionSink(longRunningSeconds: 1, diagnosticMessageSink: diagSink);
            var testCase = Substitute.For<ITestCase>();
            testCase.DisplayName.Returns("My test display name");

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1000);
            await sink.AdvanceClockAsync(500);
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Collection(events,
                @event => Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", @event.Message),
                @event => Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:02", @event.Message)
            );
        }

        class TestableExecutionSink : ExecutionSink
        {
            volatile bool stop = false;
            volatile int stopEventTriggerCount;
            DateTime utcNow = DateTime.UtcNow;
            readonly AutoResetEvent workEvent = new(initialState: false);

            public TestableExecutionSink(int longRunningSeconds, IMessageSink diagnosticMessageSink)
                : base(Substitute.For<IMessageSinkWithTypes>(),
                       new ExecutionSinkOptions
                       {
                           DiagnosticMessageSink = diagnosticMessageSink,
                           LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
                       })
            { }

            public TestableExecutionSink(int longRunningSeconds, Action<LongRunningTestsSummary> callback)
                : base(Substitute.For<IMessageSinkWithTypes>(),
                       new ExecutionSinkOptions
                       {
                           LongRunningTestCallback = callback,
                           LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
                       })
            { }

            protected override DateTime UtcNow => utcNow;

            public async Task AdvanceClockAsync(int milliseconds)
            {
                utcNow += TimeSpan.FromMilliseconds(milliseconds);

                var currentCount = stopEventTriggerCount;
                workEvent.Set();

                var stopTime = DateTime.UtcNow.AddSeconds(60);

                while (stopTime > DateTime.UtcNow)
                {
                    await Task.Delay(25);
                    if (currentCount != stopEventTriggerCount)
                        return;
                }

                throw new InvalidOperationException("After AdvanceClock, next work run never happened.");
            }

            public override void Dispose()
            {
                stop = true;
                workEvent.Set();

                var stopTime = DateTime.UtcNow.AddSeconds(60);

                while (stopTime > DateTime.UtcNow)
                {
                    Thread.Sleep(25);
                    if (stopEventTriggerCount == -1)
                    {
                        workEvent.Dispose();
                        return;
                    }
                }

                throw new InvalidOperationException("Worker thread did not shut down within 60 seconds.");
            }

            protected override bool WaitForStopEvent(int millionsecondsDelay)
            {
                Interlocked.Increment(ref stopEventTriggerCount);

                workEvent.WaitOne();

                if (stop)
                {
                    stopEventTriggerCount = -1;
                    return true;
                }

                return false;
            }

            public bool OnMessage(IMessageSinkMessage message)
                => OnMessageWithTypes(message, null);
        }
    }

    public class Messages
    {
        public class TestAssemblyFinished
        {
            [Fact]
            public void EnsureInnerHandlerIsCalledBeforeFinishedIsSet()
            {
                var innerSink = Substitute.For<IMessageSinkWithTypes>();
                var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions());
                bool? isFinishedDuringDispatch = default;
                innerSink
                    .OnMessageWithTypes(Arg.Any<IMessageSinkMessage>(), Arg.Any<HashSet<string>>())
                    .Returns(callInfo =>
                    {
                        if (callInfo.Arg<IMessageSinkMessage>() is ITestAssemblyFinished)
                            isFinishedDuringDispatch = sink.Finished.WaitOne(0);
                        return true;
                    });

                sink.OnMessageWithTypes(Substitute.For<ITestAssemblyFinished>(), null);
                var isFinishedAfterDispatch = sink.Finished.WaitOne(0);

                Assert.False(isFinishedDuringDispatch);
                Assert.True(isFinishedAfterDispatch);
            }
        }
    }

    public class XmlCreation
    {
        readonly IMessageSinkWithTypes innerSink;

        public XmlCreation()
        {
            innerSink = Substitute.For<IMessageSinkWithTypes>();
        }

        [Fact]
        public void AddsAssemblyStartingInformationToXml()
        {
            var assemblyStarting = Substitute.For<ITestAssemblyStarting>();
            assemblyStarting.TestAssembly.Assembly.AssemblyPath.Returns("assembly");
            assemblyStarting.TestAssembly.ConfigFileName.Returns("config");
            assemblyStarting.StartTime.Returns(new DateTime(2013, 7, 6, 16, 24, 32));
            assemblyStarting.TestEnvironment.Returns("256-bit MentalFloss");
            assemblyStarting.TestFrameworkDisplayName.Returns("xUnit.net v14.42");

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(assemblyStarting);

            Assert.Equal("assembly", assemblyElement.Attribute("name")?.Value);
            Assert.Equal("256-bit MentalFloss", assemblyElement.Attribute("environment")?.Value);
            Assert.Equal("xUnit.net v14.42", assemblyElement.Attribute("test-framework")?.Value);
            Assert.Equal("config", assemblyElement.Attribute("config-file")?.Value);
            Assert.Equal("2013-07-06", assemblyElement.Attribute("run-date")?.Value);
            Assert.Equal("16:24:32", assemblyElement.Attribute("run-time")?.Value);
        }

        [Fact]
        public void AssemblyStartingDoesNotIncludeNullConfigFile()
        {
            var assemblyStarting = Substitute.For<ITestAssemblyStarting>();
            assemblyStarting.TestAssembly.ConfigFileName.Returns((string)null);

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(assemblyStarting);

            Assert.Null(assemblyElement.Attribute("config-file"));
        }

        [CulturedFact]
        public void AddsAssemblyFinishedInformationToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);
            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });
            var errorMessage = Substitute.For<IErrorMessage>();
            errorMessage.ExceptionTypes.Returns(new[] { "ExceptionType" });
            errorMessage.Messages.Returns(new[] { "Message" });
            errorMessage.StackTraces.Returns(new[] { "Stack" });

            sink.OnMessage(errorMessage);
            sink.OnMessage(assemblyFinished);

            Assert.Equal("2112", assemblyElement.Attribute("total").Value);
            Assert.Equal("2064", assemblyElement.Attribute("passed").Value);
            Assert.Equal("42", assemblyElement.Attribute("failed").Value);
            Assert.Equal("6", assemblyElement.Attribute("skipped").Value);
            Assert.Equal(123.457M.ToString(CultureInfo.InvariantCulture), assemblyElement.Attribute("time").Value);
            Assert.Equal("1", assemblyElement.Attribute("errors").Value);
        }

        [CulturedFact]
        public void AddsTestCollectionElementsToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCollection = Substitute.For<ITestCollection>();
            testCollection.DisplayName.Returns("Collection Name");
            var testCollectionFinished = Substitute.For<ITestCollectionFinished>();
            testCollectionFinished.TestCollection.Returns(testCollection);
            testCollectionFinished.TestsRun.Returns(2112);
            testCollectionFinished.TestsFailed.Returns(42);
            testCollectionFinished.TestsSkipped.Returns(6);
            testCollectionFinished.ExecutionTime.Returns(123.4567M);

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testCollectionFinished);
            sink.OnMessage(assemblyFinished);

            var collectionElement = Assert.Single(assemblyElement.Elements("collection"));
            Assert.Equal("Collection Name", collectionElement.Attribute("name").Value);
            Assert.Equal("2112", collectionElement.Attribute("total").Value);
            Assert.Equal("2064", collectionElement.Attribute("passed").Value);
            Assert.Equal("42", collectionElement.Attribute("failed").Value);
            Assert.Equal("6", collectionElement.Attribute("skipped").Value);
            Assert.Equal(123.457M.ToString(CultureInfo.InvariantCulture), collectionElement.Attribute("time").Value);
        }

        [CulturedFact]
        public void AddsPassingTestElementToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            testCase.SourceInformation.Returns(new SourceInformation());
            var test = Mocks.Test(testCase, "Test Display Name");
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(testCase);
            testPassed.Test.Returns(test);
            testPassed.ExecutionTime.Returns(123.4567809M);
            testPassed.Output.Returns("test output");

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testPassed);
            sink.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Pass", testElement.Attribute("result").Value);
            Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time").Value);
            Assert.Equal("test output", testElement.Element("output").Value);
            Assert.Null(testElement.Attribute("source-file"));
            Assert.Null(testElement.Attribute("source-line"));
            Assert.Empty(testElement.Elements("traits"));
            Assert.Empty(testElement.Elements("failure"));
            Assert.Empty(testElement.Elements("reason"));
        }

        [CulturedFact]
        public void EmptyOutputStringDoesNotShowUpInResultingXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            testCase.SourceInformation.Returns(new SourceInformation());
            var test = Mocks.Test(testCase, "Test Display Name");
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(testCase);
            testPassed.Test.Returns(test);
            testPassed.ExecutionTime.Returns(123.4567809M);
            testPassed.Output.Returns(string.Empty);

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testPassed);
            sink.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Pass", testElement.Attribute("result").Value);
            Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time").Value);
            Assert.Null(testElement.Attribute("output"));
            Assert.Null(testElement.Attribute("source-file"));
            Assert.Null(testElement.Attribute("source-line"));
            Assert.Empty(testElement.Elements("traits"));
            Assert.Empty(testElement.Elements("failure"));
            Assert.Empty(testElement.Elements("reason"));
        }

        [CulturedFact]
        public void AddsFailingTestElementToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var test = Mocks.Test(testCase, "Test Display Name");
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestCase.Returns(testCase);
            testFailed.Test.Returns(test);
            testFailed.ExecutionTime.Returns(123.4567809M);
            testFailed.Output.Returns("test output");
            testFailed.ExceptionTypes.Returns(new[] { "Exception Type" });
            testFailed.Messages.Returns(new[] { "Exception Message" });
            testFailed.StackTraces.Returns(new[] { "Exception Stack Trace" });

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testFailed);
            sink.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Fail", testElement.Attribute("result").Value);
            Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time").Value);
            Assert.Equal("test output", testElement.Element("output").Value);
            var failureElement = Assert.Single(testElement.Elements("failure"));
            Assert.Equal("Exception Type", failureElement.Attribute("exception-type").Value);
            Assert.Equal("Exception Type : Exception Message", failureElement.Elements("message").Single().Value);
            Assert.Equal("Exception Stack Trace", failureElement.Elements("stack-trace").Single().Value);
            Assert.Empty(testElement.Elements("reason"));
        }

        [Fact]
        public void NullStackTraceInFailedTestResultsInEmptyStackTraceXmlElement()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestCase.Returns(testCase);
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.Messages.Returns(new[] { "Exception Message" });
            testFailed.StackTraces.Returns(new[] { (string)null });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testFailed);
            sink.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            var failureElement = Assert.Single(testElement.Elements("failure"));
            Assert.Empty(failureElement.Elements("stack-trace").Single().Value);
        }

        [CulturedFact]
        public void AddsSkippedTestElementToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var test = Mocks.Test(testCase, "Test Display Name");
            var testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestCase.Returns(testCase);
            testSkipped.Test.Returns(test);
            testSkipped.ExecutionTime.Returns(0.0M);
            testSkipped.Reason.Returns("Skip Reason");

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testSkipped);
            sink.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Skip", testElement.Attribute("result").Value);
            Assert.Equal(0.0M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time").Value);
            var reasonElement = Assert.Single(testElement.Elements("reason"));
            Assert.Equal("Skip Reason", reasonElement.Value);
            Assert.Empty(testElement.Elements("failure"));
        }

        [Fact]
        public void TestElementSourceInfoIsPlacedInXmlWhenPresent()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            testCase.SourceInformation.Returns(new SourceInformation { FileName = "source file", LineNumber = 42 });
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(testCase);

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testPassed);
            sink.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("source file", testElement.Attribute("source-file").Value);
            Assert.Equal("42", testElement.Attribute("source-line").Value);
        }

        [Fact]
        public void TestElementTraisArePlacedInXmlWhenPresent()
        {
            var traits = new Dictionary<string, List<string>>
                {
                    { "name1", new List<string> { "value1" }},
                    { "name2", new List<string> { "value2" }}
                };
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var passingTestCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            passingTestCase.Traits.Returns(traits);
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(passingTestCase);

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testPassed);
            sink.OnMessage(assemblyFinished);

            var traitsElements = assemblyElement.Elements("collection").Single().Elements("test").Single().Elements("traits").Single().Elements("trait");
            var name1Element = Assert.Single(traitsElements, e => e.Attribute("name").Value == "name1");
            Assert.Equal("value1", name1Element.Attribute("value").Value);
            var name2Element = Assert.Single(traitsElements, e => e.Attribute("name").Value == "name2");
            Assert.Equal("value2", name2Element.Attribute("value").Value);
        }

        public static IEnumerable<object[]> IllegalXmlTestData()
        {
            yield return new object[]
            {
                    new string(Enumerable.Range(0, 32).Select(x => (char)x).ToArray()),
                    @"\0\x01\x02\x03\x04\x05\x06\a\b\t\n\v\f\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"
            };
            // Invalid surrogate characters should be added as \x----, where ---- is the hex value of the char
            yield return new object[]
            {
                    "\xd800 Hello.World \xdc00",
                    @"\xd800 Hello.World \xdc00"
            };
            // ...but valid ones should be outputted like normal
            yield return new object[]
            {
                    "\xd800\xdfff This.Is.Valid \xda00\xdd00",
                    "\xd800\xdfff This.Is.Valid \xda00\xdd00" // note: no @
            };
        }

        [Theory]
        [MemberData(nameof(IllegalXmlTestData))]
        public void IllegalXmlDoesNotPreventXmlFromBeingSaved(string inputName, string outputName)
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var test = Mocks.Test(testCase, inputName);
            var testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestCase.Returns(testCase);
            testSkipped.Test.Returns(test);
            testSkipped.Reason.Returns("Bad\0\r\nString");

            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(testSkipped);
            sink.OnMessage(assemblyFinished);

            using (var writer = new StringWriter())
            {
                assemblyElement.Save(writer, SaveOptions.DisableFormatting);

                var outputXml = writer.ToString();
                Assert.Equal($@"<?xml version=""1.0"" encoding=""utf-16""?><assembly total=""0"" passed=""0"" failed=""0"" skipped=""0"" time=""0.000"" errors=""0""><errors /><collection><test name=""{outputName}"" type=""ExecutionSinkTests+XmlCreation+ClassUnderTest"" method=""TestMethod"" time=""0"" result=""Skip"" source-file=""""><reason><![CDATA[Bad\0\r\nString]]></reason></test></collection></assembly>", outputXml);
            }
        }

        class ClassUnderTest
        {
            [Fact]
            public void TestMethod() { }
        }

        static TMessageType MakeFailureInformationSubstitute<TMessageType>()
            where TMessageType : class, IFailureInformation
        {
            var result = Substitute.For<TMessageType>();
            result.ExceptionTypes.Returns(new[] { "ExceptionType" });
            result.Messages.Returns(new[] { "This is my message \t\r\n" });
            result.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });
            return result;
        }

        public static IEnumerable<object[]> Messages
        {
            get
            {
                yield return new object[] { MakeFailureInformationSubstitute<IErrorMessage>(), "fatal", null };

                var assemblyCleanupFailure = MakeFailureInformationSubstitute<ITestAssemblyCleanupFailure>();
                var testAssembly = Mocks.TestAssembly(@"C:\Foo\bar.dll");
                assemblyCleanupFailure.TestAssembly.Returns(testAssembly);
                yield return new object[] { assemblyCleanupFailure, @"assembly-cleanup", @"C:\Foo\bar.dll" };

                var collectionCleanupFailure = MakeFailureInformationSubstitute<ITestCollectionCleanupFailure>();
                var testCollection = Mocks.TestCollection(displayName: "FooBar");
                collectionCleanupFailure.TestCollection.Returns(testCollection);
                yield return new object[] { collectionCleanupFailure, "test-collection-cleanup", "FooBar" };

                var classCleanupFailure = MakeFailureInformationSubstitute<ITestClassCleanupFailure>();
                var testClass = Mocks.TestClass("MyType");
                classCleanupFailure.TestClass.Returns(testClass);
                yield return new object[] { classCleanupFailure, "test-class-cleanup", "MyType" };

                var methodCleanupFailure = MakeFailureInformationSubstitute<ITestMethodCleanupFailure>();
                var testMethod = Mocks.TestMethod(methodName: "MyMethod");
                methodCleanupFailure.TestMethod.Returns(testMethod);
                yield return new object[] { methodCleanupFailure, "test-method-cleanup", "MyMethod" };

                var testCaseCleanupFailure = MakeFailureInformationSubstitute<ITestCaseCleanupFailure>();
                var testCase = Mocks.TestCase(typeof(object), "ToString", displayName: "MyTestCase");
                testCaseCleanupFailure.TestCase.Returns(testCase);
                yield return new object[] { testCaseCleanupFailure, "test-case-cleanup", "MyTestCase" };

                var testCleanupFailure = MakeFailureInformationSubstitute<ITestCleanupFailure>();
                var test = Mocks.Test(testCase, "MyTest");
                testCleanupFailure.Test.Returns(test);
                yield return new object[] { testCleanupFailure, "test-cleanup", "MyTest" };
            }
        }

        [Theory]
        [MemberData("Messages", DisableDiscoveryEnumeration = true)]
        public void AddsErrorMessagesToXml(IMessageSinkMessage errorMessage, string messageType, string name)
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var assemblyElement = new XElement("assembly");
            var sink = new ExecutionSink(innerSink, new ExecutionSinkOptions { AssemblyElement = assemblyElement });

            sink.OnMessage(errorMessage);
            sink.OnMessage(assemblyFinished);

            var errorElement = Assert.Single(assemblyElement.Element("errors").Elements());
            Assert.Equal(messageType, errorElement.Attribute("type").Value);

            if (name == null)
                Assert.Null(errorElement.Attribute("name"));
            else
                Assert.Equal(name, errorElement.Attribute("name").Value);

            var failureElement = Assert.Single(errorElement.Elements("failure"));
            Assert.Equal("ExceptionType", failureElement.Attribute("exception-type").Value);
            Assert.Equal("ExceptionType : This is my message \\t\\r\\n", failureElement.Elements("message").Single().Value);
            Assert.Equal("Line 1\r\nLine 2\r\nLine 3", failureElement.Elements("stack-trace").Single().Value);
        }
    }
}
