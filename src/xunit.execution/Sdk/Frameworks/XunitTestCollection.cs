using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;


#if WINDOWS_PHONE_APP
using Xunit.Serialization;
#endif

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestCollection"/> that is used by xUnit.net v2.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ id = {UniqueID}, display = {DisplayName} \}")]
    public class XunitTestCollection : LongLivedMarshalByRefObject, ITestCollection, ISerializable
#if JSON
        , IGetTypeData
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCollection"/> class.
        /// </summary>
        public XunitTestCollection(ITestAssembly testAssembly, ITypeInfo collectionDefinition, string displayName)
        {
            CollectionDefinition = collectionDefinition;
            DisplayName = displayName;
            TestAssembly = testAssembly;
            UniqueID = Guid.NewGuid();
        }

        /// <inheritdoc/>
        protected XunitTestCollection(SerializationInfo info, StreamingContext context)
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
        public ITypeInfo CollectionDefinition { get; set; }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public ITestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        public Guid UniqueID { get; set; }

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

#if JSON
        public virtual void GetData(
#if !WINDOWS_PHONE_APP
Xunit.Serialization.
#endif
SerializationInfo info)
        {
            info.AddValue("DisplayName", DisplayName);
            info.AddValue("ID", ID.ToString());

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
#endif
    }
}