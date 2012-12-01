namespace Xunit.Abstractions
{
    public interface ITestClassFinished : IFinishedMessage
    {
        string ClassName { get; }
    }
}
