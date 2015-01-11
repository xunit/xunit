using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net Execution (MonoAndroid)")]
#elif __IOS__ && ! __UNIFIED__
[assembly: AssemblyTitle("xUnit.net Execution (MonoTouch)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net Execution (iOS Universal)")]
#elif ASPNET50 || ASPNETCORE50
[assembly: AssemblyTitle("xUnit.net Execution (ASP.NET)")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net Execution (Universal [WPA81, WIN81])")]
#elif WINDOWS_PHONE
[assembly: AssemblyTitle("xUnit.net Execution (Windows Phone 8 Silverlight)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net Execution (Windows 8)")]
#elif NO_SERIALIZATION
[assembly: AssemblyTitle("xUnit.net Execution (Desktop [String Serialization])")]
#else
[assembly: AssemblyTitle("xUnit.net Execution (Desktop)")]
#endif

[assembly: CLSCompliant(true)]
