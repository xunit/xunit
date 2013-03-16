using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    // REVIEW: Once Reflector is moved here, then we can dispense with this custom type, and instead
    // make the reflected wrappers public (+ an overloaded constructor to make this class's ctor).
    public class ReflectionAssemblyInfo : LongLivedMarshalByRefObject, IAssemblyInfo
    {
        readonly IAssemblyInfo inner;

        public ReflectionAssemblyInfo(string assemblyFileName)
        {
            inner = Reflector.Wrap(Assembly.LoadFile(assemblyFileName));
        }

        /// <inheritdoc/>
        public string AssemblyPath { get { return inner.AssemblyPath; } }

        /// <inheritdoc/>
        public string Name { get { return inner.Name; } }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
        }

        /// <inheritdoc/>
        public ITypeInfo GetType(string typeName)
        {
            return inner.GetType(typeName);
        }

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            return inner.GetTypes(includePrivateTypes);
        }
    }
}