#pragma warning disable TPEXP  // IBannerMessageOwnerCapability is experimental

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

internal sealed class XunitBannerCapability : IBannerMessageOwnerCapability
{
	public Task<string?> GetBannerMessageAsync() =>
		Task.FromResult<string?>(string.Format(
			CultureInfo.CurrentCulture,
			"xUnit.net v3 Microsoft.Testing.Platform Runner v{0} ({1}-bit {2}){3}",
			ThisAssembly.AssemblyInformationalVersion,
			IntPtr.Size * 8,
			RuntimeInformation.FrameworkDescription,
			Environment.NewLine
		));
}
