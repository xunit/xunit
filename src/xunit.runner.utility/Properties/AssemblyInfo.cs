using System;
using System.Reflection;

#if ANDROID
[assembly: AssemblyTitle("xUnit.net Runner Utility (MonoAndroid)")]
#elif __IOS__
[assembly: AssemblyTitle("xUnit.net Runner Utility (MonoTouch)")]
#elif NO_APPDOMAIN
[assembly: AssemblyTitle("xUnit.net Runner Utility (Net45-NoAppdomain)")]
#elif WINDOWS_PHONE_APP
[assembly: AssemblyTitle("xUnit.net Runner Utility (Wpa81+Win81)")]
#else
[assembly: AssemblyTitle("xUnit.net Runner Utility (Net35)")]
#endif
[assembly: CLSCompliant(true)]