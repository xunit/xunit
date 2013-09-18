using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    public class Xunit1AssemblyInfo : IAssemblyInfo
    {
        public Xunit1AssemblyInfo(string assemblyFileName)
        {
            AssemblyFileName = assemblyFileName;
        }

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
