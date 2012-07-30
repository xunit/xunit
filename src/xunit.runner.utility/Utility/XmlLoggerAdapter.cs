using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Parses the XML nodes from the version resilient runner facility and converts
    /// them into calls against the provided <see cref="IRunnerLogger"/>.
    /// </summary>
    public static class XmlLoggerAdapter
    {
        /// <summary>
        /// Logs a result XML node. Maybe be any kind of XML node.
        /// </summary>
        /// <param name="node">The node to be logged.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>Returns true if the user wishes to continue running tests; returns false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static bool LogNode(XmlNode node, IRunnerLogger logger)
        {
            Guard.ArgumentNotNull("node", node);
            Guard.ArgumentNotNull("logger", logger);

            switch (node.Name)
            {
                case "assembly":
                    LogAssemblyNode(node, logger);
                    return true;

                case "class":
                    return LogClassNode(node, logger);

                case "test":
                    return LogTestNode(node, logger);

                case "start":
                    return LogStartNode(node, logger);
            }

            return true;
        }

        /// <summary>
        /// Logs the assembly node by calling <see cref="IRunnerLogger.AssemblyFinished"/>.
        /// </summary>
        /// <param name="assemblyNode">The assembly node.</param>
        /// <param name="logger">The logger.</param>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static void LogAssemblyNode(XmlNode assemblyNode, IRunnerLogger logger)
        {
            Guard.ArgumentNotNull("assemblyNode", assemblyNode);
            Guard.ArgumentNotNull("logger", logger);

            int total = Int32.Parse(assemblyNode.Attributes["total"].Value, CultureInfo.InvariantCulture);
            int failed = Int32.Parse(assemblyNode.Attributes["failed"].Value, CultureInfo.InvariantCulture);
            int skipped = Int32.Parse(assemblyNode.Attributes["skipped"].Value, CultureInfo.InvariantCulture);
            double time = Double.Parse(assemblyNode.Attributes["time"].Value, CultureInfo.InvariantCulture);

            logger.AssemblyFinished(assemblyNode.Attributes["name"].Value, total, failed, skipped, time);
        }

        /// <summary>
        /// Logs the class node by calling <see cref="IRunnerLogger.ClassFailed"/> (if the class failed).
        /// The exception type was added in xUnit.net 1.1, so when the test assembly is linked against
        /// xUnit.net versions prior to 1.1, the exception type will be null.
        /// </summary>
        /// <param name="classNode">The class node.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>Returns true if the user wishes to continue running tests; returns false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static bool LogClassNode(XmlNode classNode, IRunnerLogger logger)
        {
            Guard.ArgumentNotNull("classNode", classNode);
            Guard.ArgumentNotNull("logger", logger);

            if (classNode.SelectSingleNode("failure") != null)
            {
                string exceptionType = null;
                XmlNode failureNode = classNode.SelectSingleNode("failure");

                // Exception type node was added in 1.1, so it might not be present
                if (failureNode.Attributes["exception-type"] != null)
                    exceptionType = failureNode.Attributes["exception-type"].Value;

                return logger.ClassFailed(classNode.Attributes["name"].Value,
                                          exceptionType,
                                          failureNode.SelectSingleNode("message").InnerText,
                                          failureNode.SelectSingleNode("stack-trace").InnerText);
            }

            return true;
        }

        /// <summary>
        /// Logs the start node by calling <see cref="IRunnerLogger.TestStart"/>. The start node was added
        /// in xUnit.net 1.1, so it will only be present when the test assembly is linked against xunit.dll
        /// version 1.1 or later.
        /// </summary>
        /// <param name="startNode">The start node.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>Returns true if the user wishes to continue running tests; returns false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static bool LogStartNode(XmlNode startNode, IRunnerLogger logger)
        {
            Guard.ArgumentNotNull("startNode", startNode);
            Guard.ArgumentNotNull("logger", logger);

            return logger.TestStart(startNode.Attributes["name"].Value,
                                    startNode.Attributes["type"].Value,
                                    startNode.Attributes["method"].Value);
        }

        /// <summary>
        /// Logs the test node by calling <see cref="IRunnerLogger.TestFinished"/>. It will also call
        /// <see cref="IRunnerLogger.TestPassed"/>, <see cref="IRunnerLogger.TestFailed"/>, or
        /// <see cref="IRunnerLogger.TestSkipped"/> as appropriate.
        /// </summary>
        /// <param name="testNode">The test node.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>Returns true if the user wishes to continue running tests; returns false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public static bool LogTestNode(XmlNode testNode, IRunnerLogger logger)
        {
            Guard.ArgumentNotNull("testNode", testNode);
            Guard.ArgumentNotNull("logger", logger);

            string name = testNode.Attributes["name"].Value;
            string type = testNode.Attributes["type"].Value;
            string method = testNode.Attributes["method"].Value;
            double time = 0;
            string output = null;
            string stackTrace = null;

            if (testNode.Attributes["time"] != null)
                time = Double.Parse(testNode.Attributes["time"].Value, CultureInfo.InvariantCulture);
            if (testNode.SelectSingleNode("output") != null)
                output = testNode.SelectSingleNode("output").InnerText;
            if (testNode.SelectSingleNode("failure/stack-trace") != null)
                stackTrace = testNode.SelectSingleNode("failure/stack-trace").InnerText;

            switch (testNode.Attributes["result"].Value)
            {
                case "Pass":
                    logger.TestPassed(name, type, method, time, output);
                    break;

                case "Fail":
                    logger.TestFailed(name, type, method, time, output,
                                      testNode.SelectSingleNode("failure").Attributes["exception-type"].Value,
                                      testNode.SelectSingleNode("failure/message").InnerText,
                                      stackTrace);
                    break;

                case "Skip":
                    logger.TestSkipped(name, type, method,
                                       testNode.SelectSingleNode("reason/message").InnerText);
                    break;
            }

            return logger.TestFinished(name, type, method);
        }
    }
}