using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public static class SpyMessageSink
    {
        public static IMessageSink Create(List<IMessageSinkMessage> messages = null, bool returnResult = true)
        {
            var result = Substitute.For<IMessageSink>();

            result.OnMessage(null).ReturnsForAnyArgs(
                callInfo =>
                {
                    if (messages != null)
                        messages.Add((IMessageSinkMessage)callInfo[0]);

                    return returnResult;
                });

            return result;
        }
    }
}
