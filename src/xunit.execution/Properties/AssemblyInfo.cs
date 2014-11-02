using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net (MonoAndroid)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net (MonoTouch)")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net (WPA81 + WIN81)")]
#elif WINDOWS_PHONE
[assembly: AssemblyTitle("xUnit.net (WP8 Silverlight)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net (.NET 4.5 [No AppDomain])")]
#elif NO_SERIALIZATION
[assembly: AssemblyTitle("xUnit.net (.NET 4.5 [String Serialization])")]
#else
[assembly: AssemblyTitle("xUnit.net (.NET 4.5)")]
#endif

[assembly: CLSCompliant(true)]
