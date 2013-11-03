using System;
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
    public class Xunit1TestCase : ITestCase, ISerializable
    {
        static readonly IMultiValueDictionary<string, string> EmptyTraits = new MultiValueDictionary<string, string>();

        readonly Xunit1ReflectionWrapper reflectionWrapper;

        /// <summary>
        /// Initializes a new instance  of the <see cref="Xunit1TestCase"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly under test.</param>
        /// <param name="typeName">The type under test.</param>
        /// <param name="methodName">The method under test.</param>
        /// <param name="displayName">The display name of the unit test.</param>
        /// <param name="traits">The traits of the unit test.</param>
        /// <param name="skipReason">The skip reason, if the test is skipped.</param>
        public Xunit1TestCase(string assemblyFileName,
                              string typeName,
                              string methodName,
                              string displayName,
                              IMultiValueDictionary<string, string> traits = null,
                              string skipReason = null)
        {
            reflectionWrapper = new Xunit1ReflectionWrapper(assemblyFileName, typeName, methodName);

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

            DisplayName = info.GetString("DisplayName");
            SkipReason = info.GetString("SkipReason");
            SourceInformation = info.GetValue<SourceInformation>("SourceInformation");
            Traits = info.GetValue<IMultiValueDictionary<string, string>>("Traits");
        }

        /// <inheritdoc/>
        public ITypeInfo Class
        {
            get { return reflectionWrapper; }
        }

        /// <inheritdoc/>
        public string DisplayName { get; private set; }

        /// <inheritdoc/>
        public IMethodInfo Method
        {
            get { return reflectionWrapper; }
        }

        /// <inheritdoc/>
        public string SkipReason { get; private set; }

        /// <inheritdoc/>
        public SourceInformation SourceInformation { get; set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; set; }

        /// <inheritdoc/>
        public IMultiValueDictionary<string, string> Traits { get; private set; }

        /// <inheritdoc/>
        public string UniqueID
        {
            get { return reflectionWrapper.UniqueID; }
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AssemblyFileName", reflectionWrapper.AssemblyFileName);
            info.AddValue("MethodName", reflectionWrapper.MethodName);
            info.AddValue("TypeName", reflectionWrapper.TypeName);

            info.AddValue("DisplayName", DisplayName);
            info.AddValue("SkipReason", SkipReason);
            info.AddValue("SourceInformation", SourceInformation);
            info.AddValue("Traits", Traits);
        }
    }
}