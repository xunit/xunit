namespace Xunit
{
    internal static class ExecutionHelper
    {
        /// <summary>
        /// Gets the name of the execution DLL used to run xUnit.net v2 tests.
        /// </summary>
#if ANDROID
        public static readonly string AssemblyName = "xunit.execution.MonoAndroid";
#elif __IOS__ && ! __UNIFIED__
        public static readonly string AssemblyName = "xunit.execution.MonoTouch";
#elif __IOS__
        public static readonly string AssemblyName = "xunit.execution.iOS-Universal";
#elif WINDOWS_PHONE_APP
        public static readonly string AssemblyName = "xunit.execution.universal";
#elif WINDOWS_PHONE
        public static readonly string AssemblyName = "xunit.execution.wp8";
#elif ASPNET50 || ASPNETCORE50
        public static readonly string AssemblyName = "xunit.execution.AspNet";
#elif NO_APPDOMAIN
        public static readonly string AssemblyName = "xunit.execution.win8";
#else
        public static readonly string AssemblyName = "xunit.execution.desktop";
#endif

        public static readonly string AssemblyFileName = AssemblyName + ".dll";
    }
}