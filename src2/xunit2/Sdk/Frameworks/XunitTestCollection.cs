using System;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    [Serializable]
    public class XunitTestCollection : LongLivedMarshalByRefObject, ITestCollection, ISerializable
    {
        public XunitTestCollection() { }

        /// <inheritdoc/>
        protected XunitTestCollection(SerializationInfo info, StreamingContext context)
        {
            DisplayName = info.GetString("DisplayName");
        }

        public string DisplayName { get; set; }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("DisplayName", DisplayName);
        }
    }
}