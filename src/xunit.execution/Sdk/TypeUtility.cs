using System;
using System.Linq;
using System.Reflection;
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
        /// Resolves argument values for the test method, including support for optional method
        /// arguments.
        /// </summary>
        /// <param name="testMethod">The test method to resolve.</param>
        /// <param name="arguments">The user-supplied method arguments.</param>
        /// <returns>The argument values</returns>
        public static object[] ResolveMethodArguments(MethodInfo testMethod, object[] arguments)
        {
            var parameters = testMethod.GetParameters();
            if (!parameters.Any(parameter => parameter.IsOptional))
                return arguments;

            // We can't call a method if we have provided more parameters than the total number of parameters in the method,
            // or if we provided fewer parameters than the number of non-optional parameters in the method.
            var nonOptionalParameterCount = parameters.Count(p => !p.IsOptional);
            if (arguments.Length < nonOptionalParameterCount || arguments.Length > parameters.Length)
                return arguments;

            var newArguments = new object[parameters.Length];
            for (int i = 0; i < arguments.Length; i++)
                newArguments[i] = arguments[i];
            for (int i = arguments.Length; i < parameters.Length; i++)
                newArguments[i] = parameters[i].DefaultValue;

            return newArguments;
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

            for (; idx < parameterInfos.Length; idx++)
            {
                var reflectionParameterInfo = parameterInfos[idx] as IReflectionParameterInfo;
                var parameterName = GetParameterName(parameterInfos, idx);
                if (reflectionParameterInfo?.ParameterInfo.IsOptional ?? false)
                    displayValues[idx] = ParameterToDisplayValue(parameterName, reflectionParameterInfo.ParameterInfo.DefaultValue);
                else
                    displayValues[idx] = parameterName + ": ???";
            }

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
        /// Resolves an individual generic type given an intended generic parameter type and the type of an object passed to that type.
        /// </summary>
        /// <param name="genericType">The generic type, e.g. T, to resolve.</param>
        /// <param name="methodParameterType">The non-generic or open generic type, e.g. T, to try to match with the type of the objecct passed to that type.</param>
        /// <param name="passedParameterType">The non-generic or closed generic type, e.g. string, used to resolve the method parameter.</param>
        /// <param name="resultType">The resolved type, e.g. the parameters (T, T, string, typeof(object)) -> (T, T, string, typeof(string)).</param>
        /// <returns>True if resolving was successful, else false.</returns>
        private static bool ResolveGenericParameter(this ITypeInfo genericType, ITypeInfo methodParameterType, Type passedParameterType, ref Type resultType)
        {
            // We can never infer the type parameter from a null type
            if (passedParameterType == null)
            {
                resultType = typeof(object);
                return true;
            }

            // Is a parameter a generic array, e.g. T[] or List<T>[]
            var isGenericArray = false;
            var strippedMethodParameterType = StripElementType(methodParameterType, ref isGenericArray);

            if (isGenericArray)
                passedParameterType = GetArrayElementTypeOrThis(passedParameterType);

            // Is the parameter generic type (e.g. List<T>, Dictionary<T, U>, List<T>[], List<string>)
            if (strippedMethodParameterType.IsGenericType)
            {
                // We recursively drill down both the method parameter and the passed parameter
                // to get and resolve inner generic arguments
                // E.g. (List<T>, List<string>) -> (T, string)
                // E.g. (List<List<T>, List<List<string>>) -> (List<T>, List<string>) -> (T, string)
                var methodParameterGenericArguments = strippedMethodParameterType.GetGenericArguments().CastOrToArray();
                var passedParameterGenericArguments = passedParameterType.GetGenericArguments();

                // We can't pass List<T> to Dictionary<T, U>
                // But we can pass Class : Interface<T> to Interface<T>
                if (methodParameterGenericArguments.Length != passedParameterGenericArguments.Length)
                    if (genericType.ResolveMismatchedGenericArguments(passedParameterType, methodParameterGenericArguments, ref resultType))
                        return true;

                for (int i = 0; i < methodParameterGenericArguments.Length; i++)
                {
                    // Drill down through the generic arguments of the parameter provided,
                    // and find one matching the genericType
                    // This allows us to resolve complex generics with any number of parameters
                    // and at any level deep
                    // e.g. Dictionary<T, U>, Dictionary<string, U>, Dictionary<T, int> etc.
                    // e.g. Dictionary<Dictionary<T, U>, List<Dictionary<V, W>>
                    var methodGenericArgument = methodParameterGenericArguments[i];
                    var passedTypeArgument = passedParameterGenericArguments[i];

                    if (genericType.ResolveGenericParameter(methodGenericArgument, passedTypeArgument, ref resultType))
                        return true;
                }
            }

            // Now finally check if we found a matching type (e.g. T -> T, List<T> -> List<T> etc.)
            if (strippedMethodParameterType.IsGenericParameter && strippedMethodParameterType.Name == genericType.Name)
            {
                if (resultType == null)
                    resultType = passedParameterType;
                else if (resultType.Name != passedParameterType.FullName)
                    return false;
            }

            return resultType != null;
        }

        /// <summary>
        /// Gets the ElementType of a type, only if it is an array.
        /// </summary>
        /// <param name="type">The type to get the ElementType of.</param>
        /// <returns>If type is an array, the ElementType of the type, else the original type.</returns>
        private static Type GetArrayElementTypeOrThis(Type type)
            => type.IsArray ? type.GetElementType() : type;

        /// <summary>
        /// Gets the underlying ElementType of a type, if the ITypeInfo supports reflection.
        /// </summary>
        /// <param name="type">The type to get the ElementType of.</param>
        /// <param name="isArray">A flag indicating whether the type is an array.</param>
        /// <returns>If type has an element type, underlying ElementType of a type, else the original type.</returns>
        private static ITypeInfo StripElementType(ITypeInfo type, ref bool isArray)
        {
            var parameterReflectionType = type as IReflectionTypeInfo;
            if (parameterReflectionType != null && parameterReflectionType.Type.HasElementType)
            {
                // We have a T[] or T&
                isArray = parameterReflectionType.Type.IsArray;
                return Reflector.Wrap(parameterReflectionType.Type.GetElementType());
            }

            return type;
        }

        /// <summary>
        /// Resolves an individual generic type given an intended generic parameter type and the type of an object passed to that type. 
        /// </summary>
        /// <param name="genericType">The generic type, e.g. T, to resolve.</param>
        /// <param name="passedParameterType">The non-generic or closed generic type, e.g. string, used to resolve the method parameter.</param>
        /// <param name="methodGenericTypeArguments">The generic arguments of the open generic type to match with the passed parameter.</param>
        /// <param name="resultType">The resolved type.</param>
        /// <returns>True if resolving was successful, else false.</returns>
        private static bool ResolveMismatchedGenericArguments(this ITypeInfo genericType, Type passedParameterType, ITypeInfo[] methodGenericTypeArguments, ref Type resultType)
        {
            // Do we have Class : BaseClass<T>, Class: BaseClass<T, U> etc.
            var baseType = passedParameterType.GetTypeInfo().BaseType;
            if (baseType != null && baseType.IsGenericType())
            {
                var baseGenericTypeArguments = baseType.GetGenericArguments();

                for (int i = 0; i < baseGenericTypeArguments.Length; i++)
                {
                    var methodGenericTypeArgument = methodGenericTypeArguments[i];
                    var baseGenericTypeArgument = baseGenericTypeArguments[i];

                    if (genericType.ResolveGenericParameter(methodGenericTypeArgument, baseGenericTypeArgument, ref resultType))
                        return true;
                }
            }

            // Do we have Class : Interface<T>, Class : Interface<T, U> etc.
            foreach (var interfaceType in passedParameterType.GetInterfaces().Where(i => i.IsGenericType()))
            {
                var interfaceGenericArguments = interfaceType.GetGenericArguments();
                for (int i = 0; i < interfaceGenericArguments.Length; i++)
                {
                    var methodGenericTypeArgument = methodGenericTypeArguments[i];
                    var baseGenericTypeArgument = interfaceGenericArguments[i];

                    if (genericType.ResolveGenericParameter(methodGenericTypeArgument, baseGenericTypeArgument, ref resultType))
                        return true;
                }
            }

            return false;
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
            for (var idx = 0; idx < parameterInfos.Length; ++idx)
            {
                var methodParameterType = parameterInfos[idx].ParameterType;
                var passedParameterType = parameters[idx]?.GetType();
                Type matchedType = null;

                if (ResolveGenericParameter(genericType, methodParameterType, passedParameterType, ref matchedType))
                    return Reflector.Wrap(matchedType);
            }

            return ObjectTypeInfo;
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
