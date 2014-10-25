using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net Runner Utility (MonoAndroid)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net Runner Utility (MonoTouch)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET 4.5 [No AppDomain])")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net Runner Utility (WPA81 + WIN81)")]
#elif WINDOWS_PHONE
[assembly: AssemblyTitle("xUnit.net Runner Utility (WP8 Silverlight)")]
#elif ASPNET50
[assembly: AssemblyTitle("xUnit.net Runner Utility (ASP.NET)")]
#elif ASPNETCORE50
[assembly: AssemblyTitle("xUnit.net Runner Utility (ASP.NET Core)")]
#else
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET 3.5)")]
#endif

[assembly: CLSCompliant(true)]
