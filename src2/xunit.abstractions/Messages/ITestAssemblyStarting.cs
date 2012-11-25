namespace Xunit.Abstractions
{
    public interface ITestAssemblyStarting : ITestMessage
    {
        IAssemblyInfo Assembly { get; }
    }
}
