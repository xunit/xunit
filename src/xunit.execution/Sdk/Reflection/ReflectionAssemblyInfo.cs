using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Reflection-based implementation of <see cref="IReflectionAssemblyInfo"/>.
    /// </summary>
    public class ReflectionAssemblyInfo : LongLivedMarshalByRefObject, IReflectionAssemblyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionAssemblyInfo"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to be wrapped.</param>
        public ReflectionAssemblyInfo(Assembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionAssemblyInfo"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly to be wrapped.</param>
        public ReflectionAssemblyInfo(string assemblyFileName)
        {
#if !ANDROID
            Assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
#else
            Assembly = Assembly.Load(assemblyFileName);
#endif
        }

        /// <inheritdoc/>
        public Assembly Assembly { get; private set; }

        /// <inheritdoc/>
        public string AssemblyPath { get { return Assembly.GetLocalCodeBase(); } }

        /// <inheritdoc/>
        public string Name { get { return Assembly.FullName; } }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            Type attributeType = Type.GetType(assemblyQualifiedAttributeTypeName);
            Guard.ArgumentValid("assemblyQualifiedAttributeTypeName", "Could not locate type name", attributeType != null);

            return CustomAttributeData.GetCustomAttributes(Assembly)
                                      .Where(attr => attributeType.IsAssignableFrom(attr.Constructor.ReflectedType))
                                      .OrderBy(attr => attr.Constructor.ReflectedType.Name)
                                      .Select(Reflector.Wrap)
                                      .Cast<IAttributeInfo>()
                                      .ToList();
        }

        /// <inheritdoc/>
        public ITypeInfo GetType(string typeName)
        {
            Type type = Assembly.GetType(typeName);
            return type == null ? null : Reflector.Wrap(type);
        }

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            Func<Type[]> selector = includePrivateTypes ? (Func<Type[]>)Assembly.GetTypes : Assembly.GetExportedTypes;

            try
            {
                return selector().Select(Reflector.Wrap).Cast<ITypeInfo>();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Select(Reflector.Wrap).Cast<ITypeInfo>();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Assembly.ToString();
        }
    }
}