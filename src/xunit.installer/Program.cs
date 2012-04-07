using System;
using System.Windows.Forms;

namespace Xunit.Installer
{
    static class Program
    {
        public static string Name
        {
            get { return "Installation Utility for xUnit.net"; }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}