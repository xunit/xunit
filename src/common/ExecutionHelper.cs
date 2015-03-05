using System;

namespace Xunit
{
    internal static class ExecutionHelper
    {
        /// <summary>
        /// Gets the file name of the execution DLL (with extension) used to run xUnit.net v2 tests.
        /// </summary>
        public static string AssemblyFileName
        {
            get { return String.Format("xunit.execution.{0}.dll", PlatformSpecificAssemblySuffix); }
        }

        /// <summary>
        /// Gets the file name suffix used to construct platform-specific DLL names.
        /// </summary>
#if ANDROID
        public static readonly string PlatformSpecificAssemblySuffix = "MonoAndroid";
#elif __IOS__ && !__UNIFIED__
        public static readonly string PlatformSpecificAssemblySuffix = "MonoTouch";
#elif __IOS__
        public static readonly string PlatformSpecificAssemblySuffix = "iOS-Universal";
#elif WINDOWS_PHONE_APP
        public static readonly string PlatformSpecificAssemblySuffix = "universal";
#elif WINDOWS_PHONE
        public static readonly string PlatformSpecificAssemblySuffix = "wp8";
#elif ASPNET50 || ASPNETCORE50
        public static readonly string PlatformSpecificAssemblySuffix = "AspNet";
#elif NO_APPDOMAIN
        public static readonly string PlatformSpecificAssemblySuffix = "win8";
#else
        public static readonly string PlatformSpecificAssemblySuffix = "desktop";
#endif

        /// <summary>
        /// Gets the substitution token used as assembly name suffix to indicate that the assembly is
        /// a generalized reference to the platform-specific assembly.
        /// </summary>
        public static readonly string SubstitutionToken = ".{Platform}";
    }
}
