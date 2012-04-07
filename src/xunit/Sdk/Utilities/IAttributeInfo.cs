using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents information about an attribute.
    /// </summary>
    public interface IAttributeInfo
    {
        /// <summary>
        /// Gets the instance of the attribute, if available.
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <returns>The instance of the attribute, if available.</returns>
        T GetInstance<T>() where T : Attribute;

        /// <summary>
        /// Gets an initialized property value of the attribute.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The property value</returns>
        TValue GetPropertyValue<TValue>(string propertyName);
    }
}