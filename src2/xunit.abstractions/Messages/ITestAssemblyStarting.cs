namespace Xunit.Abstractions
{
    /// <summary>
    /// The ITestAssemblyMessage is sent during execution to indicate 
    /// that the for the specified assembly has begun running tests. 
    /// </summary>
    public interface ITestAssemblyStarting : ITestMessage
    {
        /// <summary>
        /// The assembly that is about to begin executing tests. 
        /// </summary>
        IAssemblyInfo Assembly { get; }
    }
}
