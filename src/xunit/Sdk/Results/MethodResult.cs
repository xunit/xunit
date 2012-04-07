using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents the results from running a test method
    /// </summary>
    [Serializable]
    public abstract class MethodResult : TestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodResult"/> class. The traits for
        /// the test method are discovered using reflection.
        /// </summary>
        /// <param name="method">The method under test.</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        protected MethodResult(IMethodInfo method, string displayName)
            : this(method.Name,
                   method.TypeName,
                   displayName,
                   MethodUtility.GetTraits(method)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodResult"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method under test.</param>
        /// <param name="typeName">The type of the method under test.</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        /// <param name="traits">The traits.</param>
        protected MethodResult(string methodName, string typeName, string displayName, MultiValueDictionary<string, string> traits)
        {
            MethodName = methodName;
            TypeName = typeName;
            DisplayName = displayName ?? TypeName + "." + MethodName;
            Traits = traits ?? new MultiValueDictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the display name of the method under test. This is the value that's shown
        /// during failures and in the resulting output XML.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the name of the method under test.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Gets or sets the standard output/standard error from the test that was captured
        /// while the test was running.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// Gets the traits attached to the test method.
        /// </summary>
        public MultiValueDictionary<string, string> Traits { get; private set; }

        /// <summary>
        /// Gets the name of the type under test.
        /// </summary>
        public string TypeName { get; private set; }

        void AddTraits(XmlNode testNode)
        {
            if (Traits.Count > 0)
            {
                XmlNode propertiesNode = XmlUtility.AddElement(testNode, "traits");

                Traits.ForEach((name, value) =>
                {
                    XmlNode propertyNode = XmlUtility.AddElement(propertiesNode, "trait");
                    XmlUtility.AddAttribute(propertyNode, "name", name);
                    XmlUtility.AddAttribute(propertyNode, "value", value);
                });
            }
        }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        public override XmlNode ToXml(XmlNode parentNode)
        {
            XmlNode testNode = XmlUtility.AddElement(parentNode, "test");

            XmlUtility.AddAttribute(testNode, "name", DisplayName);
            XmlUtility.AddAttribute(testNode, "type", TypeName);
            XmlUtility.AddAttribute(testNode, "method", MethodName);
            AddTraits(testNode);

            if (!String.IsNullOrEmpty(Output))
            {
                XmlNode node = XmlUtility.AddElement(testNode, "output");
                node.InnerText = Output;
            }

            return testNode;
        }
    }
}