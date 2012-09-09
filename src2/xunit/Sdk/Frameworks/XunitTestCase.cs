using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestCase : IMethodTestCase
    {
        public XunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IEnumerable<object> arguments = null)
        {
            Arguments = arguments ?? Enumerable.Empty<object>();
            Assembly = assembly;
            Class = type;
            Method = method;
            DisplayName = type.Name + "." + method.Name;

            if (arguments != null)
            {
                var Parameters = arguments.ToArray();

                IReflectionMethodInfo testMethod = (IReflectionMethodInfo)method;
                ParameterInfo[] parameterInfos = testMethod.MethodInfo.GetParameters();
                string[] displayValues = new string[Math.Max(Parameters.Length, parameterInfos.Length)];
                int idx;

                for (idx = 0; idx < Parameters.Length; idx++)
                    displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), Parameters[idx]);

                for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                    displayValues[idx] = parameterInfos[idx].Name + ": ???";

                DisplayName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", DisplayName, string.Join(", ", displayValues));
            }
        }

        public IEnumerable<object> Arguments { get; private set; }

        public IAssemblyInfo Assembly { get; private set; }

        public ITypeInfo Class { get; private set; }

        public string DisplayName { get; private set; }

        public IMethodInfo Method { get; private set; }

        public ITestCollection TestCollection { get; private set; }

        public IDictionary<string, string> Traits { get; private set; }

        static string ConvertToSimpleTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            Type[] genericTypes = type.GetGenericArguments();
            string[] simpleNames = new string[genericTypes.Length];

            for (int idx = 0; idx < genericTypes.Length; idx++)
                simpleNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            string baseTypeName = type.Name;
            int backTickIdx = type.Name.IndexOf('`');

            return baseTypeName.Substring(0, backTickIdx) + "<" + String.Join(", ", simpleNames) + ">";
        }

        static string GetParameterName(ParameterInfo[] parameters, int index)
        {
            if (index >= parameters.Length)
                return "???";

            return parameters[index].Name;
        }

        static string ParameterToDisplayValue(object parameterValue)
        {
            if (parameterValue == null)
                return "null";

            if (parameterValue is char)
                return "'" + parameterValue + "'";

            string stringParameter = parameterValue as string;
            if (stringParameter != null)
            {
                if (stringParameter.Length > 50)
                    return "\"" + stringParameter.Substring(0, 50) + "\"...";

                return "\"" + stringParameter + "\"";
            }

            return Convert.ToString(parameterValue, CultureInfo.CurrentCulture);
        }

        static string ParameterToDisplayValue(string parameterName, object parameterValue)
        {
            return parameterName + ": " + ParameterToDisplayValue(parameterValue);
        }
    }
}
