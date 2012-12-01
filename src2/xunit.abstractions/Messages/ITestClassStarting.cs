namespace Xunit.Abstractions
{
    public interface ITestClassStarting : ITestMessage
    {
        IAssemblyInfo Assembly { get; }
        string ClassName { get; }
    }
}
