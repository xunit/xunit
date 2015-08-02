using System;
using System.Globalization;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Extension methods for <see cref="ITypeInfo"/>.
    /// </summary>
    public static class TypeUtility
    {
        readonly static ITypeInfo ObjectTypeInfo = Reflector.Wrap(typeof(object));

        static string ConvertToSimpleTypeName(ITypeInfo type)
        {
            var baseTypeName = type.Name;

            var backTickIdx = baseTypeName.IndexOf('`');
            if (backTickIdx >= 0)
                baseTypeName = baseTypeName.Substring(0, backTickIdx);

            var lastIndex = baseTypeName.LastIndexOf('.');
            if (lastIndex >= 0)
                baseTypeName = baseTypeName.Substring(lastIndex + 1);

            if (!type.IsGenericType)
                return baseTypeName;

            var genericTypes = type.GetGenericArguments().ToArray();
            var simpleNames = new string[genericTypes.Length];

            for (var idx = 0; idx < genericTypes.Length; idx++)
                simpleNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            return $"{baseTypeName}<{string.Join(", ", simpleNames)}>";
        }

        /// <summary>
        /// Formulates the extended portion of the display name for a test method. For tests with no arguments, this will
        /// return just the base name; for tests with arguments, attempts to format the arguments and appends the argument
        /// list to the test name.
        /// </summary>
        /// <param name="method">The test method</param>
        /// <param name="baseDisplayName">The base part of the display name</param>
        /// <param name="arguments">The test method arguments</param>
        /// <param name="genericTypes">The test method's generic types</param>
        /// <returns>The full display name for the test method</returns>
        public static string GetDisplayNameWithArguments(this IMethodInfo method, string baseDisplayName, object[] arguments, ITypeInfo[] genericTypes)
        {
            baseDisplayName += ResolveGenericDisplay(genericTypes);

            if (arguments == null)
                return baseDisplayName;

            var parameterInfos = method.GetParameters().CastOrToArray();
            var displayValues = new string[Math.Max(arguments.Length, parameterInfos.Length)];
            int idx;

            for (idx = 0; idx < arguments.Length; idx++)
                displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), arguments[idx]);

            for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                displayValues[idx] = GetParameterName(parameterInfos, idx) + ": ???";

            return $"{baseDisplayName}({string.Join(", ", displayValues)})";
        }

        static string GetParameterName(IParameterInfo[] parameters, int index)
        {
            if (index >= parameters.Length)
                return "???";

            return parameters[index].Name;
        }

        static string ParameterToDisplayValue(string parameterName, object parameterValue)
            => $"{parameterName}: {ArgumentFormatter.Format(parameterValue)}";

        static string ResolveGenericDisplay(ITypeInfo[] genericTypes)
        {
            if (genericTypes == null || genericTypes.Length == 0)
                return string.Empty;

            var typeNames = new string[genericTypes.Length];
            for (var idx = 0; idx < genericTypes.Length; idx++)
                typeNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            return $"<{string.Join(", ", typeNames)}>";
        }

        /// <summary>
        /// Resolves a generic type for a test method. The test parameters (and associated parameter infos) are
        /// used to determine the best matching generic type for the test method that can be satisfied by all
        /// the generic parameters and their values.
        /// </summary>
        /// <param name="genericType">The generic type to be resolved</param>
        /// <param name="parameters">The parameter values being passed to the test method</param>
        /// <param name="parameterInfos">The parameter infos for the test method</param>
        /// <returns>The best matching generic type</returns>
        public static ITypeInfo ResolveGenericType(this ITypeInfo genericType, object[] parameters, IParameterInfo[] parameterInfos)
        {
            var sawNullValue = false;
            ITypeInfo matchedType = null;

            for (var idx = 0; idx < parameterInfos.Length; ++idx)
            {
                var parameterType = parameterInfos[idx].ParameterType;
                if (parameterType.IsGenericParameter && parameterType.Name == genericType.Name)
                {
                    var parameterValue = parameters[idx];

                    if (parameterValue == null)
                        sawNullValue = true;
                    else if (matchedType == null)
                        matchedType = Reflector.Wrap(parameterValue.GetType());
                    else if (matchedType.Name != parameterValue.GetType().FullName)
                        return ObjectTypeInfo;
                }
            }

            if (matchedType == null)
                return ObjectTypeInfo;

            return sawNullValue && matchedType.IsValueType ? ObjectTypeInfo : matchedType;
        }

        /// <summary>
        /// Resolves all the generic types for a test method. The test parameters are used to determine
        /// the best matching generic types for the test method that can be satisfied by all
        /// the generic parameters and their values.
        /// </summary>
        /// <param name="method">The test method</param>
        /// <param name="parameters">The parameter values being passed to the test method</param>
        /// <returns>The best matching generic types</returns>
        public static ITypeInfo[] ResolveGenericTypes(this IMethodInfo method, object[] parameters)
        {
            var genericTypes = method.GetGenericArguments().ToArray();
            var resolvedTypes = new ITypeInfo[genericTypes.Length];
            var parameterInfos = method.GetParameters().CastOrToArray();

            for (var idx = 0; idx < genericTypes.Length; ++idx)
                resolvedTypes[idx] = ResolveGenericType(genericTypes[idx], parameters, parameterInfos);

            return resolvedTypes;
        }
    }
}
