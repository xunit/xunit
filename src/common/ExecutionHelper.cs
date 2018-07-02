using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xunit
{
    static class ExecutionHelper
    {
        static readonly string executionAssemblyNamePrefix = "xunit.execution.";
        static string platformSuffix = "__unknown__";
#if NETSTANDARD1_1
        static readonly string[] platformSuffixes = new[] { "dotnet", "MonoAndroid", "MonoTouch", "iOS-Universal", "universal", "win8", "wp8" };
#elif NETSTANDARD
        static readonly string[] platformSuffixes = new[] { "dotnet", "MonoAndroid", "iOS-Universal" };
#endif

        public static string PlatformSuffix
        {
            get
            {
                lock (executionAssemblyNamePrefix)
                {
                    if (platformSuffix == "__unknown__")
                    {
                        platformSuffix = null;

#if NETSTANDARD
                        foreach (var suffix in platformSuffixes)
                            try
                            {
                                Assembly.Load(new AssemblyName { Name = executionAssemblyNamePrefix + suffix });
                                platformSuffix = suffix;
                                break;
                            }
                            catch { }
#else
                        foreach (var name in AppDomain.CurrentDomain.GetAssemblies().Select(a => a?.GetName()?.Name))
                            if (name != null && name.StartsWith(executionAssemblyNamePrefix, StringComparison.Ordinal))
                            {
                                platformSuffix = name.Substring(executionAssemblyNamePrefix.Length);
                                break;
                            }
#endif
                    }
                }

                if (platformSuffix == null)
                    throw new InvalidOperationException($"Could not find any xunit.execution.* assembly loaded in the current context");

                return platformSuffix;
            }
        }

        /// <summary>
        /// Gets the substitution token used as assembly name suffix to indicate that the assembly is
        /// a generalized reference to the platform-specific assembly.
        /// </summary>
        public static readonly string SubstitutionToken = ".{Platform}";
    }
}
