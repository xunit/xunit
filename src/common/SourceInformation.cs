using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
using Xunit.Sdk;

namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ISourceInformation"/>.
    /// </summary>
#if XUNIT_FRAMEWORK
    public class SourceInformation : ISourceInformation
#else
    public class SourceInformation : LongLivedMarshalByRefObject, ISourceInformation
#endif
    {
        /// <inheritdoc/>
        public string? FileName { get; set; }

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
