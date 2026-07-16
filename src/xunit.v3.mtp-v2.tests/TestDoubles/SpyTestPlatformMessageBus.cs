using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;
using Xunit;

public class SpyTestPlatformMessageBus(bool validateSessionUid = false) :
	IMessageBus
{
	public List<IData> PublishedData { get; } = [];

	public SessionUid? SessionUid { get; private set; }

	public void Clear()
	{
		PublishedData.Clear();
		SessionUid = null;
	}

	public Task PublishAsync(
		IDataProducer dataProducer,
		IData data)
	{
		if (validateSessionUid)
		{
			var currentSessionUid = TestContext.Current.MicrosoftTestingPlatformSession();
			Assert.NotNull(currentSessionUid);

			if (SessionUid is null)
				SessionUid = currentSessionUid;
			else
				Assert.Equal(SessionUid.Value.Value, currentSessionUid.Value.Value);
		}

		PublishedData.Add(data);
		return Task.CompletedTask;
	}
}
