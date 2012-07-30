using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Extensions
{
    /// <summary>
    /// Represents a single invocation of a data theory test method.
    /// </summary>
    public class TheoryCommand : TestCommand
    {
        /// <summary>
        /// Creates a new instance of <see cref="TheoryCommand"/>.
        /// </summary>
        /// <param name="testMethod">The method under test</param>
        /// <param name="parameters">The parameters to be passed to the test method</param>
        public TheoryCommand(IMethodInfo testMethod, object[] parameters)
            : this(testMethod, parameters, null) { }

        /// <summary>
        /// Creates a new instance of <see cref="TheoryCommand"/> based on a generic theory.
        /// </summary>
        /// <param name="testMethod">The method under test</param>
        /// <param name="parameters">The parameters to be passed to the test method</param>
        /// <param name="genericTypes">The generic types that were used to resolved the generic method.</param>
        public TheoryCommand(IMethodInfo testMethod, object[] parameters, Type[] genericTypes)
            : base(testMethod, MethodUtility.GetDisplayName(testMethod), MethodUtility.GetTimeoutParameter(testMethod))
        {
            Guard.ArgumentNotNull("testMethod", testMethod);

            int idx;

            Parameters = parameters ?? new object[0];

            if (genericTypes != null && genericTypes.Length > 0)
            {
                string[] typeNames = new string[genericTypes.Length];
                for (idx = 0; idx < genericTypes.Length; idx++)
                    typeNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

                DisplayName = String.Format(CultureInfo.CurrentCulture, "{0}<{1}>", DisplayName, string.Join(", ", typeNames));
            }

            ParameterInfo[] parameterInfos = testMethod.MethodInfo.GetParameters();
            string[] displayValues = new string[Math.Max(Parameters.Length, parameterInfos.Length)];

            for (idx = 0; idx < Parameters.Length; idx++)
                displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), Parameters[idx]);

            for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                displayValues[idx] = parameterInfos[idx].Name + ": ???";

            DisplayName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", DisplayName, string.Join(", ", displayValues));
        }

        /// <summary>
        /// Gets the parameter values that are passed to the test method.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This would be a breaking change.")]
        public object[] Parameters { get; protected set; }

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

        /// <inheritdoc/>
        public override MethodResult Execute(object testClass)
        {
            try
            {
                ParameterInfo[] parameterInfos = testMethod.MethodInfo.GetParameters();
                if (parameterInfos.Length != Parameters.Length)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Expected {0} parameters, got {1} parameters", parameterInfos.Length, Parameters.Length));

                testMethod.Invoke(testClass, Parameters);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
            }

            return new PassedResult(testMethod, DisplayName);
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