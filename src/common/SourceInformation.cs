using System;
using System.Runtime.Serialization;
using Xunit.Abstractions;

#if WINDOWS_PHONE_APP
using Xunit.Serialization;
#endif

#if XUNIT_CORE_DLL
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
#if JSON
, IGetTypeData
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation"/> class.
        /// </summary>
        public SourceInformation() { }

        /// <summary/>
        protected SourceInformation(SerializationInfo info, StreamingContext context)
        {
            FileName = info.GetString("FileName");
            LineNumber = (int?)info.GetValue("LineNumber", typeof(int?));
        }

        /// <inheritdoc/>
        public string FileName { get; set; }

        /// <inheritdoc/>
        public int? LineNumber { get; set; }

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber, typeof(int?));
        }

#if JSON
        public virtual void GetData(Xunit.Serialization.SerializationInfo info)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber, typeof(int?));
        }
#endif
    }
}