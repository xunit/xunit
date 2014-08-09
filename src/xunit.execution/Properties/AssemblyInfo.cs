using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net (MonoAndroid)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net (MonoTouch)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net (Net45-NoAppdomain)")]
#elif NO_SERIALIZATION
[assembly: AssemblyTitle("xUnit.net (Net45-StringSerialization)")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net (Wpa81+Win81)")]
#else
[assembly: AssemblyTitle("xUnit.net (Net45)")]
#endif
[assembly: CLSCompliant(true)]
