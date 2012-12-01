namespace Xunit.Abstractions
{
    public interface ITestCollectionStarting : ITestMessage
    {
        IAssemblyInfo Assembly { get; }
        // TODO: How do we represent the collection?
    }
}
