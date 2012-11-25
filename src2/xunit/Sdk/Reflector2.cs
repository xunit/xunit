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
    public static class Reflector2
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

        class ReflectionAssemblyInfo : IReflectionAssemblyInfo
        {
            public ReflectionAssemblyInfo(Assembly assembly)
            {
                Assembly = assembly;
            }

            public Assembly Assembly { get; private set; }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return CustomAttributeData.GetCustomAttributes(Assembly)
                                          .Where(attr => attributeType.IsAssignableFrom(attr.AttributeType))
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
            public ReflectionAttributeInfo(CustomAttributeData attribute)
            {
                AttributeData = attribute;
                Attribute = Instantiate(AttributeData);
            }

            public Attribute Attribute { get; private set; }

            public CustomAttributeData AttributeData { get; private set; }

            static IEnumerable<object> Convert(IEnumerable<CustomAttributeTypedArgument> arguments)
            {
                foreach (CustomAttributeTypedArgument argument in arguments)
                {
                    object value = argument.Value;
                    IEnumerable<CustomAttributeTypedArgument> valueAsEnumeration = value as IEnumerable<CustomAttributeTypedArgument>;
                    if (valueAsEnumeration != null)
                        value = Convert(valueAsEnumeration);

                    yield return value;
                }
            }

            public IEnumerable<object> GetConstructorArguments()
            {
                return Convert(AttributeData.ConstructorArguments);
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return CustomAttributeData.GetCustomAttributes(AttributeData.AttributeType)
                                          .Where(attr => attributeType.IsAssignableFrom(attr.AttributeType))
                                          .Select(Wrap);
            }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                PropertyInfo propInfo = Attribute.GetType().GetProperty(propertyName);
                Guard.ArgumentValid("propertyName", "Could not find property " + propertyName + " on instance of " + Attribute.GetType().FullName, propInfo != null);

                return (TValue)propInfo.GetValue(Attribute, new object[0]);
            }

            private Attribute Instantiate(CustomAttributeData attributeData)
            {
                // TODO: Guard type is correct
                Attribute attribute = (Attribute)Activator.CreateInstance(attributeData.AttributeType, GetConstructorArguments().ToArray());

                foreach (CustomAttributeNamedArgument namedArg in attributeData.NamedArguments)
                    ((PropertyInfo)namedArg.MemberInfo).SetValue(attribute, namedArg.TypedValue.Value);

                return attribute;
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
                return CustomAttributeData.GetCustomAttributes(MethodInfo)
                                          .Where(attr => attributeType.IsAssignableFrom(attr.AttributeType))
                                          .Select(Wrap);
            }

            public override string ToString()
            {
                return MethodInfo.ToString();
            }


            public IEnumerable<IParameterInfo> GetParameters()
            {
                return MethodInfo.GetParameters()
                                 .Select(Wrap);
            }
        }

        class ReflectionParameterInfo : IReflectionParameterInfo
        {
            public ReflectionParameterInfo(ParameterInfo parameterInfo)
            {
                ParameterInfo = parameterInfo;
            }

            public string Name
            {
                get { return ParameterInfo.Name; }
            }

            public ParameterInfo ParameterInfo { get; private set; }
        }

        class ReflectionTypeInfo : IReflectionTypeInfo
        {
            const BindingFlags publicBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            const BindingFlags nonPublicBindingFlags = BindingFlags.NonPublic | publicBindingFlags;

            public ReflectionTypeInfo(Type type)
            {
                Type = type;
            }

            public IAssemblyInfo Assembly
            {
                get { return Wrap(Type.Assembly); }
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
                return CustomAttributeData.GetCustomAttributes(Type)
                                          .Where(attr => attributeType.IsAssignableFrom(attr.AttributeType))
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