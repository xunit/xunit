using Xunit.Sdk;
using Xunit.v3;

public sealed class SpyMessageBus(Func<IMessageSinkMessage, bool>? cancellationThunk = null) :
	IMessageBus
{
	readonly Func<IMessageSinkMessage, bool> cancellationThunk = cancellationThunk ?? (msg => true);
	public List<IMessageSinkMessage> Messages = [];

	public void Dispose()
	{ }

	public bool QueueMessage(IMessageSinkMessage message)
	{
		Messages.Add(message);
		return cancellationThunk(message);
	}
}
