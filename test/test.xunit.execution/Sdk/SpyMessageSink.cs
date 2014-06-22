using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public static class SpyMessageSink
    {
        public static IMessageSink Create(bool returnResult = true, List<IMessageSinkMessage> messages = null)
        {
            return Create(_ => returnResult, messages);
        }

        public static IMessageSink Create(Func<IMessageSinkMessage, bool> lambda, List<IMessageSinkMessage> messages = null)
        {
            var result = Substitute.For<IMessageSink>();

            result.OnMessage(null).ReturnsForAnyArgs(
                callInfo =>
                {
                    var message = callInfo.Arg<IMessageSinkMessage>();

                    if (messages != null)
                        messages.Add(message);

                    return lambda(message);
                });

            return result;
        }
    }
}
