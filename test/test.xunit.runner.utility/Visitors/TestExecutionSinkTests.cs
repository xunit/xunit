using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class TestExecutionSinkTests
{
    [Fact]
    [Trait("LongRunning", "true")]
    public async Task TestLongRunningTestBasicBehavior()
    {
        var sink = new TestableTestExecutionSink(null, () => false, 1);

        var testCase1 = Substitute.For<ITestCase>();

        sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
        sink.OnMessage(new TestCaseStarting(testCase1));

        await Task.Delay(3000);


        sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));

        sink.OnMessage(Substitute.For<ITestAssemblyFinished>());


        Assert.NotEmpty(sink.LongRunningTestNotificationMessages);
        Assert.NotEmpty(sink.LongRunningTestNotificationMessages.First()
                            .TestCases);
        Assert.Same(testCase1, sink.LongRunningTestNotificationMessages.First()
                                   .TestCases.Keys.First());

        Assert.NotEmpty(sink.DiagnosticMessages);
        Assert.Contains($"[Long Test] '{testCase1.DisplayName}'", sink.DiagnosticMessages.First()
                                                                      .Message);
    }


    [Fact]
    [Trait("LongRunning", "true")]
    public async Task TestLongRunningTestShouldOnlyHaveLast()
    {
        var sink = new TestableTestExecutionSink(null, () => false, 1);

        var testCase1 = Substitute.For<ITestCase>();
        var testCase2 = Substitute.For<ITestCase>();

        sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
        sink.OnMessage(new TestCaseStarting(testCase1));
        await Task.Delay(250);
        sink.OnMessage(new TestCaseStarting(testCase2));
        sink.OnMessage(new TestCaseFinished(testCase1, 275, 1, 0, 0));

        // finished test will reset timer

        await Task.Delay(750);

        Assert.Empty(sink.LongRunningTestNotificationMessages);

        await Task.Delay(2000);
        Assert.NotEmpty(sink.LongRunningTestNotificationMessages);

        sink.OnMessage(new TestCaseFinished(testCase2, 2000, 1, 0, 0));
        sink.OnMessage(Substitute.For<ITestAssemblyFinished>());


        Assert.NotEmpty(sink.LongRunningTestNotificationMessages);
        Assert.NotEmpty(sink.LongRunningTestNotificationMessages.First()
                            .TestCases);
        Assert.Same(testCase2, sink.LongRunningTestNotificationMessages.First()
                                   .TestCases.Keys.First());
    }

    [Fact]
    [Trait("LongRunning", "true")]
    public async Task TestLongRunningTestShouldHaveNone()
    {
        var sink = new TestableTestExecutionSink(null, () => false, 1);

        var testCase1 = Substitute.For<ITestCase>();
        var testCase2 = Substitute.For<ITestCase>();

        sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
        sink.OnMessage(new TestCaseStarting(testCase1));
        await Task.Delay(250);
        sink.OnMessage(new TestCaseStarting(testCase2));
        sink.OnMessage(new TestCaseFinished(testCase1, 275, 1, 0, 0));

        // finished test will reset timer

        await Task.Delay(750);

        Assert.Empty(sink.LongRunningTestNotificationMessages);

        await Task.Delay(50);
        Assert.Empty(sink.LongRunningTestNotificationMessages);

        sink.OnMessage(new TestCaseFinished(testCase2, 2000, 1, 0, 0));
        sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

        await Task.Delay(2000); 

        Assert.Empty(sink.LongRunningTestNotificationMessages);
    }
}

class TestableTestExecutionSink : TestExecutionSink
{
    public readonly ConcurrentBag<IDiagnosticMessage> DiagnosticMessages = new ConcurrentBag<IDiagnosticMessage>();

    public readonly ConcurrentBag<ILongRunningTestNotificationMessage> LongRunningTestNotificationMessages = new ConcurrentBag<ILongRunningTestNotificationMessage>();

    public TestableTestExecutionSink(ConcurrentDictionary<string, ExecutionSummary> completionMessages, Func<bool> cancelThunk, int longRunningSeconds)
        : base(completionMessages, cancelThunk, longRunningSeconds)
    {
        LongRunningTestEvent += OnLongRunningTestEvent;
        DiagnosticMessageEvent += OnDiagnosticMessageEvent;
    }


    public bool OnMessage(IMessageSinkMessage message)
    {
        return ((IMessageSink)this).OnMessage(message);
    }

    void OnDiagnosticMessageEvent(MessageHandlerArgs<IDiagnosticMessage> args)
    {
        DiagnosticMessages.Add(args.Message);
    }

    void OnLongRunningTestEvent(MessageHandlerArgs<ILongRunningTestNotificationMessage> args)
    {
        LongRunningTestNotificationMessages.Add(args.Message);
    }
}