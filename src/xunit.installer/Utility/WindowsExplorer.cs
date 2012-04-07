using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Xunit.Installer
{
    public class WindowsExplorer : IApplication
    {
        public bool Enableable
        {
            get { return true; }
        }

        public bool Enabled
        {
            get
            {
                using (RegistryKey classesKey = OpenClassesKey())
                using (RegistryKey extensionKey = classesKey.OpenSubKey(".xunit"))
                    return extensionKey != null;
            }
        }

        public string PreRequisites
        {
            get { return null; }
        }

        static string RunPath
        {
            get { return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath); }
        }

        public string XunitVersion
        {
            get { return null; }
        }

        public string Disable()
        {
            using (RegistryKey classesKey = OpenClassesKey())
            {
                classesKey.DeleteSubKeyTree(".xunit");
                classesKey.DeleteSubKeyTree("xUnit.net Project File");
            }

            return null;
        }

        public string Enable()
        {
            string runnerPath = Path.Combine(RunPath, "xunit.gui.exe");

            if (!File.Exists(runnerPath))
                return "Installation failed because the following file could not be found:\r\n\r\n" + runnerPath;

            using (RegistryKey classesKey = OpenClassesKey())
            {
                using (RegistryKey extensionKey = classesKey.CreateSubKey(".xunit"))
                    extensionKey.SetValue(null, "xUnit.net Project File");

                using (RegistryKey explorerKey = classesKey.CreateSubKey("xUnit.net Project File"))
                {
                    explorerKey.SetValue(null, "xUnit.net Project");

                    using (RegistryKey iconKey = explorerKey.CreateSubKey("DefaultIcon"))
                        iconKey.SetValue(null, runnerPath);

                    using (RegistryKey exeKey = explorerKey.CreateSubKey("Executable"))
                        exeKey.SetValue(null, runnerPath);

                    using (RegistryKey shellKey = explorerKey.CreateSubKey("shell"))
                    using (RegistryKey openKey = shellKey.CreateSubKey("open"))
                    using (RegistryKey commandKey = openKey.CreateSubKey("command"))
                        commandKey.SetValue(null, String.Format("\"{0}\" \"%1\"", runnerPath));
                }
            }

            // Show this here instead of return it, because we want an information icon, not an error icon.
            MessageBox.Show("You may need to log out and log back in for Windows Explorer to recognize the new settings.", Program.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        static RegistryKey OpenClassesKey()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
        }
    }
}