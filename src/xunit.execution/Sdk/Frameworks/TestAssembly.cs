using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;
using Xunit.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestAssembly"/>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ assembly = {Assembly.AssemblyPath}, config = {ConfigFileName} \}")]
    public class TestAssembly : LongLivedMarshalByRefObject, ITestAssembly, ISerializable, IGetTypeData
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", error: true)]
        public TestAssembly() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssembly"/> class.
        /// </summary>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="configFileName">The optional configuration filename (defaults to the
        /// configuration file of the current app domain if not provided)</param>
        public TestAssembly(IAssemblyInfo assembly, string configFileName = null)
        {
            Guard.ArgumentNotNull("assembly", assembly);

            ConfigFileName = configFileName;
            Assembly = assembly;

#if !WINDOWS_PHONE_APP
            if (ConfigFileName == null)
                ConfigFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
        }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public string ConfigFileName { get; set; }

        // -------------------- Serialization --------------------

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AssemblyPath", Assembly.AssemblyPath);
            info.AddValue("ConfigFileName", ConfigFileName);
        }

        /// <inheritdoc/>
        public void GetData(XunitSerializationInfo info)
        {
            info.AddValue("AssemblyPath", Assembly.AssemblyPath);
            info.AddValue("ConfigFileName", ConfigFileName);
        }

        /// <inheritdoc/>
        protected TestAssembly(SerializationInfo info, StreamingContext context)
        {
            ConfigFileName = info.GetString("ConfigFileName");

            var assemblyPath = info.GetString("AssemblyPath");

#if !WINDOWS_PHONE_APP && !WIN8_STORE
            var assembly = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .First(a => !a.IsDynamic && String.Equals(a.GetLocalCodeBase(), assemblyPath, StringComparison.OrdinalIgnoreCase));
#else
            // On WPA, this will be the assemblyname
            var assembly = System.Reflection.Assembly.Load(new AssemblyName
            {
                Name = Path.GetFileNameWithoutExtension(assemblyPath)
            });
#endif

            Assembly = Reflector.Wrap(assembly);
        }

        /// <inheritdoc/>
        public void SetData(XunitSerializationInfo info)
        {
            var assemblyPath = info.GetString("AssemblyPath");
            var assembly = System.Reflection.Assembly.Load(new AssemblyName
            {
                Name = Path.GetFileNameWithoutExtension(assemblyPath)
            });

            ConfigFileName = info.GetString("ConfigFileName");
            Assembly = Reflector.Wrap(assembly);
        }
    }
}