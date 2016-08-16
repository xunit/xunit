using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class TestExecutionSinkTests
{
    [Fact]
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
        Assert.NotEmpty(sink.LongRunningTestNotificationMessages.First().TestCases);
        Assert.Same(testCase1, sink.LongRunningTestNotificationMessages.First().TestCases.Keys.First());

        Assert.NotEmpty(sink.DiagnosticMessages);
        Assert.Contains($"[Long Test] '{testCase1.DisplayName}'", sink.DiagnosticMessages.First().Message);
    }

    class TestableTestExecutionSink : TestExecutionSink
    {
        public TestableTestExecutionSink(ConcurrentDictionary<string, ExecutionSummary> completionMessages, Func<bool> cancelThunk, int longRunningSeconds)
            : base(completionMessages, cancelThunk, longRunningSeconds)
        {
            LongRunningTestEvent += OnLongRunningTestEvent;
            DiagnosticMessageEvent += OnDiagnosticMessageEvent;
        }

        void OnDiagnosticMessageEvent(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            DiagnosticMessages.Add(args.Message);
        }

        public readonly ConcurrentBag<ILongRunningTestNotificationMessage> LongRunningTestNotificationMessages = new ConcurrentBag<ILongRunningTestNotificationMessage>();
        public readonly ConcurrentBag<IDiagnosticMessage> DiagnosticMessages = new ConcurrentBag<IDiagnosticMessage>();

        void OnLongRunningTestEvent(MessageHandlerArgs<ILongRunningTestNotificationMessage> args)
        {
            LongRunningTestNotificationMessages.Add(args.Message);
        }


        public bool OnMessage(IMessageSinkMessage message)
        {
            return ((IMessageSink)this).OnMessage(message);
        }
    }
}
