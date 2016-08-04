using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate void MessageHandler<TMessage>(MessageHandlerArgs<TMessage> args) where TMessage : class, IMessageSinkMessage;
}