using Xunit.Abstractions;
using Xunit.Sdk;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ISourceInformation"/>.
    /// </summary>
    public class SourceInformation : LongLivedMarshalByRefObject, ISourceInformation
    {
        /// <inheritdoc/>
        public string FileName { get; set; }

        /// <inheritdoc/>
        public int? LineNumber { get; set; }

        /// <inheritdoc/>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber);
        }

        /// <inheritdoc/>
        public void Deserialize(IXunitSerializationInfo info)
        {
            FileName = info.GetValue<string>("FileName");
            LineNumber = info.GetValue<int?>("LineNumber");
        }
    }
}
