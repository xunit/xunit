#if !WINDOWS_UAP
using System.Xml.Linq;
#endif

namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public static class RunSettingsHelper
    {
        /// <summary>
        /// Gets a value which indicates whether we should attempt to get source line information.
        /// </summary>
        public static bool CollectSourceInformation { get; private set; }

        /// <summary>
        /// Gets a value which indicates whether we're running in design mode inside the IDE.
        /// </summary>
        public static bool DesignMode { get; private set; }

        /// <summary>
        /// Gets a value which indicates if we should disable app domains.
        /// </summary>
        public static bool DisableAppDomain { get; private set; }

        /// <summary>
        /// Gets a value which indicates if we should disable parallelization.
        /// </summary>
        public static bool DisableParallelization { get; private set; }

        /// <summary>
        /// Gets a value which indiciates if we should disable automatic reporters.
        /// </summary>
        public static bool NoAutoReporters { get; private set; }

        /// <summary>
        /// Gets a value which indicates which reporter we should use.
        /// </summary>
        public static string ReporterSwitch { get; private set; }

        /// <summary>
        /// Gets a value which indicates the target framework the tests are being run in.
        /// </summary>
        public static string TargetFrameworkVersion { get; private set; }

        /// <summary>
        /// Reads settings for the current run from run settings xml
        /// </summary>
        /// <param name="runSettingsXml">RunSettingsXml of the run</param>
        public static void ReadRunSettings(string runSettingsXml)
        {
            // Reset to defaults
            CollectSourceInformation = true;
            DesignMode = true;
            DisableAppDomain = false;
            DisableParallelization = false;
            NoAutoReporters = false;
            ReporterSwitch = null;
            TargetFrameworkVersion = null;

#if !WINDOWS_UAP
            if (!string.IsNullOrEmpty(runSettingsXml))
            {
                try
                {
                    var element = XDocument.Parse(runSettingsXml)?.Element("RunSettings")?.Element("RunConfiguration");
                    if (element != null)
                    {
                        var disableAppDomainString = element.Element("DisableAppDomain")?.Value;
                        if (bool.TryParse(disableAppDomainString, out bool disableAppDomain))
                            DisableAppDomain = disableAppDomain;

                        var disableParallelizationString = element.Element("DisableParallelization")?.Value;
                        if (bool.TryParse(disableParallelizationString, out bool disableParallelization))
                            DisableParallelization = disableParallelization;

                        var designModeString = element.Element("DesignMode")?.Value;
                        if (bool.TryParse(designModeString, out bool designMode))
                            DesignMode = designMode;

                        var noAutoReportersString = element.Element("NoAutoReporters")?.Value;
                        if (bool.TryParse(noAutoReportersString, out bool noAutoReporters))
                            NoAutoReporters = noAutoReporters;

                        var collectSourceInformationString = element.Element("CollectSourceInformation")?.Value;
                        if (bool.TryParse(collectSourceInformationString, out bool collectSourceInformation))
                            CollectSourceInformation = collectSourceInformation;

                        ReporterSwitch = element.Element("ReporterSwitch")?.Value;
                        TargetFrameworkVersion = element.Element("TargetFrameworkVersion")?.Value;
                    }
                }
                catch { }
            }
#endif
        }
    }
}
