using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;

public class SpyTestPlatformMessageBus : IMessageBus
{
	public List<IData> PublishedData { get; } = [];

	public Task PublishAsync(
		IDataProducer dataProducer,
		IData data)
	{
		PublishedData.Add(data);
		return Task.CompletedTask;
	}
}
