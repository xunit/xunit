using System.Reflection;

#if NET472
[assembly: AssemblyTitle("xUnit.net Runner Reporters (.NET 4.7.2)")]
#elif NETSTANDARD2_0
[assembly: AssemblyTitle("xUnit.net Runner Reporters (.NET Standard 2.0)")]
#else
#error Unknown target platform
#endif
