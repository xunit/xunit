using System;
using System.IO;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Contains the test results from an assembly.
    /// </summary>
    [Serializable]
    public class AssemblyResult : CompositeResult
    {
        static string environment = String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version);
        static string testFramework = String.Format("xUnit.net {0}", typeof(AssemblyResult).Assembly.GetName().Version);

        /// <summary>
        /// Creates a new instance of the <see cref="AssemblyResult"/> class.
        /// </summary>
        /// <param name="assemblyFilename">The filename of the assembly</param>
        public AssemblyResult(string assemblyFilename) : this(assemblyFilename, null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="AssemblyResult"/> class.
        /// </summary>
        /// <param name="assemblyFilename">The filename of the assembly</param>
        /// <param name="configFilename">The configuration filename</param>
        public AssemblyResult(string assemblyFilename, string configFilename)
        {
            Filename = Path.GetFullPath(assemblyFilename);

            if (configFilename != null)
                ConfigFilename = Path.GetFullPath(configFilename);
        }

        /// <summary>
        /// Gets the fully qualified filename of the configuration file.
        /// </summary>
        public string ConfigFilename { get; private set; }

        /// <summary>
        /// Gets the directory where the assembly resides.
        /// </summary>
        public string Directory
        {
            get { return Path.GetDirectoryName(Filename); }
        }

        /// <summary>
        /// Gets the number of failed results.
        /// </summary>
        public int FailCount { get; private set; }

        /// <summary>
        /// Gets the fully qualified filename of the assembly.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the number of passed results.
        /// </summary>
        public int PassCount { get; private set; }

        /// <summary>
        /// Gets the number of skipped results.
        /// </summary>
        public int SkipCount { get; private set; }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        public override XmlNode ToXml(XmlNode parentNode)
        {
            XmlNode assemblyNode = XmlUtility.AddElement(parentNode, "assembly");
            XmlUtility.AddAttribute(assemblyNode, "name", Filename);
            XmlUtility.AddAttribute(assemblyNode, "run-date", DateTime.Now.ToString("yyyy-MM-dd"));
            XmlUtility.AddAttribute(assemblyNode, "run-time", DateTime.Now.ToString("HH:mm:ss"));

            if (ConfigFilename != null)
                XmlUtility.AddAttribute(assemblyNode, "configFile", ConfigFilename);

            foreach (ClassResult child in Results)
            {
                child.ToXml(assemblyNode); // Must call so that values get computed

                PassCount += child.PassCount;
                FailCount += child.FailCount;
                SkipCount += child.SkipCount;
                ExecutionTime += child.ExecutionTime;
            }

            AddTime(assemblyNode);
            XmlUtility.AddAttribute(assemblyNode, "total", PassCount + FailCount + SkipCount);
            XmlUtility.AddAttribute(assemblyNode, "passed", PassCount);
            XmlUtility.AddAttribute(assemblyNode, "failed", FailCount);
            XmlUtility.AddAttribute(assemblyNode, "skipped", SkipCount);
            XmlUtility.AddAttribute(assemblyNode, "environment", environment);
            XmlUtility.AddAttribute(assemblyNode, "test-framework", testFramework);

            return assemblyNode;
        }
    }
}