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
        public Xunit1ReflectionWrapper(string assemblyFileName, string typeName, string methodName)
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
        public string MethodName { get; private set; }

        /// <summary>
        /// Gets the name of the type under test.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the unique ID for the test.
        /// </summary>
        public string UniqueID { get; private set; }

        // IAssemblyInfo

        string IAssemblyInfo.AssemblyPath
        {
            get { return AssemblyFileName; }
        }

        string IAssemblyInfo.Name
        {
            get { return Path.GetFileNameWithoutExtension(AssemblyFileName); }
        }

        IEnumerable<IAttributeInfo> IAssemblyInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return Enumerable.Empty<IAttributeInfo>();
        }

        ITypeInfo IAssemblyInfo.GetType(string typeName)
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

        bool IMethodInfo.IsAbstract
        {
            get { return false; }
        }

        bool IMethodInfo.IsGenericMethodDefinition
        {
            get { return false; }
        }

        bool IMethodInfo.IsPublic
        {
            get { return true; }
        }

        bool IMethodInfo.IsStatic
        {
            get { return false; }
        }

        string IMethodInfo.Name
        {
            get { return MethodName; }
        }

        ITypeInfo IMethodInfo.ReturnType
        {
            get { return VoidType; }
        }

        ITypeInfo IMethodInfo.Type
        {
            get { return this; }
        }

        IEnumerable<IAttributeInfo> IMethodInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return Enumerable.Empty<IAttributeInfo>();
        }

        IEnumerable<ITypeInfo> IMethodInfo.GetGenericArguments()
        {
            return Enumerable.Empty<ITypeInfo>();
        }

        IEnumerable<IParameterInfo> IMethodInfo.GetParameters()
        {
            return Enumerable.Empty<IParameterInfo>();
        }

        IMethodInfo IMethodInfo.MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            throw new NotImplementedException();
        }

        // ITypeInfo

        IAssemblyInfo ITypeInfo.Assembly
        {
            get { return this; }
        }

        ITypeInfo ITypeInfo.BaseType
        {
            get { return null; }
        }

        IEnumerable<ITypeInfo> ITypeInfo.Interfaces
        {
            get { return Enumerable.Empty<ITypeInfo>(); }
        }

        bool ITypeInfo.IsAbstract
        {
            get { return false; }
        }

        bool ITypeInfo.IsGenericParameter
        {
            get { return false; }
        }

        bool ITypeInfo.IsGenericType
        {
            get { return false; }
        }

        bool ITypeInfo.IsSealed
        {
            get { return false; }
        }

        bool ITypeInfo.IsValueType
        {
            get { return false; }
        }

        string ITypeInfo.Name
        {
            get { return TypeName; }
        }

        IEnumerable<IAttributeInfo> ITypeInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return Enumerable.Empty<IAttributeInfo>();
        }

        IEnumerable<ITypeInfo> ITypeInfo.GetGenericArguments()
        {
            return Enumerable.Empty<ITypeInfo>();
        }

        IMethodInfo ITypeInfo.GetMethod(string methodName, bool includePrivateMethods)
        {
            return this;
        }

        IEnumerable<IMethodInfo> ITypeInfo.GetMethods(bool includePrivateMethods)
        {
            yield return this;
        }
    }
}
