using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Wrapper to implement types from xunit.abstractions.dll using reflection.
    /// </summary>
    public static class Reflector
    {
        internal readonly static object[] EmptyArgs = new object[0];
        internal readonly static Type[] EmptyTypes = new Type[0];

        readonly static MethodInfo EnumerableCast = typeof(Enumerable).GetRuntimeMethods().First(m => m.Name == "Cast");
        readonly static MethodInfo EnumerableToArray = typeof(Enumerable).GetRuntimeMethods().First(m => m.Name == "ToArray");

        /// <summary>
        /// Converts arguments into their target types. Can be particularly useful when pulling attribute
        /// constructor arguments, whose types may not strictly match the parameter types.
        /// </summary>
        /// <param name="args">The arguments to be converted.</param>
        /// <param name="types">The target types for the conversion.</param>
        /// <returns>The converted arguments.</returns>
        public static object[] ConvertArguments(object[] args, Type[] types)
        {
            if (args == null)
                args = EmptyArgs;
            if (types == null)
                types = EmptyTypes;

            if (args.Length == types.Length)
            {
                for (var idx = 0; idx < args.Length; idx++)
                {
                    args[idx] = ConvertArgument(args[idx], types[idx]);
                }
            }

            return args;
        }

        internal static object ConvertArgument(object arg, Type type)
        {
            if (arg != null && !type.IsAssignableFrom(arg.GetType()))
            {
                try
                {
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType();
                        var enumerable = (IEnumerable<object>)arg;
                        var castMethod = EnumerableCast.MakeGenericMethod(elementType);
                        var toArrayMethod = EnumerableToArray.MakeGenericMethod(elementType);
                        return toArrayMethod.Invoke(null, new object[] { castMethod.Invoke(null, new object[] { enumerable }) });
                    }
                    else
                    {
                        if (type == typeof(Guid))
                        {
                            return Guid.Parse(arg.ToString());
                        }

                        if (type == typeof(DateTime))
                        {
                            return DateTime.Parse(arg.ToString(), CultureInfo.InvariantCulture);
                        }

                        if (type == typeof(DateTimeOffset))
                        {
                            return DateTimeOffset.Parse(arg.ToString(), CultureInfo.InvariantCulture);
                        }

                        return Convert.ChangeType(arg, type);
                    }
                }
                catch { } // Eat conversion-related exceptions; they'll get re-surfaced during execution
            }
            return arg;
        }

        /// <summary>
        /// Converts an <see cref="Assembly"/> into an <see cref="IReflectionAssemblyInfo"/>.
        /// </summary>
        /// <param name="assembly">The assembly to wrap.</param>
        /// <returns>The wrapper</returns>
        public static IReflectionAssemblyInfo Wrap(Assembly assembly)
        {
            return new ReflectionAssemblyInfo(assembly);
        }

        /// <summary>
        /// Converts an <see cref="Attribute"/> into an <see cref="IAttributeInfo"/> using reflection.
        /// </summary>
        /// <param name="attribute">The attribute to wrap.</param>
        /// <returns>The wrapper</returns>
        public static IReflectionAttributeInfo Wrap(CustomAttributeData attribute)
        {
            return new ReflectionAttributeInfo(attribute);
        }

        /// <summary>
        /// Converts a <see cref="MethodInfo"/> into an <see cref="IMethodInfo"/> using reflection.
        /// </summary>
        /// <param name="method">The method to wrap</param>
        /// <returns>The wrapper</returns>
        public static IReflectionMethodInfo Wrap(MethodInfo method)
        {
            return new ReflectionMethodInfo(method);
        }

        /// <summary>
        /// Converts a <see cref="ParameterInfo"/> into an <see cref="IParameterInfo"/> using reflection.
        /// </summary>
        /// <param name="parameter">THe parameter to wrap</param>
        /// <returns>The wrapper</returns>
        public static IReflectionParameterInfo Wrap(ParameterInfo parameter)
        {
            return new ReflectionParameterInfo(parameter);
        }

        /// <summary>
        /// Converts a <see cref="Type"/> into an <see cref="ITypeInfo"/> using reflection.
        /// </summary>
        /// <param name="type">The type to wrap</param>
        /// <returns>The wrapper</returns>
        public static IReflectionTypeInfo Wrap(Type type)
        {
            return new ReflectionTypeInfo(type);
        }
    }
}
