using System;
using Xunit.Abstractions;
#if !ASPNETCORE50
using System.Runtime.Serialization;
#endif

#if XUNIT_CORE_DLL
using Xunit.Serialization;

namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ISourceInformation"/>.
    /// </summary>
    [Serializable]
    public class SourceInformation : LongLivedMarshalByRefObject, ISourceInformation, ISerializable
#if XUNIT_CORE_DLL
        , IGetTypeData
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation"/> class.
        /// </summary>
        public SourceInformation() { }

        /// <inheritdoc/>
        public string FileName { get; set; }

        /// <inheritdoc/>
        public int? LineNumber { get; set; }

        // -------------------- Serialization --------------------

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber, typeof(int?));
        }

        /// <summary/>
        protected SourceInformation(SerializationInfo info, StreamingContext context)
        {
            FileName = info.GetString("FileName");
            LineNumber = (int?)info.GetValue("LineNumber", typeof(int?));
        }

#if XUNIT_CORE_DLL
        /// <inheritdoc/>
        public void GetData(XunitSerializationInfo info)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber, typeof(int?));
        }

        /// <inheritdoc/>
        public void SetData(XunitSerializationInfo info)
        {
            FileName = info.GetString("FileName");
            LineNumber = (int?)info.GetValue("LineNumber", typeof(int?));
        }
#endif
    }
}