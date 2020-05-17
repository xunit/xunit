using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Reflection-based implementation of <see cref="IReflectionTypeInfo"/>.
    /// </summary>
    public class ReflectionTypeInfo : LongLivedMarshalByRefObject, IReflectionTypeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionTypeInfo"/> class.
        /// </summary>
        /// <param name="type">The type to wrap.</param>
        public ReflectionTypeInfo(Type type)
        {
            Type = type;
        }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly
        {
            get { return Reflector.Wrap(Type.GetTypeInfo().Assembly); }
        }

        /// <inheritdoc/>
        public ITypeInfo BaseType
        {
            get { return Reflector.Wrap(Type.GetTypeInfo().BaseType); }
        }

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> Interfaces
        {
            get { return Type.GetTypeInfo().ImplementedInterfaces.Select(i => Reflector.Wrap(i)).ToList(); }
        }

        /// <inheritdoc/>
        public bool IsAbstract
        {
            get { return Type.GetTypeInfo().IsAbstract; }
        }

        /// <inheritdoc/>
        public bool IsGenericParameter
        {
            get { return Type.IsGenericParameter; }
        }

        /// <inheritdoc/>
        public bool IsGenericType
        {
            get { return Type.GetTypeInfo().IsGenericType; }
        }

        /// <inheritdoc/>
        public bool IsSealed
        {
            get { return Type.GetTypeInfo().IsSealed; }
        }

        /// <inheritdoc/>
        public bool IsValueType
        {
            get { return Type.GetTypeInfo().IsValueType; }
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return Type.FullName ?? Type.Name; }
        }

        /// <inheritdoc/>
        public Type Type { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return ReflectionAttributeInfo.GetCustomAttributes(Type, assemblyQualifiedAttributeTypeName).CastOrToList();
        }

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            return Type.GetTypeInfo().GenericTypeArguments
                       .Select(t => Reflector.Wrap(t))
                       .ToList();
        }

        /// <inheritdoc/>
        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            var method = Type.GetRuntimeMethods()
                             .FirstOrDefault(m => (includePrivateMethod || m.IsPublic && m.DeclaringType != typeof(object)) && m.Name == methodName);
            if (method == null)
                return null;

            return Reflector.Wrap(method);
        }

        /// <inheritdoc/>
        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            var methodInfos = Type.GetRuntimeMethods();
            if (!includePrivateMethods)
            {
                methodInfos = methodInfos.Where(m => m.IsPublic);
            }
            return methodInfos.Select(m => Reflector.Wrap(m)).ToList();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
