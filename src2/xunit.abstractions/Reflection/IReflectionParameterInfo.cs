using System.Reflection;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a reflection-backed implementation of <see cref="IParameterInfo"/>.
    /// </summary>
    public interface IReflectionParameterInfo : IParameterInfo
    {
        /// <summary>
        /// Gets the underlying <see cref="ParameterInfo"/> for the parameter.
        /// </summary>
        ParameterInfo ParameterInfo { get; }
    }
}