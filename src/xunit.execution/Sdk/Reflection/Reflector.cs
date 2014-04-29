using System;
using System.Collections.Generic;
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
        readonly static object[] EmptyArgs = new object[0];
        readonly static Type[] EmptyTypes = new Type[0];
        readonly static MethodInfo EnumerableCast = typeof(Enumerable).GetMethod("Cast");
        readonly static MethodInfo EnumerableToArray = typeof(Enumerable).GetMethod("ToArray");

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
                for (int idx = 0; idx < args.Length; idx++)
                {
                    var type = types[idx];
                    if (type.IsArray && args[idx] != null && args[idx].GetType() != type)
                    {
                        var elementType = type.GetElementType();
                        var arg = (IEnumerable<object>)args[idx];
                        var castMethod = EnumerableCast.MakeGenericMethod(elementType);
                        var toArrayMethod = EnumerableToArray.MakeGenericMethod(elementType);
                        args[idx] = toArrayMethod.Invoke(null, new object[] { castMethod.Invoke(null, new object[] { arg }) });
                    }
                }

            return args;
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

        /// <summary>
        /// Converts an assembly qualified type name into a <see cref="Type"/> object.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
        /// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
        public static Type GetType(string assemblyQualifiedTypeName)
        {
            var parts = assemblyQualifiedTypeName.Split(new[] { ',' }, 2).Select(x => x.Trim()).ToList();
            if (parts.Count == 0)
                return null;

            if (parts.Count == 1)
                return Type.GetType(parts[0]);

            return GetType(parts[1], parts[0]);
        }

        /// <summary>
        /// Converts an assembly name + type name into a <see cref="Type"/> object.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
        public static Type GetType(string assemblyName, string typeName)
        {
            // Support both long name ("assembly, version=x.x.x.x, etc.") and short name ("assembly")
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);
            if (assembly == null)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch { }
            }

            if (assembly == null)
                return null;

            return assembly.GetType(typeName);
        }
    }
}