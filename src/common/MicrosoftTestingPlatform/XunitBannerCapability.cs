#pragma warning disable TPEXP  // IBannerMessageOwnerCapability is experimental

using System.Runtime.InteropServices;
using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace Xunit.MicrosoftTestingPlatform;

internal sealed class XunitBannerCapability : IBannerMessageOwnerCapability
{
	public Task<string?> GetBannerMessageAsync() =>
		Task.FromResult<string?>(string.Format(
			CultureInfo.CurrentCulture,
			"xUnit.net v3 Microsoft.Testing.Platform {0} Runner v{1} ({2}-bit {3}){4}",
#if MTP_V1
			"v1",
#else
			"v2",
#endif
			ThisAssembly.AssemblyInformationalVersion,
			IntPtr.Size * 8,
			RuntimeInformation.FrameworkDescription,
			Environment.NewLine
		));
}
