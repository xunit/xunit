namespace Xunit.Abstractions
{
    /// <summary>
    /// Interface implement by objects that want to support serialization in xUnit.net.
    /// </summary>
    public interface IXunitSerializable
    {
        /// <summary>
        /// Called when the object should populate itself with data from the serialization info.
        /// </summary>
        /// <param name="info">The info to get the data from</param>
        void Deserialize(IXunitSerializationInfo info);

        /// <summary>
        /// Called when the object should store its data into the serialization info.
        /// </summary>
        /// <param name="info">The info to store the data in</param>
        void Serialize(IXunitSerializationInfo info);
    }
}
