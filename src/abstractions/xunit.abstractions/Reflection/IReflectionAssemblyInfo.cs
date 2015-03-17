using System.Reflection;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a reflection-backed implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public interface IReflectionAssemblyInfo : IAssemblyInfo
    {
        /// <summary>
        /// Gets the underlying <see cref="Assembly"/> for the assembly.
        /// </summary>
        Assembly Assembly { get; }
    }
}