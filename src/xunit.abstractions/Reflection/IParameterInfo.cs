namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents information about a method parameter. The primary implementation is based on runtime
    /// reflection, but may also be implemented by runner authors to provide non-reflection-based
    /// test discovery (for example, AST-based runners like CodeRush or Resharper).
    /// </summary>
    public interface IParameterInfo
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        ITypeInfo ParameterType { get; }
    }
}