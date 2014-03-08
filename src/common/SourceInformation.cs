using System;
using System.Runtime.Serialization;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    [Serializable]
    internal class SourceInformation : LongLivedMarshalByRefObject, ISourceInformation, ISerializable
    {
        public SourceInformation() { }

        protected SourceInformation(SerializationInfo info, StreamingContext context)
        {
            FileName = info.GetString("FileName");
            LineNumber = (int?)info.GetValue("LineNumber", typeof(int?));
        }

        public string FileName { get; set; }

        public int? LineNumber { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber, typeof(int?));
        }
    }
}