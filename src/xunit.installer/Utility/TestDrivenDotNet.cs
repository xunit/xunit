using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Xunit.Installer
{
    public class TestDrivenDotNet : IApplication
    {
        public bool Enableable
        {
            get
            {
                using (RegistryKey runnerKey = OpenKey())
                    return (runnerKey != null);
            }
        }

        public bool Enabled
        {
            get
            {
                using (RegistryKey runnerKey = OpenKey())
                {
                    if (runnerKey == null)
                        return false;

                    using (RegistryKey xunitKey = runnerKey.OpenSubKey("xunit"))
                        return xunitKey != null;
                }
            }
        }

        public string PreRequisites
        {
            get { return "Test Driven .NET 2.x or later"; }
        }

        static string RunPath
        {
            get { return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath); }
        }

        public string XunitVersion
        {
            get { return null; }      // Works with any version of xUnit.net
        }

        public string Disable()
        {
            try
            {
                using (RegistryKey runnerKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MutantDesign\TestDriven.NET\TestRunners", true))
                    if (runnerKey != null)
                        runnerKey.DeleteSubKeyTree("xunit");

                using (RegistryKey runnerKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MutantDesign\TestDriven.NET\TestRunners", true))
                    if (runnerKey != null)
                        runnerKey.DeleteSubKeyTree("xunit");
            }
            catch (ArgumentException) { }

            return null;
        }

        public string Enable()
        {
            using (RegistryKey runnerKey = OpenKey())
            {
                if (runnerKey == null)
                    return null;

                string xunitPath = Path.Combine(RunPath, "xunit.dll");
                string runnerPath = Path.Combine(RunPath, "xunit.runner.tdnet.dll");

                if (!File.Exists(xunitPath))
                    return "Installation failed because the following file could not be found:\r\n\r\n" + xunitPath;

                if (!File.Exists(runnerPath))
                    return "Installation failed because the following file could not be found:\r\n\r\n" + runnerPath;

                using (RegistryKey xunitKey = runnerKey.OpenSubKey("xunit", true) ?? runnerKey.CreateSubKey("xunit"))
                {
                    xunitKey.SetValue("", "4");
                    xunitKey.SetValue("AssemblyPath", runnerPath);
                    xunitKey.SetValue("TypeName", "Xunit.Runner.TdNet.TdNetRunner");
                }
            }

            return null;
        }

        static RegistryKey OpenKey()
        {
            return
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MutantDesign\TestDriven.NET\TestRunners", true) ??
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MutantDesign\TestDriven.NET\TestRunners", true);
        }
    }
}