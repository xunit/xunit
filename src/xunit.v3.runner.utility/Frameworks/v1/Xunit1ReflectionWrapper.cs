using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyInfo"/>, <see cref="ITypeInfo"/>
    /// and <see cref="IMethodInfo"/> for xUnit.net v1 tests.
    /// </summary>
    public class Xunit1ReflectionWrapper : IAssemblyInfo, ITypeInfo, IMethodInfo
    {
        static readonly Xunit1ReflectionWrapper VoidType = new Xunit1ReflectionWrapper("mscorlib.dll", "System.Void", null);

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit1ReflectionWrapper" /> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly under test.</param>
        /// <param name="typeName">The type under test.</param>
        /// <param name="methodName">The method under test.</param>
        public Xunit1ReflectionWrapper(string assemblyFileName, string typeName, string? methodName)
        {
            AssemblyFileName = assemblyFileName;
            TypeName = typeName;
            MethodName = methodName;
            UniqueID = $"{typeName}.{methodName} ({assemblyFileName})";
        }

        /// <summary>
        /// Gets the name of the assembly under test.
        /// </summary>
        public string AssemblyFileName { get; private set; }

        /// <summary>
        /// Gets the name of the method under test.
        /// </summary>
        public string? MethodName { get; private set; }

        /// <summary>
        /// Gets the name of the type under test.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the unique ID for the test.
        /// </summary>
        public string UniqueID { get; private set; }

        // IAssemblyInfo

        string IAssemblyInfo.AssemblyPath => AssemblyFileName;

        string IAssemblyInfo.Name => Path.GetFileNameWithoutExtension(AssemblyFileName);

        IEnumerable<IAttributeInfo> IAssemblyInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) =>
            Enumerable.Empty<IAttributeInfo>();

        ITypeInfo? IAssemblyInfo.GetType(string? typeName)
        {
            if (typeName == TypeName)
                return this;

            return null;
        }

        IEnumerable<ITypeInfo> IAssemblyInfo.GetTypes(bool includePrivateTypes)
        {
            yield return this;
        }

        // IMethodInfo

        bool IMethodInfo.IsAbstract => false;

        bool IMethodInfo.IsGenericMethodDefinition => false;

        bool IMethodInfo.IsPublic => true;

        bool IMethodInfo.IsStatic => false;

        string? IMethodInfo.Name => MethodName;

        ITypeInfo IMethodInfo.ReturnType => VoidType;

        ITypeInfo IMethodInfo.Type => this;

        IEnumerable<IAttributeInfo> IMethodInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) =>
            Enumerable.Empty<IAttributeInfo>();

        IEnumerable<ITypeInfo> IMethodInfo.GetGenericArguments() =>
            Enumerable.Empty<ITypeInfo>();

        IEnumerable<IParameterInfo> IMethodInfo.GetParameters() =>
            Enumerable.Empty<IParameterInfo>();

        IMethodInfo IMethodInfo.MakeGenericMethod(params ITypeInfo[] typeArguments) =>
            throw new NotImplementedException();

        // ITypeInfo

        IAssemblyInfo ITypeInfo.Assembly => this;

        ITypeInfo? ITypeInfo.BaseType => null;

        IEnumerable<ITypeInfo> ITypeInfo.Interfaces => Enumerable.Empty<ITypeInfo>();

        bool ITypeInfo.IsAbstract => false;

        bool ITypeInfo.IsGenericParameter => false;

        bool ITypeInfo.IsGenericType => false;

        bool ITypeInfo.IsSealed => false;

        bool ITypeInfo.IsValueType => false;

        string ITypeInfo.Name => TypeName;

        IEnumerable<IAttributeInfo> ITypeInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) =>
            Enumerable.Empty<IAttributeInfo>();

        IEnumerable<ITypeInfo> ITypeInfo.GetGenericArguments() =>
            Enumerable.Empty<ITypeInfo>();

        IMethodInfo ITypeInfo.GetMethod(string? methodName, bool includePrivateMethods) => this;

        IEnumerable<IMethodInfo> ITypeInfo.GetMethods(bool includePrivateMethods)
        {
            yield return this;
        }
    }
}
