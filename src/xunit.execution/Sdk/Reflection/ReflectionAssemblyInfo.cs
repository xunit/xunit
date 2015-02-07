using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
#if WIN8_STORE
            try
            {
                Assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
            }
            catch (Exception)
            {
                Assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyFileName));
            }
#elif WINDOWS_PHONE_APP || WINDOWS_PHONE || ASPNET50 || ASPNETCORE50
            Assembly = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) });
#elif ANDROID
            Assembly = Assembly.Load(assemblyFileName);
#else
            Assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
#endif
        }

        /// <inheritdoc/>
        public Assembly Assembly { get; private set; }

        /// <inheritdoc/>
        public string AssemblyPath
        {
            get
            {
#if WINDOWS_PHONE_APP || WINDOWS_PHONE || ASPNET50 || ASPNETCORE50
                return Assembly.GetName().Name + ".dll"; // Return the short name on WPA81 as that's all that can be loaded
#else
                return Assembly.GetLocalCodeBase();
#endif
            }
        }

        /// <inheritdoc/>
        public string Name { get { return Assembly.FullName; } }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var attributeType = SerializationHelper.GetType(assemblyQualifiedAttributeTypeName);
            Guard.ArgumentValid("assemblyQualifiedAttributeTypeName", "Could not locate type name", attributeType != null);

            return Assembly.CustomAttributes
                           .Where(attr => attributeType.GetTypeInfo().IsAssignableFrom(attr.AttributeType.GetTypeInfo()))
                           .OrderBy(attr => attr.AttributeType.Name)
                           .Select(Reflector.Wrap)
                           .Cast<IAttributeInfo>()
                           .ToList();
        }

        /// <inheritdoc/>
        public ITypeInfo GetType(string typeName)
        {
            var type = Assembly.GetType(typeName);
            return type == null ? null : Reflector.Wrap(type);
        }

        /// <inheritdoc/>
        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            var selector = includePrivateTypes ? Assembly.DefinedTypes.Select(t => t.AsType()) : Assembly.ExportedTypes;

            try
            {
                return selector.Select(Reflector.Wrap).Cast<ITypeInfo>();
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
