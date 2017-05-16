using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
#if NETSTANDARD1_1
            get { throw new NotSupportedException(); }
#else
            get { return Reflector.Wrap(MethodInfo.ReflectedType); }
#endif
        }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return GetCustomAttributes(MethodInfo, assemblyQualifiedAttributeTypeName).CastOrToList();
        }

        static IEnumerable<IAttributeInfo> GetCustomAttributes(MethodInfo method, string assemblyQualifiedAttributeTypeName)
        {
            var attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

            return GetCustomAttributes(method, attributeType, ReflectionAttributeInfo.GetAttributeUsage(attributeType));
        }

        static IEnumerable<IAttributeInfo> GetCustomAttributes(MethodInfo method, Type attributeType, AttributeUsageAttribute attributeUsage)
        {
            List<ReflectionAttributeInfo> list = null;
            foreach (CustomAttributeData attr in method.CustomAttributes)
            {
                if (attributeType.GetTypeInfo().IsAssignableFrom(attr.AttributeType.GetTypeInfo()))
                {
                    if (list == null)
                        list = new List<ReflectionAttributeInfo>();

                    list.Add(new ReflectionAttributeInfo(attr));
                }
            }

            if (list != null)
                list.Sort((left, right) => string.Compare(left.AttributeData.AttributeType.Name, right.AttributeData.AttributeType.Name, StringComparison.Ordinal));

            IEnumerable<IAttributeInfo> results = list ?? Enumerable.Empty<IAttributeInfo>();

            if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || list == null))
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
            return MethodInfo.GetGenericArguments().Select(t => Reflector.Wrap(t)).ToArray();
        }

        static MethodInfo GetParent(MethodInfo method)
        {
            if (!method.IsVirtual)
                return null;

            var methodParameters = method.GetParameters();
            var methodGenericArgCount = method.GetGenericArguments().Length;

            var currentType = method.DeclaringType;

            while (currentType != typeof(object))
            {
                currentType = currentType.GetTypeInfo().BaseType;
                if (currentType == null)
                    return null;

                foreach (MethodInfo m in currentType.GetMatchingMethods(method))
                {
                    if (m.Name == method.Name &&
                        m.GetGenericArguments().Length == methodGenericArgCount &&
                        ParametersHaveSameTypes(methodParameters, m.GetParameters()))
                        return m;
                }
            }

            return null;
        }

        static bool ParametersHaveSameTypes(ParameterInfo[] left, ParameterInfo[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; i++)
            {
                if (!TypeComparer.Equals(left[i].ParameterType, right[i].ParameterType))
                    return false;
            }

            return true;
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

        private IEnumerable<IParameterInfo> _cachedParameters = null;

        /// <inheritdoc/>
        public IEnumerable<IParameterInfo> GetParameters()
        {
            if (_cachedParameters == null)
            {
                ParameterInfo[] parameters = MethodInfo.GetParameters();

                IParameterInfo[] iParameters = new IParameterInfo[parameters.Length];
                for (int i = 0; i < iParameters.Length; i++)
                {
                    iParameters[i] = Reflector.Wrap(parameters[i]);
                }
                _cachedParameters = iParameters;
            }
            return _cachedParameters;
        }

        class GenericTypeComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                var typeX = (Type)x;
                var typeY = (Type)y;

                if (typeX.IsGenericParameter && typeY.IsGenericParameter)
                    return typeX.GenericParameterPosition == typeY.GenericParameterPosition;

                return typeX == typeY;
            }

            [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This class is not intended to be used in a hashed container")]
            int IEqualityComparer.GetHashCode(object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}