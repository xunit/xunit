namespace Xunit.Runner.VisualStudio
{
    /// <summary>
    /// Used to discover tests before running when VS says "run everything in the assembly".
    /// </summary>
    internal class VsExecutionDiscoveryVisitor : TestDiscoveryVisitor, IVsDiscoveryVisitor
    {
        public int Finish()
        {
            Finished.WaitOne();
            return TestCases.Count;
        }
    }
}
