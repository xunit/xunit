using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net (MonoAndroid)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net (MonoTouch)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net (.NET 4.5 [No AppDomain])")]
#elif NO_SERIALIZATION
[assembly: AssemblyTitle("xUnit.net (.NET 4.5 [String Serialization])")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net (WPA81 + WIN81)")]
#else
[assembly: AssemblyTitle("xUnit.net (.NET 4.5)")]
#endif

[assembly: CLSCompliant(true)]
