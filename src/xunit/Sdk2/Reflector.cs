using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk2
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
        public static IReflectionAttributeInfo Wrap(Attribute attribute)
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
        /// Converts a <see cref="Type"/> into an <see cref="ITypeInfo"/> using reflection.
        /// </summary>
        /// <param name="type">The type to wrap</param>
        /// <returns>The wrapper</returns>
        public static IReflectionTypeInfo Wrap(Type type)
        {
            return new ReflectionTypeInfo(type);
        }

        class ReflectionAssemblyInfo : IReflectionAssemblyInfo
        {
            public ReflectionAssemblyInfo(Assembly assembly)
            {
                Assembly = assembly;
            }

            public Assembly Assembly { get; private set; }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return Assembly.GetCustomAttributes(attributeType, inherit: false)
                               .Cast<Attribute>()
                               .Select(Wrap);
            }

            public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
            {
                Func<Type[]> selector = includePrivateTypes ? (Func<Type[]>)Assembly.GetTypes : Assembly.GetExportedTypes;

                try
                {
                    return selector().Select(Wrap);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Select(Wrap);
                }
            }

            public override string ToString()
            {
                return Assembly.ToString();
            }
        }

        class ReflectionAttributeInfo : IReflectionAttributeInfo
        {
            public ReflectionAttributeInfo(Attribute attribute)
            {
                Attribute = attribute;
            }

            public Attribute Attribute { get; private set; }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                PropertyInfo propInfo = Attribute.GetType().GetProperty(propertyName);
                if (propInfo == null)
                    throw new ArgumentException("Could not find property " + propertyName + " on instance of " + Attribute.GetType().FullName, "propertyName");

                return (TValue)propInfo.GetValue(Attribute, new object[0]);
            }

            public override string ToString()
            {
                return Attribute.ToString();
            }
        }

        class ReflectionMethodInfo : IReflectionMethodInfo
        {
            public ReflectionMethodInfo(MethodInfo method)
            {
                MethodInfo = method;
            }

            public bool IsAbstract
            {
                get { return MethodInfo.IsAbstract; }
            }

            public bool IsStatic
            {
                get { return MethodInfo.IsStatic; }
            }

            public MethodInfo MethodInfo { get; private set; }

            public string Name
            {
                get { return MethodInfo.Name; }
            }

            public ITypeInfo ReturnType
            {
                get { return Wrap(MethodInfo.ReturnType); }
            }

            public ITypeInfo Type
            {
                get { return Wrap(MethodInfo.ReflectedType); }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return MethodInfo.GetCustomAttributes(attributeType, inherit: false)
                                 .Cast<Attribute>()
                                 .Select(Wrap);
            }

            public override string ToString()
            {
                return MethodInfo.ToString();
            }
        }

        class ReflectionTypeInfo : IReflectionTypeInfo
        {
            const BindingFlags publicBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            const BindingFlags nonPublicBindingFlags = BindingFlags.NonPublic | publicBindingFlags;

            public ReflectionTypeInfo(Type type)
            {
                Type = type;
            }

            public ITypeInfo BaseType
            {
                get { return Wrap(Type.BaseType); }
            }

            public IEnumerable<ITypeInfo> Interfaces
            {
                get { return Type.GetInterfaces().Select(Wrap); }
            }

            public bool IsAbstract
            {
                get { return Type.IsAbstract; }
            }

            public bool IsSealed
            {
                get { return Type.IsSealed; }
            }

            public string Name
            {
                get { return Type.FullName; }
            }

            public Type Type { get; private set; }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return Type.GetCustomAttributes(attributeType, inherit: true)
                           .Cast<Attribute>()
                           .Select(Wrap);
            }

            public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
            {
                return Type.GetMethods(includePrivateMethods ? nonPublicBindingFlags : publicBindingFlags).Select(Wrap);
            }

            public override string ToString()
            {
                return Type.ToString();
            }
        }
    }
}