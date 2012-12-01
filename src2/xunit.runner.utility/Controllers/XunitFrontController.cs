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
            Guard.ArgumentNotNull("assemblyFileName", assemblyFileName);

            assemblyFileName = Path.GetFullPath(assemblyFileName);
            Guard.ArgumentValid("assemblyFileName", "Could not find file: " + assemblyFileName, File.Exists(assemblyFileName));

            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            controller = factories.Select(factory => factory.Create(assemblyFileName, configFileName, shadowCopy))
                                  .FirstOrDefault(buddy => buddy != null);

            if (controller == null)
                throw new InvalidOperationException("Could not locate a controller for your unit tests. Are you missing xunit.dll or xunit2.dll?");
        }

        /// <inheritdoc/>
        public string AssemblyFileName
        {
            get { return controller.AssemblyFileName; }
        }

        /// <inheritdoc/>
        public string ConfigFileName
        {
            get { return controller.ConfigFileName; }
        }

        /// <inheritdoc/>
        public Version XunitVersion
        {
            get { return controller.XunitVersion; }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (controller != null)
                controller.Dispose();
        }

        /// <inheritdoc/>
        public void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            controller.Find(includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            controller.Find(type, includeSourceInformation, messageSink);
        }

        static string GetDefaultConfigFile(string assemblyFile)
        {
            string configFilename = assemblyFile + ".config";

            if (File.Exists(configFilename))
                return configFilename;

            return null;
        }

        /// <inheritdoc/>
        public void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            controller.Run(testMethods, messageSink);
        }
    }
}
