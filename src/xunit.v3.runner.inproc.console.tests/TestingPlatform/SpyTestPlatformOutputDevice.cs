using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

public class SpyTestPlatformOutputDevice :
	IOutputDevice
{
	public List<IOutputDeviceData> DisplayedData { get; } = [];

	public Task DisplayAsync(
		IOutputDeviceDataProducer producer,
		IOutputDeviceData data,
		CancellationToken cancellationToken)
	{
		DisplayedData.Add(data);

		return Task.CompletedTask;
	}
}
