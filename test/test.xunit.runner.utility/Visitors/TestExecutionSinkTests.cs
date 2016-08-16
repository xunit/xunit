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
    public async Task TestLongRunningTest()
    {
        var sink = new TestableTestExecutionSink(null, () => false, 2);
        
        var testCase1 = Substitute.For<ITestCase>();

        var testCase2 = Substitute.For<ITestCase>();

        sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
        sink.OnMessage(new TestCaseStarting(testCase1));

        await Task.Delay(6000);


        sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));

        sink.OnMessage(Substitute.For<ITestAssemblyFinished>());


        Assert.NotNull(sink.LongRunningTestNotificationMessage);
        Assert.NotEmpty(sink.LongRunningTestNotificationMessage.TestCases);
        Assert.Same(testCase1, sink.LongRunningTestNotificationMessage.TestCases.Keys.First());
    }

    class TestableTestExecutionSink : TestExecutionSink
    {
        public TestableTestExecutionSink(ConcurrentDictionary<string, ExecutionSummary> completionMessages, Func<bool> cancelThunk, int longRunningSeconds)
            : base(completionMessages, cancelThunk, longRunningSeconds)
        {
            LongRunningTestEvent += OnLongRunningTestEvent;
        }

        void OnLongRunningTestEvent(MessageHandlerArgs<ILongRunningTestNotificationMessage> args)
        {
            LongRunningTestNotificationMessage = args.Message;
        }


        public bool OnMessage(IMessageSinkMessage message)
        {
            return ((IMessageSink)this).OnMessage(message);
        }

        public ILongRunningTestNotificationMessage LongRunningTestNotificationMessage { get; private set; }
    }
}
