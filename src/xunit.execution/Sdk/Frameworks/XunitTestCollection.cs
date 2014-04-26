using System;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestCollection"/> that is used by xUnit.net v2.
    /// </summary>
    [Serializable]
    public class XunitTestCollection : LongLivedMarshalByRefObject, ITestCollection, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCollection"/> class.
        /// </summary>
        public XunitTestCollection()
        {
            ID = Guid.NewGuid();
        }

        /// <inheritdoc/>
        protected XunitTestCollection(SerializationInfo info, StreamingContext context)
        {
            DisplayName = info.GetString("DisplayName");
            ID = Guid.Parse(info.GetString("ID"));

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
        public Guid ID { get; set; }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
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
    }
}