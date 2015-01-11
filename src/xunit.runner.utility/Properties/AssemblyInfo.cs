using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net Runner Utility (MonoAndroid)")]
#elif __IOS__ && ! __UNIFIED__
[assembly: AssemblyTitle("xUnit.net Runner Utility (MonoTouch)")]
#elif __IOS__ 
[assembly: AssemblyTitle("xUnit.net Runner Utility (iOS Universal)")]
#elif ASPNET50
[assembly: AssemblyTitle("xUnit.net Runner Utility (ASP.NET)")]
#elif ASPNETCORE50
[assembly: AssemblyTitle("xUnit.net Runner Utility (ASP.NET Core)")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net Runner Utility (Universal [WPA81, WIN81])")]
#elif WINDOWS_PHONE
[assembly: AssemblyTitle("xUnit.net Runner Utility (Windows Phone 8 Silverlight)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net Runner Utility (Windows 8)")]
#else
[assembly: AssemblyTitle("xUnit.net Runner Utility (Desktop)")]
#endif

[assembly: CLSCompliant(true)]
