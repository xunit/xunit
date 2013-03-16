
namespace Xunit.Abstractions
{
    public interface IFinishedMessage : ITestMessage
    {
        decimal ExecutionTime { get; }
        int TestsFailed { get; }
        int TestsRun { get; }
        int TestsSkipped { get; }
    }
}
