namespace Xunit.Runner.VisualStudio
{
    public static class Constants
    {
#if NET452 || WINDOWS_UAP
        public const string ExecutorUri = "executor://xunit/VsTestRunner2/desktop_and_uap";
#elif NETCOREAPP1_0
        public const string ExecutorUri = "executor://xunit/VsTestRunner2/netcoreapp";
#else
#error Unknown target platform
#endif
    }
}
