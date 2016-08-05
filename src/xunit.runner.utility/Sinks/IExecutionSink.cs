namespace Xunit
{
    /// <summary>
    /// Represents an <see cref="IMessageSinkWithTypes"/> that can also provide execution
    /// information like an <see cref="IExecutionVisitor"/>.
    /// </summary>
#pragma warning disable CS0618
    public interface IExecutionSink : IExecutionVisitor, IMessageSinkWithTypes { }
#pragma warning restore CS0618
}
