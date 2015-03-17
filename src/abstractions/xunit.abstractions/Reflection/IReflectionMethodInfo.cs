using System.Reflection;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a reflection-backed implementation of <see cref="IMethodInfo"/>.
    /// </summary>
    public interface IReflectionMethodInfo : IMethodInfo
    {
        /// <summary>
        /// Gets the underlying <see cref="MethodInfo"/> for the method.
        /// </summary>
        MethodInfo MethodInfo { get; }
    }
}