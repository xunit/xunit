namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents information about an attribute. The primary implementation is based on runtime
    /// reflection, but may also be implemented by runner authors to provide non-reflection-based
    /// test discovery (for example, AST-based runners like CodeRush or Resharper).
    /// </summary>
    public interface IAttributeInfo
    {
        /// <summary>
        /// Gets an initialized property value of the attribute.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The property value</returns>
        TValue GetPropertyValue<TValue>(string propertyName);
    }
}
