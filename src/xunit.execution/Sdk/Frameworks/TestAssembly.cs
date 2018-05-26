using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestAssembly"/>.
    /// </summary>
    [DebuggerDisplay(@"\{ assembly = {Assembly.AssemblyPath}, config = {ConfigFileName} \}")]
    public class TestAssembly : LongLivedMarshalByRefObject, ITestAssembly
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public TestAssembly() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssembly"/> class.
        /// </summary>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="configFileName">The optional configuration filename (defaults to the
        /// configuration file of the current app domain if not provided)</param>
        /// <param name="version">The version number of the assembly (defaults to "0.0.0.0")</param>
        public TestAssembly(IAssemblyInfo assembly, string configFileName = null, Version version = null)
        {
            Guard.ArgumentNotNull("assembly", assembly);

            Assembly = assembly;
            ConfigFileName = configFileName;
            Version = version;

            if (Version == null)
            {
                var reflectionAssembly = assembly as IReflectionAssemblyInfo;
                Version = reflectionAssembly?.Assembly?.GetName()?.Version;
            }

            if (Version == null)
                Version = new Version(0, 0, 0, 0);

#if NETFRAMEWORK
            if (ConfigFileName == null)
                ConfigFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
        }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public string ConfigFileName { get; set; }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        public Version Version { get; private set; }

        /// <inheritdoc/>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("AssemblyPath", Assembly.AssemblyPath);
            info.AddValue("ConfigFileName", ConfigFileName);
            info.AddValue("Version", Version.ToString());
        }

        /// <inheritdoc/>
        public void Deserialize(IXunitSerializationInfo info)
        {
            Version = new Version(info.GetValue<string>("Version"));
            ConfigFileName = info.GetValue<string>("ConfigFileName");

            var assemblyPath = info.GetValue<string>("AssemblyPath");
            var assembly = System.Reflection.Assembly.Load(new AssemblyName
            {
                Name = Path.GetFileNameWithoutExtension(assemblyPath),
                Version = Version
            });

            Assembly = Reflector.Wrap(assembly);
        }
    }
}
