using System;
using System.Reflection;
using Xunit.Sdk;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net Execution (MonoAndroid)")]
#elif __IOS__ && ! __UNIFIED__
[assembly: AssemblyTitle("xUnit.net Execution (MonoTouch)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net Execution (iOS Universal)")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net Execution (Universal [WPA81, WIN81])")]
#elif WINDOWS_PHONE
[assembly: AssemblyTitle("xUnit.net Execution (Windows Phone 8 Silverlight)")]
#elif DOTNETCORE
[assembly: AssemblyTitle("xUnit.net Execution (.NET Core)")]
#else
[assembly: AssemblyTitle("xUnit.net Execution (Desktop)")]
#endif

[assembly: CLSCompliant(true)]
[assembly: PlatformSpecificAssembly]
