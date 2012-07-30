using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// XML utility methods
    /// </summary>
    public static class XmlUtility
    {
        static Regex escapeRegex = new Regex("&#x(?<char>[0-9A-Fa-f]+);", RegexOptions.Compiled);

        /// <summary>
        /// Adds an attribute to an XML node.
        /// </summary>
        /// <param name="node">The XML node.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static void AddAttribute(XmlNode node, string name, object value)
        {
            Guard.ArgumentNotNull("node", node);
            Guard.ArgumentNotNull("name", name);
            Guard.ArgumentNotNull("value", value);

            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = Escape(node.OwnerDocument, value.ToString());
            node.Attributes.Append(attr);
        }

        /// <summary>
        /// Adds a child element to an XML node.
        /// </summary>
        /// <param name="parentNode">The parent XML node.</param>
        /// <param name="name">The child element name.</param>
        /// <returns>The new child XML element.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static XmlNode AddElement(XmlNode parentNode, string name)
        {
            Guard.ArgumentNotNull("parentNode", parentNode);
            Guard.ArgumentNotNull("name", name);

            XmlNode element = parentNode.OwnerDocument.CreateElement(name);
            parentNode.AppendChild(element);
            return element;
        }

        static string Escape(XmlDocument doc, string value)
        {
            XmlNode element = doc.CreateElement("unused");
            SetInnerText(element, value);
            return element.InnerText;
        }

        /// <summary>
        /// Sets the inner text of the XML node, properly escaping it as necessary.
        /// </summary>
        /// <param name="element">The element whose inner text will be set.</param>
        /// <param name="value">The inner text to be escaped and then set.</param>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static void SetInnerText(XmlNode element, string value)
        {
            Guard.ArgumentNotNull("element", element);
            Guard.ArgumentNotNull("value", value);

            // Let .NET set the the inner text value, which will escape it (often improperly),
            // then read the value back out in escaped form via InnerXml and fix up the escaping.
            element.InnerText = value;
            element.InnerXml = escapeRegex.Replace(element.InnerXml, "\\x${char}");
        }
    }
}
