namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a test assembly.
    /// </summary>
    public interface ITestAssembly : IXunitSerializable
    {
        /// <summary>
        /// Gets the assembly that this test assembly belongs to.
        /// </summary>
        IAssemblyInfo Assembly { get; }

        /// <summary>
        /// Gets the full path of the configuration file name, if one is present.
        /// May be <c>null</c> if there is no configuration file.
        /// </summary>
        string ConfigFileName { get; }
    }
}