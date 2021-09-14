using System.Collections;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Web.UI;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ExecutorCallbackTests
    {
        public class Notify
        {
            [Fact]
            public void IMessageSink()
            {
                IMessage msg = null;
                var handler = new Mock<IMessageSink>();
                handler.Setup(h => h.SyncProcessMessage(It.IsAny<IMessage>()))
                       .Callback<IMessage>(_msg => msg = _msg)
                       .Returns((IMessage)null);
                var callback = ExecutorCallback.Wrap(handler.Object);

                callback.Notify("This is the callback value");

                Assert.Equal("This is the callback value", msg.Properties["data"]);
            }

            [Fact]
            public void ICallbackEventHandler()
            {
                var handler = new Mock<ICallbackEventHandler>();
                var callback = ExecutorCallback.Wrap(handler.Object);

                callback.Notify("This is the callback value");

                handler.Verify(h => h.RaiseCallbackEvent("This is the callback value"), Times.Once());
            }

            [Fact]
            public void NullCallbackDoesNotThrow()
            {
                var callback = ExecutorCallback.Wrap(null);

                Assert.DoesNotThrow(() => callback.Notify("This is the callback value"));
            }
        }

        public class ShouldContinue
        {
            [Fact]
            public void IMessageSinkReturnsTrueByDefault()
            {
                var handler = new Mock<IMessageSink>();
                var callback = ExecutorCallback.Wrap(handler.Object);
                // Don't call Notify here

                var result = callback.ShouldContinue();

                Assert.True(result);
            }

            [Fact]
            public void IMessageSinkReturningTrue()
            {
                var values = new Hashtable { { "data", "true" } };
                var message = new Mock<IMessage>();
                message.Setup(m => m.Properties).Returns(values);
                var handler = new Mock<IMessageSink>();
                handler.Setup(h => h.SyncProcessMessage(It.IsAny<IMessage>()))
                       .Returns(message.Object);
                var callback = ExecutorCallback.Wrap(handler.Object);
                // Have to call Notify() because that's how we discover the intended ShouldContinue value
                callback.Notify(null);

                var result = callback.ShouldContinue();

                Assert.True(result);
            }

            [Fact]
            public void IMessageSinkReturningFalse()
            {
                var values = new Hashtable { { "data", "false" } };
                var message = new Mock<IMessage>();
                message.Setup(m => m.Properties).Returns(values);
                var handler = new Mock<IMessageSink>();
                handler.Setup(h => h.SyncProcessMessage(It.IsAny<IMessage>()))
                       .Returns(message.Object);
                var callback = ExecutorCallback.Wrap(handler.Object);
                // Have to call Notify() because that's how we discover the intended ShouldContinue value
                callback.Notify(null);

                var result = callback.ShouldContinue();

                Assert.False(result);
            }

            [Fact]
            public void ICallbackEventHandlerReturningTrue()
            {
                var handler = new Mock<ICallbackEventHandler>();
                handler.Setup(h => h.GetCallbackResult()).Returns("true");
                var callback = ExecutorCallback.Wrap(handler.Object);

                var result = callback.ShouldContinue();

                Assert.True(result);
            }

            [Fact]
            public void ICallbackEventHandlerReturningFalse()
            {
                var handler = new Mock<ICallbackEventHandler>();
                handler.Setup(h => h.GetCallbackResult()).Returns("false");
                var callback = ExecutorCallback.Wrap(handler.Object);

                var result = callback.ShouldContinue();

                Assert.False(result);
            }

            [Fact]
            public void ICallbackEventHandlerReturningNull()
            {
                var handler = new Mock<ICallbackEventHandler>();
                var callback = ExecutorCallback.Wrap(handler.Object);

                var result = callback.ShouldContinue();

                Assert.True(result);
            }

            [Fact]
            public void NullCallbackAlwaysReturnsTrue()
            {
                var callback = ExecutorCallback.Wrap(null);

                var result = callback.ShouldContinue();

                Assert.True(result);
            }
        }

        public class Wrap
        {
            [Fact]
            public void UnsupportedCallbackTypeThrows()
            {
                ExecutorCallback wrapper = ExecutorCallback.Wrap(42);

                var ex = Record.Exception(() => wrapper.Notify("Notification"));

                Assert.IsType<TargetException>(ex);
            }
        }
    }
}
