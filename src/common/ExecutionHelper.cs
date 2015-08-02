using System;

namespace Xunit
{
    static class ExecutionHelper
    {
        /// <summary>
        /// Gets the file name of the execution DLL (with extension) used to run xUnit.net v2 tests.
        /// </summary>
        public static string AssemblyFileName
            => $"xunit.execution.{PlatformSpecificAssemblySuffix}.dll";

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
#elif DOTNETCORE
        public static readonly string PlatformSpecificAssemblySuffix = "DotNetCore";
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
