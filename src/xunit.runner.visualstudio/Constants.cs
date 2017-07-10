namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public static class Constants
    {
#if NET452
        public const string ExecutorUri = "executor://xunit/VsTestRunner2/desktop";
#elif NETCOREAPP1_0
        public const string ExecutorUri = "executor://xunit/VsTestRunner2/netcoreapp";
#elif WINDOWS_UAP
        public const string ExecutorUri = "executor://xunit/VsTestRunner2/uap";
#else
#error Unknown target platform
#endif
    }
}
