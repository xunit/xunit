using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestAssembly"/> that is used by xUnit.net v2.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ assembly = {Assembly.AssemblyPath}, config = {ConfigFileName} \}")]
    public class XunitTestAssembly : LongLivedMarshalByRefObject, ITestAssembly, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestAssembly"/> class.
        /// </summary>
        public XunitTestAssembly(IAssemblyInfo assembly, string configFileName)
        {
            ConfigFileName = configFileName;
            Assembly = assembly;
        }

        /// <inheritdoc/>
        protected XunitTestAssembly(SerializationInfo info, StreamingContext context)
        {
            ConfigFileName = info.GetString("ConfigFileName");

            var assemblyPath = info.GetString("AssemblyPath");
            var assembly = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .First(a => String.Equals(a.GetLocalCodeBase(), assemblyPath, StringComparison.OrdinalIgnoreCase));

            Assembly = Reflector.Wrap(assembly);
        }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public string ConfigFileName { get; set; }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AssemblyPath", Assembly.AssemblyPath);
            info.AddValue("ConfigFileName", ConfigFileName);
        }
    }
}