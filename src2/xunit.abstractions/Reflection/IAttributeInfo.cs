using System;
using System.Collections.Generic;

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
        /// Gets the arguments passed to the constructor.
        /// </summary>
        /// <returns>The constructor arguments, in order</returns>
        IEnumerable<object> GetConstructorArguments();

        /// <summary>
        /// Gets all the custom attributes for the given attribute.
        /// </summary>
        /// <param name="attributeType">The type of the attribute to find</param>
        /// <returns>The matching attributes that decorate the attribute</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType);

        /// <summary>
        /// Gets an initialized property value of the attribute.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The property value</returns>
        TValue GetPropertyValue<TValue>(string propertyName);
    }
}
