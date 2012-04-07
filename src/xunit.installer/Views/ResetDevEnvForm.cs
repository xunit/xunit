using System.Windows.Forms;

namespace Xunit.Installer
{
    public partial class ResetDevEnvForm : Form
    {
        public ResetDevEnvForm()
        {
            InitializeComponent();
        }

        public void SetProductName(string productName)
        {
            labelPrimary.Text = labelPrimary.Text.Replace("Visual Studio", productName);
            labelSecondary.Text = labelSecondary.Text.Replace("Visual Studio", productName);
        }
    }
}
