using System;

namespace Xunit
{
    static class EnvironmentHelper
    {
        public static string GetEnvironmentVariable(string variable)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            Console.WriteLine($"{variable} = {value}");
            return value;
        }
    }
}
