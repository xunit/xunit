using System;
using System.Xml.Linq;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// Represents a single report transformation from XML.
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// Gets the transform ID.
        /// </summary>
        public string ID { get; internal set; }

        /// <summary>
        /// Gets description of the transformation. Suitable for displaying to end users.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the output handler for the transformation. Converts XML to a file on the
        /// file system.
        /// </summary>
        public Action<XElement, string> OutputHandler { get; internal set; }
    }
}
