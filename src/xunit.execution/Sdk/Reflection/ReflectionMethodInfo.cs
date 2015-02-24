using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Reflection-based implementation of <see cref="IReflectionMethodInfo"/>.
    /// </summary>
    public class ReflectionMethodInfo : LongLivedMarshalByRefObject, IReflectionMethodInfo
    {
        static readonly IEqualityComparer TypeComparer = new GenericTypeComparer();
        static readonly IEqualityComparer<IEnumerable<Type>> TypeListComparer = new AssertEqualityComparer<IEnumerable<Type>>(innerComparer: TypeComparer);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionMethodInfo"/> class.
        /// </summary>
        /// <param name="method">The method to be wrapped.</param>
        public ReflectionMethodInfo(MethodInfo method)
        {
            MethodInfo = method;
        }

        /// <inheritdoc/>
        public bool IsAbstract
        {
            get { return MethodInfo.IsAbstract; }
        }

        /// <inheritdoc/>
        public bool IsGenericMethodDefinition
        {
            get { return MethodInfo.IsGenericMethodDefinition; }
        }

        /// <inheritdoc/>
        public bool IsPublic
        {
            get { return MethodInfo.IsPublic; }
        }

        /// <inheritdoc/>
        public bool IsStatic
        {
            get { return MethodInfo.IsStatic; }
        }

        /// <inheritdoc/>
        public MethodInfo MethodInfo { get; private set; }

        /// <inheritdoc/>
        public string Name
        {
            get { return MethodInfo.Name; }
        }

        /// <inheritdoc/>
        public ITypeInfo ReturnType
        {
            get { return Reflector.Wrap(MethodInfo.ReturnType); }
        }

        /// <inheritdoc/>
        public ITypeInfo Type
        {
#if WINDOWS_PHONE_APP || ASPNETCORE50
            // WinRT/"new reflection" does not have ReflectedType on MethodInfo
            get { throw new NotSupportedException(); }
#else
            get { return Reflector.Wrap(MethodInfo.ReflectedType); }
#endif
        }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return GetCustomAttributes(MethodInfo, assemblyQualifiedAttributeTypeName).ToList();
        }

        static IEnumerable<IAttributeInfo> GetCustomAttributes(MethodInfo method, string assemblyQualifiedAttributeTypeName)
        {
            var attributeType = SerializationHelper.GetType(assemblyQualifiedAttributeTypeName);

            return GetCustomAttributes(method, attributeType, ReflectionAttributeInfo.GetAttributeUsage(attributeType));
        }

        static IEnumerable<IAttributeInfo> GetCustomAttributes(MethodInfo method, Type attributeType, AttributeUsageAttribute attributeUsage)
        {

            // TODO: Does this need to be CustomAttributeData?
            IEnumerable<IAttributeInfo> results =
                method.CustomAttributes
                      .Where(attr => attributeType.GetTypeInfo().IsAssignableFrom(attr.AttributeType.GetTypeInfo()))
                      .OrderBy(attr => attr.AttributeType.Name)
                      .Select(Reflector.Wrap)
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

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            return MethodInfo.GetGenericArguments().Select(Reflector.Wrap).ToArray();
        }

        static MethodInfo GetParent(MethodInfo method)
        {
            if (!method.IsVirtual)
                return null;

            var baseType = method.DeclaringType.GetTypeInfo().BaseType;
            if (baseType == null)
                return null;

            var methodParameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var methodGenericArgCount = method.GetGenericArguments().Length;

            return baseType.GetMatchingMethods(method)
                           .FirstOrDefault(m => m.Name == method.Name
                                             && m.GetGenericArguments().Length == methodGenericArgCount
                                             && TypeListComparer.Equals(m.GetParameters().Select(p => p.ParameterType).ToArray(), methodParameters));
        }

        /// <inheritdoc/>
        public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            var unwrapedTypeArguments = typeArguments.Select(t => ((IReflectionTypeInfo)t).Type).ToArray();
            return Reflector.Wrap(MethodInfo.MakeGenericMethod(unwrapedTypeArguments));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return MethodInfo.ToString();
        }

        /// <inheritdoc/>
        public IEnumerable<IParameterInfo> GetParameters()
        {
            return MethodInfo.GetParameters()
                             .Select(Reflector.Wrap)
                             .Cast<IParameterInfo>()
                             .ToList();
        }

        class GenericTypeComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                Type typeX = (Type)x;
                Type typeY = (Type)y;

                if (typeX.IsGenericParameter && typeY.IsGenericParameter)
                    return typeX.GenericParameterPosition == typeY.GenericParameterPosition;

                return typeX == typeY;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}