using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Xunit.Installer
{
    public static class VisualWebDeveloper2010
    {
        public static string GetVWDExpressPath()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VWDExpress\10.0"))
                return Path.Combine((string)key.GetValue("InstallDir"), @"vwdexpress.exe");
        }

        public static string GetTestProjectTemplatePath(string language)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VWDExpress\10.0"))
                return Path.Combine((string)key.GetValue("InstallDir"), @"VWDExpress\ProjectTemplates\" + language + @"\Test");
        }

        public static void ResetVisualStudio(Form owner)
        {
            string vwdExpressPath = GetVWDExpressPath();

            using (ResetDevEnvForm form = new ResetDevEnvForm())
            {
                form.SetProductName("Visual Web Developer");
                form.Show(owner);
                owner.Enabled = false;

                using (Process vwdExpress = Process.Start(vwdExpressPath, "/installvstemplates"))
                    while (!vwdExpress.WaitForExit(50))
                        Application.DoEvents();

                owner.Enabled = true;
                form.Close();
            }
        }
    }
}