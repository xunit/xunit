using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using Xunit.Abstractions;
using Xunit.Serialization;
#if !ASPNETCORE50
using System.Runtime.Serialization;
#endif

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestCollection"/>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ id = {UniqueID}, display = {DisplayName} \}")]
    public class TestCollection : LongLivedMarshalByRefObject, ITestCollection, ISerializable, IGetTypeData
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", error: true)]
        public TestCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollection"/> class.
        /// </summary>
        /// <param name="testAssembly">The test assembly the collection belongs to</param>
        /// <param name="collectionDefinition">The optional type which contains the collection definition</param>
        /// <param name="displayName">The display name for the test collection</param>
        public TestCollection(ITestAssembly testAssembly, ITypeInfo collectionDefinition, string displayName)
        {
            Guard.ArgumentNotNull("testAssembly", testAssembly);

            CollectionDefinition = collectionDefinition;
            DisplayName = displayName;
            TestAssembly = testAssembly;
            UniqueID = Guid.NewGuid();
        }

        /// <inheritdoc/>
        public ITypeInfo CollectionDefinition { get; set; }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public ITestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        public Guid UniqueID { get; set; }

        // -------------------- Serialization --------------------

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
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
        public void GetData(XunitSerializationInfo info)
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
        protected TestCollection(SerializationInfo info, StreamingContext context)
        {
            DisplayName = info.GetString("DisplayName");
            TestAssembly = info.GetValue<ITestAssembly>("TestAssembly");
            UniqueID = Guid.Parse(info.GetString("UniqueID"));

            var assemblyName = info.GetString("DeclarationAssemblyName");
            var typeName = info.GetString("DeclarationTypeName");

            if (!String.IsNullOrWhiteSpace(assemblyName) && String.IsNullOrWhiteSpace(typeName))
                CollectionDefinition = Reflector.Wrap(Reflector.GetType(assemblyName, typeName));
        }

        /// <inheritdoc/>
        public void SetData(XunitSerializationInfo info)
        {
            DisplayName = info.GetString("DisplayName");
            TestAssembly = info.GetValue<ITestAssembly>("TestAssembly");
            UniqueID = Guid.Parse(info.GetString("UniqueID"));

            var assemblyName = info.GetString("DeclarationAssemblyName");
            var typeName = info.GetString("DeclarationTypeName");

            if (!String.IsNullOrWhiteSpace(assemblyName) && String.IsNullOrWhiteSpace(typeName))
                CollectionDefinition = Reflector.Wrap(Reflector.GetType(assemblyName, typeName));
        }
    }
}