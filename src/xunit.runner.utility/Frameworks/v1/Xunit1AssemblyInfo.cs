using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyInfo"/> for xUnit.net v1.
    /// </summary>
    public class Xunit1AssemblyInfo : IAssemblyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit1AssemblyInfo" /> class.
        /// </summary>
        /// <param name="assemblyFileName">The filename of the test assembly.</param>
        public Xunit1AssemblyInfo(string assemblyFileName)
        {
            AssemblyFileName = assemblyFileName;
        }

        /// <summary>
        /// Gets the filename of the test assembly.
        /// </summary>
        public string AssemblyFileName { get; private set; }

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
            return null;
        }

        IEnumerable<ITypeInfo> IAssemblyInfo.GetTypes(bool includePrivateTypes)
        {
            return Enumerable.Empty<ITypeInfo>();
        }
    }
}
