using System;
using System.ComponentModel;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestCollection"/>.
    /// </summary>
    [DebuggerDisplay(@"\{ id = {UniqueID}, display = {DisplayName} \}")]
    public class TestCollection : LongLivedMarshalByRefObject, ITestCollection
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public TestCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollection"/> class.
        /// </summary>
        /// <param name="testAssembly">The test assembly the collection belongs to</param>
        /// <param name="collectionDefinition">The optional type which contains the collection definition</param>
        /// <param name="displayName">The display name for the test collection</param>
        public TestCollection(ITestAssembly testAssembly, ITypeInfo collectionDefinition, string displayName)
            : this(testAssembly, collectionDefinition, displayName, Guid.NewGuid()) { }

        internal TestCollection(ITestAssembly testAssembly, ITypeInfo collectionDefinition, string displayName, Guid uniqueId)
        {
            Guard.ArgumentNotNull("testAssembly", testAssembly);

            CollectionDefinition = collectionDefinition;
            DisplayName = displayName;
            TestAssembly = testAssembly;
            UniqueID = uniqueId;
        }

        /// <inheritdoc/>
        public ITypeInfo CollectionDefinition { get; set; }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public ITestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        public Guid UniqueID { get; set; }

        /// <inheritdoc/>
        public virtual void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("DisplayName", DisplayName);
            info.AddValue("TestAssembly", TestAssembly);
            info.AddValue("UniqueID", UniqueID.ToString());

            if (CollectionDefinition != null)
            {
                info.AddValue("DeclarationAssemblyName", CollectionDefinition.Assembly.Name);
                info.AddValue("DeclarationTypeName", CollectionDefinition.Name);
            }
            else
            {
                info.AddValue("DeclarationAssemblyName", null);
                info.AddValue("DeclarationTypeName", null);
            }
        }

        /// <inheritdoc/>
        public virtual void Deserialize(IXunitSerializationInfo info)
        {
            DisplayName = info.GetValue<string>("DisplayName");
            TestAssembly = info.GetValue<ITestAssembly>("TestAssembly");
            UniqueID = Guid.Parse(info.GetValue<string>("UniqueID"));

            var assemblyName = info.GetValue<string>("DeclarationAssemblyName");
            var typeName = info.GetValue<string>("DeclarationTypeName");

            if (!string.IsNullOrWhiteSpace(assemblyName) && !string.IsNullOrWhiteSpace(typeName))
                CollectionDefinition = Reflector.Wrap(SerializationHelper.GetType(assemblyName, typeName));
        }
    }
}
