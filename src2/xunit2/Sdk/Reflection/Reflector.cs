using System;
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

        internal static Type GetType(string assemblyQualifiedAttributeTypeName)
        {
            var parts = assemblyQualifiedAttributeTypeName.Split(new[] { ',' }, 2).Select(x => x.Trim()).ToList();
            if (parts.Count == 0)
                return null;

            if (parts.Count == 1)
                return Type.GetType(parts[0]);

            return GetType(parts[0], parts[1]);
        }

        internal static Type GetType(string typeName, string assemblyName)
        {
            // Support both long name ("assembly, version=x.x.x.x, etc.") and short name ("assembly")
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);
            if (assembly == null)
                return null;

            return assembly.GetType(typeName);
        }
    }
}