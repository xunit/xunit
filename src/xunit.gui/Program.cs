using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Xunit.Gui
{
    static class Program
    {
        public const string REGISTRY_KEY_XUNIT = @"Software\Outercurve Foundation\xUnit.net";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RunnerForm form = null;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = e.ExceptionObject as Exception;
                string message = ex != null ? ex.Message : "Error of unknown type thrown in applicaton domain";
                ShowError(message);
                Environment.Exit(-1);
            };

            if (args.Length == 0)
                form = new RunnerForm();
            else if (args.Length == 1 && args[0] == "/?")
            {
                ShowUsage();
                return;
            }
            else if (args.Length == 1 && IsProjectFilename(args[0]))
                form = new RunnerForm(args[0]);
            else
            {
                foreach (string assemblyFilename in args)
                    if (IsProjectFilename(assemblyFilename))
                    {
                        MessageBox.Show("The xUnit.net GUI command line can only accept a list of assemblies, or a single test project file.", "xUnit.net Test Runner", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                form = new RunnerForm(args);
            }

            Application.Run(form);
        }

        static bool IsProjectFilename(string filename)
        {
            return Path.GetExtension(filename).Equals(".xunit", StringComparison.OrdinalIgnoreCase);
        }

        static void ShowError(string message)
        {
            MessageBox.Show(message, "xUnit.net Test Runner", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void ShowUsage()
        {
            string executableName = Path.GetFileNameWithoutExtension(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string usage =
@"usage: {0} <xunitProjectFile>
usage: {0} <assemblyFile> [assemblyFile...]";

            ShowError(String.Format(usage, executableName));
        }
    }
}
