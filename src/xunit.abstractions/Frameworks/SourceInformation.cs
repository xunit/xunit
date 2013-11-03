using System;
using System.Runtime.Serialization;
using System.Security;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents source information about a test case.
    /// </summary>
    [Serializable]
    public class SourceInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation"/> class.
        /// </summary>
        public SourceInformation() { }

        /// <inheritdoc/>
        protected SourceInformation(SerializationInfo info, StreamingContext context)
        {
            FileName = info.GetString("FileName");
            LineNumber = (int?)info.GetValue("LineNumber", typeof(int?));
        }

        /// <summary>
        /// Gets or sets the source file name. A <c>null</c> value indicates that the
        /// source file name is not known.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the source file line. A <c>null</c> value indicates that the
        /// source file line is not known.
        /// </summary>
        public int? LineNumber { get; set; }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FileName", FileName);
            info.AddValue("LineNumber", LineNumber);
        }
    }
}