using System.Reflection;
using System.Runtime.InteropServices;
using Xunit.v3;

namespace ExtensionProject;

/*
The intended usage of this sample attribute is as an extra attribute on a unit test method. For example:

    public class TestClass
    {
        [Fact]
        [SupportedOS(SupportedOS.Linux, SupportedOS.macOS)]
        public void TestMethod()
        {
        }
    }

TestMethod will only run when executed on Linux or macOS; it will not run on Windows or FreeBSD, and will be
dynamically skipped instead with a message about the current OS not being supported.
*/

public sealed class SupportedOSAttribute(params SupportedOS[] supportedOSes) :
    BeforeAfterTestAttribute
{
    private static readonly Dictionary<SupportedOS, OSPlatform> osMappings = new()
    {
        { SupportedOS.FreeBSD, OSPlatform.Create("FreeBSD") },
        { SupportedOS.Linux, OSPlatform.Linux },
        { SupportedOS.macOS, OSPlatform.OSX },
        { SupportedOS.Windows, OSPlatform.Windows },
    };

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var match = false;

        foreach (var supportedOS in supportedOSes)
        {
            if (!osMappings.TryGetValue(supportedOS, out var osPlatform))
                throw new ArgumentException($"Supported OS value '{supportedOS}' is not a known OS", nameof(supportedOSes));

            if (RuntimeInformation.IsOSPlatform(osPlatform))
            {
                match = true;
                break;
            }
        }

        // We use the dynamic skip exception message pattern to turn this into a skipped test
        // when it's not running on one of the targeted OSes
        if (!match)
            throw new Exception($"$XunitDynamicSkip$This test is not supported on {RuntimeInformation.OSDescription}");
    }
}
