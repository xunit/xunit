using System;

namespace Xunit.Runner.VisualStudio
{
    public class RunSettings
    {
        public RunSettings()
        {
            CollectSourceInformation = true;
            DesignMode = true;
            DisableAppDomain = false;
            DisableParallelization = false;
            InternalDiagnostics = false;
            NoAutoReporters = false;
            ReporterSwitch = null;
            TargetFrameworkVersion = null;
        }

        /// <summary>
        /// Gets a value which indicates whether we should attempt to get source line information.
        /// </summary>
        public bool CollectSourceInformation { get; set; }

        /// <summary>
        /// Gets a value which indicates whether we're running in design mode inside the IDE.
        /// </summary>
        public bool DesignMode { get; set; }

        /// <summary>
        /// Gets a value which indicates if we should disable app domains.
        /// </summary>
        public bool DisableAppDomain { get; set; }

        /// <summary>
        /// Gets a value which indicates if we should disable parallelization.
        /// </summary>
        public bool DisableParallelization { get; set; }

        public bool InternalDiagnostics { get; set; }

        /// <summary>
        /// Gets a value which indiciates if we should disable automatic reporters.
        /// </summary>
        public bool NoAutoReporters { get; set; }

        /// <summary>
        /// Gets a value which indicates which reporter we should use.
        /// </summary>
        public string ReporterSwitch { get; set; }

        /// <summary>
        /// Gets a value which indicates the target framework the tests are being run in.
        /// </summary>
        public string TargetFrameworkVersion { get; set; }

        /// <summary>
        /// Reads settings for the current run from run settings xml
        /// </summary>
        /// <param name="runSettingsXml">RunSettingsXml of the run</param>
        public static RunSettings Parse(string runSettingsXml)
        {
            var result = new RunSettings();

#if !WINDOWS_UAP
            if (!string.IsNullOrEmpty(runSettingsXml))
            {
                try
                {
                    var element = System.Xml.Linq.XDocument.Parse(runSettingsXml)?.Element("RunSettings")?.Element("RunConfiguration");
                    if (element != null)
                    {
                        var disableAppDomainString = element.Element("DisableAppDomain")?.Value;
                        if (bool.TryParse(disableAppDomainString, out bool disableAppDomain))
                            result.DisableAppDomain = disableAppDomain;

                        var disableParallelizationString = element.Element("DisableParallelization")?.Value;
                        if (bool.TryParse(disableParallelizationString, out bool disableParallelization))
                            result.DisableParallelization = disableParallelization;

                        var designModeString = element.Element("DesignMode")?.Value;
                        if (bool.TryParse(designModeString, out bool designMode))
                            result.DesignMode = designMode;

                        var internalDiagnosticsString = element.Element("InternalDiagnostics")?.Value;
                        if (bool.TryParse(internalDiagnosticsString, out bool internalDiagnostics))
                            result.InternalDiagnostics = internalDiagnostics;

                        var noAutoReportersString = element.Element("NoAutoReporters")?.Value;
                        if (bool.TryParse(noAutoReportersString, out bool noAutoReporters))
                            result.NoAutoReporters = noAutoReporters;

                        var collectSourceInformationString = element.Element("CollectSourceInformation")?.Value;
                        if (bool.TryParse(collectSourceInformationString, out bool collectSourceInformation))
                            result.CollectSourceInformation = collectSourceInformation;

                        result.ReporterSwitch = element.Element("ReporterSwitch")?.Value;
                        result.TargetFrameworkVersion = element.Element("TargetFrameworkVersion")?.Value;
                    }
                }
                catch { }
            }
#endif

            return result;
        }

        public bool IsMatchingTargetFramework()
        {
#if NETCOREAPP1_0
            return TargetFrameworkVersion.StartsWith(".NETCore", StringComparison.OrdinalIgnoreCase) ||
                   TargetFrameworkVersion.StartsWith("FrameworkCore", StringComparison.OrdinalIgnoreCase);
#else
            return true;
#endif
        }
    }
}
