using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

public static class SerializationUtility
{
    public static T SerializeAndDeserialize<T>(T obj)
    {
        Assert.NotNull(obj);

        using (var memoryStream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();

            formatter.Serialize(memoryStream, obj);
            memoryStream.Seek(0, SeekOrigin.Begin);
            object deserialized = formatter.Deserialize(memoryStream);

            Assert.NotNull(deserialized);
            return Assert.IsType<T>(deserialized);
        }
    }
}
