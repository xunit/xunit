// These types are mirrors of the same types in the Settings assembly, which cannot be directly
// shared because of the dependencies on types from Visual Studio.

namespace Xunit.Runner.VisualStudio.Settings
{
    public enum MessageDisplay
    {
        None = 1,
        Minimal = 2,
        Diagnostic = 3
    }

    public enum NameDisplay
    {
        Short = 1,
        Full = 2
    }

    public class XunitVisualStudioSettings
    {
        public int MaxParallelThreads { get; set; }
        public MessageDisplay MessageDisplay { get; set; }
        public NameDisplay NameDisplay { get; set; }
        public bool ParallelizeAssemblies { get; set; }
        public bool ParallelizeTestCollections { get; set; }
        public bool ShutdownAfterRun { get; set; }

        public string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName)
        {
            if (NameDisplay == NameDisplay.Full)
                return displayName;
            if (displayName == fullyQualifiedMethodName || displayName.StartsWith(fullyQualifiedMethodName + "("))
                return shortMethodName + displayName.Substring(fullyQualifiedMethodName.Length);
            return displayName;
        }

        public void Merge(XunitTestRunSettings runSettings)
        {
            if (runSettings.MaxParallelThreads.HasValue)
                MaxParallelThreads = runSettings.MaxParallelThreads.GetValueOrDefault();
            if (runSettings.MessageDisplay.HasValue)
                MessageDisplay = runSettings.MessageDisplay.GetValueOrDefault();
            if (runSettings.NameDisplay.HasValue)
                NameDisplay = runSettings.NameDisplay.GetValueOrDefault();
            if (runSettings.ParallelizeAssemblies.HasValue)
                ParallelizeAssemblies = runSettings.ParallelizeAssemblies.GetValueOrDefault();
            if (runSettings.ParallelizeTestCollections.HasValue)
                ParallelizeTestCollections = runSettings.ParallelizeTestCollections.GetValueOrDefault();
            if (runSettings.ShutdownAfterRun.HasValue)
                ShutdownAfterRun = runSettings.ShutdownAfterRun.GetValueOrDefault();
        }
    }
}
