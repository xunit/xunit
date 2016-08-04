using Xunit.Abstractions;

namespace Xunit
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class MessageHandlerArgs<TMessage> where TMessage : class, IMessageSinkMessage
    {
        /// <summary />
        public MessageHandlerArgs(TMessage message)
        {
            Message = message;
        }

        /// <summary />
        public TMessage Message { get; }

        /// <summary />
        public bool IsStopped { get; private set; }

        /// <summary />
        public void Stop()
        {
            IsStopped = true;
        }
    }
}
