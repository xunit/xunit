using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    public class XunitFrontController : IXunitController, IDisposable
    {
        IXunitController controller;

        public XunitFrontController(string assemblyFileName, string configFileName, bool shadowCopy)
            : this(assemblyFileName, configFileName, shadowCopy, new IXunitControllerFactory[] { Xunit2Controller.Factory }) { }

        public XunitFrontController(string assemblyFileName, string configFileName, bool shadowCopy, params IXunitControllerFactory[] factories)
        {
            if (assemblyFileName == null)
                throw new ArgumentNullException("assemblyFileName");

            assemblyFileName = Path.GetFullPath(assemblyFileName);
            if (!File.Exists(assemblyFileName))
                throw new ArgumentException("Could not find file: " + assemblyFileName, "assemblyFileName");

            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            controller = factories.Select(factory => factory.Create(assemblyFileName, configFileName, shadowCopy))
                                  .FirstOrDefault(buddy => buddy != null);

            if (controller == null)
                throw new InvalidOperationException("Could not locate a controller for your unit tests. Are you missing xunit.dll or xunit2.dll?");
        }

        /// <summary>
        /// Gets the full pathname to the assembly under test.
        /// </summary>
        public string AssemblyFileName
        {
            get { return controller.AssemblyFileName; }
        }

        /// <summary>
        /// Gets the full pathname to the configuration file.
        /// </summary>
        public string ConfigFileName
        {
            get { return controller.ConfigFileName; }
        }

        /// <summary>
        /// Gets the version of xunit.dll used by the test assembly.
        /// </summary>
        public string XunitVersion
        {
            get { return controller.XunitVersion; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (controller != null)
                controller.Dispose();
        }

        /// <summary>
        /// Enumerates the tests in an assembly.
        /// </summary>
        /// <returns>The list of test cases in the test assembly.</returns>
        public IEnumerable<ITestCase> EnumerateTests()
        {
            return controller.EnumerateTests();
        }

        static string GetDefaultConfigFile(string assemblyFile)
        {
            string configFilename = assemblyFile + ".config";

            if (File.Exists(configFilename))
                return configFilename;

            return null;
        }
    }
}
