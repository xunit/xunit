using System;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a passing test result.
    /// </summary>
    [Serializable]
    public class PassedResult : MethodResult
    {
        /// <summary>
        /// Create a new instance of the <see cref="PassedResult"/> class.
        /// </summary>
        /// <param name="method">The method under test</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        public PassedResult(IMethodInfo method, string displayName)
            : base(method, displayName) { }

        /// <summary>
        /// Create a new instance of the <see cref="PassedResult"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method under test</param>
        /// <param name="typeName">The name of the type under test</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        /// <param name="traits">The custom properties attached to the test method</param>
        public PassedResult(string methodName, string typeName, string displayName, MultiValueDictionary<string, string> traits)
            : base(methodName, typeName, displayName, traits) { }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        public override XmlNode ToXml(XmlNode parentNode)
        {
            XmlNode testNode = base.ToXml(parentNode);

            XmlUtility.AddAttribute(testNode, "result", "Pass");
            AddTime(testNode);

            return testNode;
        }
    }
}