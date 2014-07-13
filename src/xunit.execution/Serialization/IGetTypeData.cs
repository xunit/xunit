namespace Xunit.Serialization
{
    /// <summary>
    /// Interface implement by objects that want to support custom serialization
    /// for platforms that don't support the binary serializer.
    /// </summary>
    public interface IGetTypeData
    {
        /// <summary>
        /// Gets the data from the object into the serialization info.
        /// </summary>
        /// <param name="info">The info to store the data in</param>
        void GetData(XunitSerializationInfo info);

        /// <summary>
        /// Sets the data from the serialization info into the object.
        /// </summary>
        /// <param name="info">The info to get the data from</param>
        void SetData(XunitSerializationInfo info);
    }
}
