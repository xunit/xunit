using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// XML utility methods
    /// </summary>
    public static class XmlUtility
    {
        /// <summary>
        /// Adds an attribute to an XML node.
        /// </summary>
        /// <param name="node">The XML node.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "This parameter is verified elsewhere.")]
        public static void AddAttribute(XmlNode node, string name, object value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value.ToString();
            node.Attributes.Append(attr);
        }

        /// <summary>
        /// Adds a child element to an XML node.
        /// </summary>
        /// <param name="parentNode">The parent XML node.</param>
        /// <param name="name">The child element name.</param>
        /// <returns>The new child XML element.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static XmlNode AddElement(XmlNode parentNode, string name)
        {
            XmlNode element = parentNode.OwnerDocument.CreateElement(name);
            parentNode.AppendChild(element);
            return element;
        }
    }
}
