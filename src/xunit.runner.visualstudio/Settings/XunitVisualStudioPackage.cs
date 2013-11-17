using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Xunit.Runner.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideOptionPage(typeof(XunitVisualStudioSettings), "xUnit.net", "xUnit.net Settings", 115, 116, true)]
    [InstalledProductRegistration("#110", "#112", "0.99.1", IconResourceID = 400)]
    [ProvideAutoLoad(Guids.UICONTEXT_NoSolution)]
    [ProvideAutoLoad(Guids.UICONTEXT_SolutionExists)]
    [Guid(Guids.Package)]
    public sealed class XunitVisualStudioPackage : Package
    {
        static XunitVisualStudioSettings settings;

        protected override void Initialize()
        {
            base.Initialize();

            settings = GetDialogPage(typeof(XunitVisualStudioSettings)) as XunitVisualStudioSettings;
            settings.PropertyChanged += (sender, e) => SettingsProvider.Save(settings);
        }
    }
}
