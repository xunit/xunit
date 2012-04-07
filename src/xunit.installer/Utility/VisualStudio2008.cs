using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Xunit.Installer
{
    public static class VisualStudio2008
    {
        public static string GetDevEnvPath()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\9.0"))
                return Path.Combine((string)key.GetValue("InstallDir"), @"devenv.exe");
        }

        public static string GetTestProjectTemplatePath(string language)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\9.0"))
                return Path.Combine((string)key.GetValue("InstallDir"), @"ProjectTemplates\" + language + @"\Test");
        }

        public static void ResetVisualStudio(Form owner)
        {
            string devEnvPath = GetDevEnvPath();

            using (ResetDevEnvForm form = new ResetDevEnvForm())
            {
                form.Show(owner);
                owner.Enabled = false;

                using (Process devEnv = Process.Start(devEnvPath, "/installvstemplates"))
                    while (!devEnv.WaitForExit(50))
                        Application.DoEvents();

                owner.Enabled = true;
                form.Close();
            }
        }
    }
}