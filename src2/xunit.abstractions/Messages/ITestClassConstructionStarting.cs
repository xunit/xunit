namespace Xunit.Abstractions
{
    public interface ITestClassConstructionStarting : ITestMessage
    {
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
