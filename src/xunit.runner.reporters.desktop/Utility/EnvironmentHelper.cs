using System;

namespace Xunit
{
    static class EnvironmentHelper
    {
        public static string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}
