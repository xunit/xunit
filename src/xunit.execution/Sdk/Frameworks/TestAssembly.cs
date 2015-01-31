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

#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !ASPNETCORE50
            if (ConfigFileName == null)
                ConfigFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
        }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public string ConfigFileName { get; set; }

        /// <inheritdoc/>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("AssemblyPath", Assembly.AssemblyPath);
            info.AddValue("ConfigFileName", ConfigFileName);
        }

        /// <inheritdoc/>
        public void Deserialize(IXunitSerializationInfo info)
        {
            var assemblyPath = info.GetValue<string>("AssemblyPath");
            var assembly = System.Reflection.Assembly.Load(new AssemblyName
            {
                Name = Path.GetFileNameWithoutExtension(assemblyPath)
            });

            ConfigFileName = info.GetValue<string>("ConfigFileName");
            Assembly = Reflector.Wrap(assembly);
        }
    }
}
