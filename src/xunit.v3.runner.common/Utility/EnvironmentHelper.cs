using System;
using System.Reflection;

namespace Xunit
{
    static class EnvironmentHelper
    {
#if NETFRAMEWORK
        public static string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
#else
        static readonly Lazy<MethodInfo> getEnvironmentVariableMethod = new Lazy<MethodInfo>(GetEnvironmentVariableMethod);

        public static string GetEnvironmentVariable(string variable)
        {
            return (string)getEnvironmentVariableMethod.Value.Invoke(null, new object[] { variable });
        }

        static MethodInfo GetEnvironmentVariableMethod()
        {
            var result = typeof(Environment).GetRuntimeMethod("GetEnvironmentVariable", new[] { typeof(string) });
            if (result == null)
                throw new InvalidOperationException("Could not find method: System.Environment.GetEnvironmentVariable");

            return result;
        }
#endif
    }
}
