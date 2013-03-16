using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    // REVIEW: Once Reflector is moved here, then we can dispense with this custom type, and instead
    // make the reflected wrappers public (+ an overloaded constructor to make this class's ctor).
    public class ReflectionAssemblyInfo : LongLivedMarshalByRefObject, IAssemblyInfo
    {
        readonly IAssemblyInfo assemblyInfo;

        public ReflectionAssemblyInfo(string assemblyFileName)
        {
            var assembly = Assembly.LoadFile(assemblyFileName);
            assemblyInfo = Reflector.Wrap(assembly);
        }

        public string AssemblyPath { get { return assemblyInfo.AssemblyPath; } }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return assemblyInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
        }

        /// <inheritdoc/>
        public ITypeInfo GetType(string typeName)
        {
            return assemblyInfo.GetType(typeName);
        }

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            return assemblyInfo.GetTypes(includePrivateTypes);
        }
    }
}