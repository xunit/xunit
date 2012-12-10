namespace Xunit.Abstractions
{
    public interface ITestClassConstructionFinished : ITestMessage
    {
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
