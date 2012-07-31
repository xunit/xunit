using System;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a skipped test result.
    /// </summary>
    [Serializable]
    public class SkipResult : MethodResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SkipResult"/> class. Uses reflection to discover
        /// the skip reason.
        /// </summary>
        /// <param name="method">The method under test</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        /// <param name="reason">The reason the test was skipped.</param>
        public SkipResult(IMethodInfo method, string displayName, string reason)
            : base(method, displayName)
        {
            Reason = reason;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SkipResult"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method under test</param>
        /// <param name="typeName">The name of the type under test</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        /// <param name="traits">The traits attached to the method under test</param>
        /// <param name="reason">The skip reason</param>
        public SkipResult(string methodName, string typeName, string displayName, MultiValueDictionary<string, string> traits, string reason)
            : base(methodName, typeName, displayName, traits)
        {
            Reason = reason;
        }

        /// <summary>
        /// Gets the skip reason.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        public override XmlNode ToXml(XmlNode parentNode)
        {
            XmlNode testNode = base.ToXml(parentNode);

            XmlUtility.AddAttribute(testNode, "result", "Skip");

            XmlNode reasonNode = XmlUtility.AddElement(testNode, "reason");
            XmlNode messageNode = XmlUtility.AddElement(reasonNode, "message");
            XmlUtility.SetInnerText(messageNode, Reason);

            return testNode;
        }
    }
}