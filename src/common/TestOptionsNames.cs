internal static class TestOptionsNames
{
    internal static class Configuration
    {
        public const string DiagnosticMessages = "xunit.diagnosticMessages";
        public const string MaxParallelThreads = "xunit.maxParallelThreads";
        public const string MethodDisplay = "xunit.methodDisplay";
        public const string ParallelizeAssembly = "xunit.parallelizeAssembly";
        public const string ParallelizeTestCollections = "xunit.parallelizeTestCollections";
        public const string PreEnumerateTheories = "xunit.preEnumerateTheories";
    }

    internal static class Discovery
    {
        public static readonly string DiagnosticMessages = "xunit.discovery.DiagnosticMessages";
        public static readonly string MethodDisplay = "xunit.discovery.MethodDisplay";
        public static readonly string PreEnumerateTheories = "xunit.discovery.PreEnumerateTheories";
    }

    internal static class Execution
    {
        public static readonly string DiagnosticMessages = "xunit.execution.DiagnosticMessages";
        public static readonly string DisableParallelization = "xunit.execution.DisableParallelization";
        public static readonly string MaxParallelThreads = "xunit.execution.MaxParallelThreads";
        public static readonly string SynchronousMessageReporting = "xunit.execution.SynchronousMessageReporting";
    }
}