using System.Reflection;

#if NET452
[assembly: AssemblyTitle("xUnit.net Runner Reporters (.NET 4.5.2)")]
#elif NETSTANDARD1_1
[assembly: AssemblyTitle("xUnit.net Runner Reporters (.NET Standard 1.1)")]
#elif NETSTANDARD1_5
[assembly: AssemblyTitle("xUnit.net Runner Reporters (.NET Standard 1.5)")]
#else
#error Unknown target platform
#endif
