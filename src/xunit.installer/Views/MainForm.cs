using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Xunit.Installer
{
    public partial class MainForm : Form
    {
        IApplication appTestDrivenDotNet = new TestDrivenDotNet();
        IApplication appWindowsExplorer = new WindowsExplorer();
        IApplication appMVC1VS2008 = new MVC1_VS2008();
        IApplication appMVC2VS2008 = new MVC2_VS2008();
        IApplication appMVC2VS2010 = new MVC2_VS2010();
        IApplication appMVC2VWD2010 = new MVC2_VWD2010();
        IApplication appMVC3VS2010 = new MVC3_VS2010();
        IApplication appMVC3VWD2010 = new MVC3_VWD2010();

        public MainForm()
        {
            InitializeComponent();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            Text = Program.Name + " (build " + Assembly.GetExecutingAssembly().GetName().Version + ")";
            statusToolTip.ToolTipTitle = Text;
            UpdateUI();
        }

        void UpdateApp(IApplication application, CheckBox chkEnable)
        {
            bool unused = false;
            UpdateApp(application, chkEnable, ref unused);
        }

        void UpdateApp(IApplication application, CheckBox chkEnable, ref bool updated)
        {
            if (!application.Enableable)
                return;

            string result = null;

            if (chkEnable.Checked && !application.Enabled)
            {
                updated = true;
                result = application.Enable();
            }
            else if (!chkEnable.Checked && application.Enabled)
            {
                updated = true;
                result = application.Disable();
            }

            if (!String.IsNullOrEmpty(result))
                MessageBox.Show(result, Program.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void UpdateUI()
        {
            UpdateUI(lblTestDrivenDotNet, chkTestDrivenDotNet, appTestDrivenDotNet);

            UpdateUI(lblWindowsExplorer, chkWindowsExplorer, appWindowsExplorer);

            UpdateUI(lblMVC1VS2008, chkMVC1VS2008, appMVC1VS2008);

            UpdateUI(lblMVC2VS2008, chkMVC2VS2008, appMVC2VS2008);
            UpdateUI(lblMVC2VS2010, chkMVC2VS2010, appMVC2VS2010);
            UpdateUI(lblMVC2VWD2010, chkMVC2VWD2010, appMVC2VWD2010);

            UpdateUI(lblMVC3VS2010, chkMVC3VS2010, appMVC3VS2010);
            UpdateUI(lblMVC3VWD2010, chkMVC3VWD2010, appMVC3VWD2010);
        }

        void UpdateUI(Label statusLabel, CheckBox installCheckbox, IApplication application)
        {
            string statusText;
            string tooltipText;
            Color color;

            if (!application.Enableable)
            {
                color = Color.DarkRed;
                tooltipText = "This feature is not available because one or more of the pre-requisites\r\nis not installed. Please verify that all of the following are installed:\r\n\r\n" + application.PreRequisites;
                statusText = "Not available";
                installCheckbox.Checked = false;
                installCheckbox.Enabled = false;
            }
            else if (application.Enabled)
            {
                string xunitVersion = application.XunitVersion;

                installCheckbox.Checked = true;
                color = Color.Green;
                tooltipText = "This is currently installed. Uncheck the box and click Apply to uninstall.";
                statusText = "Installed";
                if (xunitVersion != null)
                    statusText += " (" + xunitVersion + ")";

            }
            else
            {
                installCheckbox.Checked = false;
                color = Color.DarkGoldenrod;
                tooltipText = "This is not currently installed. Check the box and click Apply to install.";
                statusText = "Not installed";
            }

            statusLabel.ForeColor = color;
            statusLabel.Text = statusText;
            statusToolTip.SetToolTip(statusLabel, tooltipText);
        }

        // Event handlers

        void btnApply_Click(object sender, EventArgs e)
        {
            bool resetVS2008 = false;
            bool resetVS2010 = false;
            bool resetVWD2010 = false;

            UpdateApp(appTestDrivenDotNet, chkTestDrivenDotNet);

            UpdateApp(appWindowsExplorer, chkWindowsExplorer);

            UpdateApp(appMVC1VS2008, chkMVC1VS2008, ref resetVS2008);

            UpdateApp(appMVC2VS2008, chkMVC2VS2008, ref resetVS2008);
            UpdateApp(appMVC2VS2010, chkMVC2VS2010, ref resetVS2010);
            UpdateApp(appMVC2VWD2010, chkMVC2VWD2010, ref resetVWD2010);

            UpdateApp(appMVC3VS2010, chkMVC3VS2010, ref resetVS2010);
            UpdateApp(appMVC3VWD2010, chkMVC3VWD2010, ref resetVWD2010);

            if (resetVS2008)
                VisualStudio2008.ResetVisualStudio(this);
            if (resetVS2010)
                VisualStudio2010.ResetVisualStudio(this);
            if (resetVWD2010)
                VisualWebDeveloper2010.ResetVisualStudio(this);

            UpdateUI();
        }

        void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        void btnReset_Click(object sender, EventArgs e)
        {
            UpdateUI();
        }
    }
}