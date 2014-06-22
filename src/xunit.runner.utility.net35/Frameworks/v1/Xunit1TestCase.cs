using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ITestCase"/> that adapts xUnit.net v1's XML-based APIs
    /// into xUnit.net v2's object-based APIs.
    /// </summary>
    [Serializable]
    public class Xunit1TestCase : ITestAssembly, ITestCollection, ITestClass, ITestMethod, ITestCase, ISerializable
    {
        static readonly Dictionary<string, List<string>> EmptyTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        readonly Xunit1ReflectionWrapper reflectionWrapper;

        /// <summary>
        /// Initializes a new instance  of the <see cref="Xunit1TestCase"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly under test.</param>
        /// <param name="configFileName">The configuration file name.</param>
        /// <param name="typeName">The type under test.</param>
        /// <param name="methodName">The method under test.</param>
        /// <param name="displayName">The display name of the unit test.</param>
        /// <param name="traits">The traits of the unit test.</param>
        /// <param name="skipReason">The skip reason, if the test is skipped.</param>
        public Xunit1TestCase(string assemblyFileName,
                              string configFileName,
                              string typeName,
                              string methodName,
                              string displayName,
                              Dictionary<string, List<string>> traits = null,
                              string skipReason = null)
        {
            reflectionWrapper = new Xunit1ReflectionWrapper(assemblyFileName, typeName, methodName);

            ConfigFileName = configFileName;
            DisplayName = displayName;
            Traits = traits ?? EmptyTraits;
            SkipReason = skipReason;
        }

        /// <inheritdoc/>
        protected Xunit1TestCase(SerializationInfo info, StreamingContext context)
        {
            reflectionWrapper = new Xunit1ReflectionWrapper(
                info.GetString("AssemblyFileName"),
                info.GetString("TypeName"),
                info.GetString("MethodName")
            );

            ConfigFileName = info.GetString("ConfigFileName");
            DisplayName = info.GetString("DisplayName");
            SkipReason = info.GetString("SkipReason");
            SourceInformation = info.GetValue<SourceInformation>("SourceInformation");

            Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var keys = info.GetValue<List<string>>("Traits.Keys");
            foreach (var key in keys)
                Traits.Add(key, info.GetValue<List<string>>(String.Format("Traits[{0}]", key)));
        }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public string SkipReason { get; set; }

        /// <inheritdoc/>
        public ISourceInformation SourceInformation { get; set; }

        /// <inheritdoc/>
        public ITestMethod TestMethod { get { return this; } }

        /// <inheritdoc/>
        public object[] TestMethodArguments { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, List<string>> Traits { get; set; }

        /// <inheritdoc/>
        public string UniqueID { get { return reflectionWrapper.UniqueID; } }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AssemblyFileName", reflectionWrapper.AssemblyFileName);
            info.AddValue("ConfigFileName", ConfigFileName);
            info.AddValue("MethodName", reflectionWrapper.MethodName);
            info.AddValue("TypeName", reflectionWrapper.TypeName);

            info.AddValue("DisplayName", DisplayName);
            info.AddValue("SkipReason", SkipReason);
            info.AddValue("SourceInformation", SourceInformation);

            info.AddValue("Traits.Keys", Traits.Keys.ToList());
            foreach (var key in Traits.Keys)
                info.AddValue(String.Format("Traits[{0}]", key), Traits[key]);
        }

        /// <inheritdoc/>
        IAssemblyInfo ITestAssembly.Assembly { get { return reflectionWrapper; } }

        /// <inheritdoc/>
        public string ConfigFileName { get; set; }

        /// <inheritdoc/>
        ITypeInfo ITestCollection.CollectionDefinition { get { return null; } }

        /// <inheritdoc/>
        string ITestCollection.DisplayName { get { return String.Format("xUnit.net v1 Tests for {0}", reflectionWrapper.AssemblyFileName); } }

        /// <inheritdoc/>
        ITestAssembly ITestCollection.TestAssembly { get { return this; } }

        /// <inheritdoc/>
        Guid ITestCollection.UniqueID { get { return Guid.Empty; } }

        /// <inheritdoc/>
        ITypeInfo ITestClass.Class { get { return reflectionWrapper; } }

        /// <inheritdoc/>
        ITestCollection ITestClass.TestCollection { get { return this; } }

        /// <inheritdoc/>
        IMethodInfo ITestMethod.Method { get { return reflectionWrapper; } }

        /// <inheritdoc/>
        ITestClass ITestMethod.TestClass { get { return this; } }
    }
}