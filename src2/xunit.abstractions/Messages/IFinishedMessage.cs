
namespace Xunit.Abstractions
{
    public interface IFinishedMessage : ITestMessage
    {
        IAssemblyInfo Assembly { get; }
        decimal ExecutionTime { get; }
        int TestsFailed { get; }
        int TestsRun { get; }
        int TestsSkipped { get; }
    }
}
