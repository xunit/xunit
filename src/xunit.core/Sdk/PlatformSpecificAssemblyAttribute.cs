using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// Marks an assembly as a platform specific assembly for use with xUnit.net. Type references from
    /// such assemblies are allowed to use a special suffix ("My.Assembly.{Platform}"), which will
    /// automatically be translated into the correct platform-specific name ("My.Assembly.desktop",
    /// "My.Assembly.win8", etc.). This affects both extensibility points which require specifying
    /// a string-based type name and assembly, as well as serialization. The supported platform target
    /// names include:
    ///   "desktop" (for desktop and PCL tests),
    ///   "DotNetCore" (for Windows Universal App tests and DNX Core tests),
    ///   "iOS-Universal" (for Xamarin test projects targeting iOS),
    ///   "MonoAndroid" (for Xamarin MonoAndroid tests),
    ///   "MonoTouch" (for Xamarin MonoTouch tests),
    ///   "universal" (for Windows Phone 8.1 and Windows 8.1 tests),
    ///   "wp8" (for Windows Phone 8 Silverlight tests).
    /// Note that file names may be case sensitive (when running on platforms with case sensitive
    /// file systems like Linux), so ensure that your assembly file name casing is consistent, and
    /// that you use the suffixes here with the exact case shown.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class PlatformSpecificAssemblyAttribute : Attribute { }
}
