using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

public class SerializationTests
{
    [Serializable]
    class SerializableObject { }

    [Fact(Skip = "Serialization is broken...")]
    void CanSerializeAndDeserializeObjectsInATest()
    {
        BinaryFormatter bf = new BinaryFormatter();

        using (Stream ms = new MemoryStream())
        {
            bf.Serialize(ms, new SerializableObject());
            ms.Position = 0;
            object o = bf.Deserialize(ms);

            Assert.IsType(typeof(SerializableObject), o);
            Assert.DoesNotThrow(delegate { SerializableObject o2 = (SerializableObject)o; });
        }
    }
}