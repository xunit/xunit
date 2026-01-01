using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

#if !MTP_V1
using System.Threading;
#endif

public class SpyTestPlatformOutputDevice : IOutputDevice
{
	public List<IOutputDeviceData> DisplayedData { get; } = [];

#if MTP_V1
	public Task DisplayAsync(
		IOutputDeviceDataProducer producer,
		IOutputDeviceData data)
#else
	public Task DisplayAsync(
		IOutputDeviceDataProducer producer,
		IOutputDeviceData data,
		CancellationToken cancellationToken)
#endif
	{
		DisplayedData.Add(data);

		return Task.CompletedTask;
	}
}
