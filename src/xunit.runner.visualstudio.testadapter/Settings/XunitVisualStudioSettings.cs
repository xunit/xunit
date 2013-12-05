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
        public MessageDisplay MessageDisplay { get; set; }
        public NameDisplay NameDisplay { get; set; }
        public bool ParallelizeAssemblies { get; set; }
        public bool ShutdownAfterRun { get; set; }
    }
}
