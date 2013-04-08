using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyStarting"/>.
    /// </summary>
    public class TestAssemblyStarting : LongLivedMarshalByRefObject, ITestAssemblyStarting
    {
    }
}