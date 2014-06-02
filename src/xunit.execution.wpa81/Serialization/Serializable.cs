using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit.Serialization;


namespace Xunit
{
    internal sealed class SerializableAttribute : Attribute
    {
    }

    internal interface ISerializable
    {
        void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
