using System;
using System.Globalization;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Base class which contains XML manipulation helper methods
    /// </summary>
    [Serializable]
    public abstract class TestResult : ITestResult
    {
        double executionTime;

        /// <inheritdoc/>
        public double ExecutionTime
        {
            get { return executionTime; }
            set { executionTime = value; }
        }

        /// <summary>
        /// Adds the test execution time to the XML node.
        /// </summary>
        /// <param name="testNode">The XML node.</param>
        protected void AddTime(XmlNode testNode)
        {
            XmlUtility.AddAttribute(testNode, "time", ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture));
        }

        /// <inheritdoc/>
        public abstract XmlNode ToXml(XmlNode parentNode);
    }
}
