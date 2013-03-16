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

        private static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
        {
            return attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                                .Cast<AttributeUsageAttribute>()
                                .SingleOrDefault()
                ?? DefaultAttributeUsageAttribute;
        }

        private static IEnumerable<IAttributeInfo> GetCustomAttributes(Type type, string assemblyQualifiedAttributeTypeName)
        {
            Type attributeType = GetType(assemblyQualifiedAttributeTypeName);

            return GetCustomAttributes(type, attributeType, GetAttributeUsage(attributeType));
        }

        private static IEnumerable<IAttributeInfo> GetCustomAttributes(Type type, Type attributeType, AttributeUsageAttribute attributeUsage)
        {
            IEnumerable<IAttributeInfo> results = Enumerable.Empty<IAttributeInfo>();

            // REVIEW: 2013/02/09: Why would type ever be null?
            if (type != null)
            {
                results = CustomAttributeData.GetCustomAttributes(type)
                                             .Where(attr => attributeType.IsAssignableFrom(attr.Constructor.ReflectedType))
                                             .OrderBy(attr => attr.Constructor.ReflectedType.Name)
                                             .Select(Wrap)
                                             .Cast<IAttributeInfo>();

                if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || !results.Any()))
                    results = results.Concat(GetCustomAttributes(type.BaseType, attributeType, attributeUsage));
            }

            return results;
        }

        private static IEnumerable<IAttributeInfo> GetCustomAttributes(MethodInfo method, string assemblyQualifiedAttributeTypeName)
        {
            var attributeType = GetType(assemblyQualifiedAttributeTypeName);

            return GetCustomAttributes(method, attributeType, GetAttributeUsage(attributeType));
        }

        private static IEnumerable<IAttributeInfo> GetCustomAttributes(MethodInfo method, Type attributeType, AttributeUsageAttribute attributeUsage)
        {
            IEnumerable<IAttributeInfo> results =
                CustomAttributeData.GetCustomAttributes(method)
                                   .Where(attr => attributeType.IsAssignableFrom(attr.Constructor.ReflectedType))
                                   .OrderBy(attr => attr.Constructor.ReflectedType.Name)
                                   .Select(Wrap)
                                   .Cast<IAttributeInfo>()
                                   .ToList();

            if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || !results.Any()))
            {
                // Need to find the parent method, which may not necessarily be on the parent type

                var baseMethod = GetParent(method);
                if (baseMethod != null)
                    results = results.Concat(GetCustomAttributes(baseMethod, attributeType, attributeUsage));
            }

            return results;
        }

        private static MethodInfo GetParent(MethodInfo m)
        {
            if (!m.IsVirtual)
                return null;

            var baseType = m.DeclaringType.BaseType;
            if (baseType == null)
                return null;

            return baseType.GetMethod(m.Name, m.GetBindingFlags(), null, m.GetParameters().Select(p => p.ParameterType).ToArray(), null);
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

        class ReflectionAssemblyInfo : LongLivedMarshalByRefObject, IReflectionAssemblyInfo
        {
            public ReflectionAssemblyInfo(Assembly assembly)
            {
                Assembly = assembly;
            }

            public Assembly Assembly { get; private set; }

            public string AssemblyPath { get { return new Uri(Assembly.CodeBase).LocalPath; } }

            public string Name { get { return Assembly.FullName; } }

            /// <inheritdoc/>
            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                Type attributeType = Type.GetType(assemblyQualifiedAttributeTypeName);
                Guard.ArgumentValid("assemblyQualifiedAttributeTypeName", "Could not locate type name", attributeType != null);

                return CustomAttributeData.GetCustomAttributes(Assembly)
                                          .Where(attr => attributeType.IsAssignableFrom(attr.Constructor.ReflectedType))
                                          .OrderBy(attr => attr.Constructor.ReflectedType.Name)
                                          .Select(Wrap)
                                          .Cast<IAttributeInfo>()
                                          .ToList();
            }

            /// <inheritdoc/>
            public ITypeInfo GetType(string typeName)
            {
                Type type = Assembly.GetType(typeName);
                return type == null ? null : Wrap(type);
            }

            /// <inheritdoc/>
            public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
            {
                Func<Type[]> selector = includePrivateTypes ? (Func<Type[]>)Assembly.GetTypes : Assembly.GetExportedTypes;

                try
                {
                    return selector().Select(Wrap).Cast<ITypeInfo>();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Select(Wrap).Cast<ITypeInfo>();
                }
            }

            public override string ToString()
            {
                return Assembly.ToString();
            }
        }

        class ReflectionAttributeInfo : LongLivedMarshalByRefObject, IReflectionAttributeInfo
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
                        value = Convert(valueAsEnumeration).ToList();

                    yield return value;
                }
            }

            public IEnumerable<object> GetConstructorArguments()
            {
                return Convert(AttributeData.ConstructorArguments).ToList();
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                return Reflector.GetCustomAttributes(AttributeData.Constructor.ReflectedType, assemblyQualifiedAttributeTypeName).ToList();
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
                Attribute attribute = (Attribute)Activator.CreateInstance(attributeData.Constructor.ReflectedType, GetConstructorArguments().ToArray());

                foreach (CustomAttributeNamedArgument namedArg in attributeData.NamedArguments)
                    ((PropertyInfo)namedArg.MemberInfo).SetValue(attribute, namedArg.TypedValue.Value, index: null);

                return attribute;
            }

            public override string ToString()
            {
                return Attribute.ToString();
            }
        }

        class ReflectionMethodInfo : LongLivedMarshalByRefObject, IReflectionMethodInfo
        {
            public ReflectionMethodInfo(MethodInfo method)
            {
                MethodInfo = method;
            }

            public bool IsAbstract
            {
                get { return MethodInfo.IsAbstract; }
            }

            public bool IsPublic
            {
                get { return MethodInfo.IsPublic; }
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

            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                return Reflector.GetCustomAttributes(MethodInfo, assemblyQualifiedAttributeTypeName).ToList();
            }

            public override string ToString()
            {
                return MethodInfo.ToString();
            }

            public IEnumerable<IParameterInfo> GetParameters()
            {
                return MethodInfo.GetParameters()
                                 .Select(Wrap)
                                 .Cast<IParameterInfo>()
                                 .ToList();
            }
        }

        class ReflectionParameterInfo : LongLivedMarshalByRefObject, IReflectionParameterInfo
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

        class ReflectionTypeInfo : LongLivedMarshalByRefObject, IReflectionTypeInfo
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
                get { return Type.GetInterfaces().Select(Wrap).Cast<ITypeInfo>().ToList(); }
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

            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                return Reflector.GetCustomAttributes(Type, assemblyQualifiedAttributeTypeName).ToList();
            }

            public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
            {
                return Type.GetMethods(includePrivateMethods ? nonPublicBindingFlags : publicBindingFlags)
                           .Select(Wrap)
                           .Cast<IMethodInfo>()
                           .ToList();
            }

            public override string ToString()
            {
                return Type.ToString();
            }
        }

        static readonly AttributeUsageAttribute DefaultAttributeUsageAttribute = new AttributeUsageAttribute(AttributeTargets.All);
    }
}