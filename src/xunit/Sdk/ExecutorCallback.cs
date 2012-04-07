using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class supports the xUnit.net infrastructure and is not intended to be used
    /// directly from your code.
    /// </summary>
    public abstract class ExecutorCallback
    {
        static Type typeICallbackEventHandler = Type.GetType("System.Web.UI.ICallbackEventHandler, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false);

        /// <summary>
        /// This API supports the xUnit.net infrastructure and is not intended to be used
        /// directly from your code.
        /// </summary>
        public static ExecutorCallback Wrap(object handler)
        {
            if (handler == null)
                return new NullCallback();

            IMessageSink messageSink = handler as IMessageSink;
            if (messageSink != null)
                return new MessageSinkCallback(messageSink);

            return new CallbackEventHandlerCallback(handler);
        }

        /// <summary>
        /// This API supports the xUnit.net infrastructure and is not intended to be used
        /// directly from your code.
        /// </summary>
        public abstract void Notify(string value);

        /// <summary>
        /// This API supports the xUnit.net infrastructure and is not intended to be used
        /// directly from your code.
        /// </summary>
        public abstract bool ShouldContinue();

        // Used when the user wants a callback through a Predicate<string>.

        class MessageSinkCallback : ExecutorCallback
        {
            bool shouldContinue = true;
            IMessageSink messageSink;

            public MessageSinkCallback(IMessageSink messageSink)
            {
                this.messageSink = messageSink;
            }

            public override void Notify(string value)
            {
                OutgoingMessage message = new OutgoingMessage(value);
                IMessage response = messageSink.SyncProcessMessage(message);
                if (response != null && response.Properties.Contains("data"))
                    shouldContinue = Convert.ToBoolean(response.Properties["data"]);
            }

            public override bool ShouldContinue()
            {
                return shouldContinue;
            }

            class OutgoingMessage : MarshalByRefObject, IMessage
            {
                Hashtable values = new Hashtable();

                public OutgoingMessage(string value)
                {
                    values["data"] = value;
                }

                IDictionary IMessage.Properties
                {
                    get { return values; }
                }
            }
        }

        // Used when the user wants a callback through the older callback
        // mechanism (System.Web.UI.ICallbackEventHandler). Since we don't want
        // an explicit reference to System.Web.dll, we do this invocation via
        // reflection, and throw when the user only has the client profile.

        class CallbackEventHandlerCallback : ExecutorCallback
        {
            object handler;
            static MethodInfo raiseCallbackEvent;
            static MethodInfo getCallbackResult;

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "No can do.")]
            static CallbackEventHandlerCallback()
            {
                raiseCallbackEvent = typeICallbackEventHandler.GetMethod("RaiseCallbackEvent");
                getCallbackResult = typeICallbackEventHandler.GetMethod("GetCallbackResult");
            }

            public CallbackEventHandlerCallback(object handler)
            {
                this.handler = handler;
            }

            public override void Notify(string value)
            {
                raiseCallbackEvent.Invoke(handler, new object[] { value });
            }

            public override bool ShouldContinue()
            {
                var result = getCallbackResult.Invoke(handler, new object[0]);
                return result == null ? true : Boolean.Parse(result as String);
            }
        }

        // Used when the user does not want a callback

        class NullCallback : ExecutorCallback
        {
            public override void Notify(string value) { }

            public override bool ShouldContinue()
            {
                return true;
            }
        }
    }
}
